using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Interaction
{
    using Scripts.Chip;
    using Scripts.Core;
    using Scripts.Graphics;

    public class ChipInteraction : Interactable
    {
        public enum State
        {
            None,
            PlacingNewChips,
            MovingOldChips,
            SelectingChips,
            PasteNewChips
        }
        public event System.Action<Chip> OnDeleteChip;

        public BoxCollider2D ChipArea;
        public Transform ChipHolder;
        public LayerMask ChipMask;
        public Material SelectionBoxMaterial;
        public Color SelectionBoxCol;
        public Color InvalidPlacementCol;

        private const float _dragDepth = -50;
        private const float _chipDepth = -0.2f;

        public List<Chip> AllChips { get; private set; }

        private State _currentState;
        private List<Chip> _newChipsToPlace;
        private List<KeyValuePair<Chip, Vector3>> _newChipsToPaste;
        public static List<Chip> s_SelectedChips;
        public List<Chip> SelectedChips => s_SelectedChips;
        private Vector2 selectionBoxStartPos;
        private Mesh _selectionMesh;
        private Vector3[] _selectedChipsOriginalPos;

        [HideInInspector]
        public List<Pin> VisiblePins;

        private List<Chip> _chipsToPaste;

        private void Awake()
        {
            _newChipsToPlace = new List<Chip>();
            _newChipsToPaste = new List<KeyValuePair<Chip, Vector3>>();
            _chipsToPaste = new List<Chip>();
            s_SelectedChips = new List<Chip>();
            AllChips = new List<Chip>();
            VisiblePins = new List<Pin>();
            MeshShapeCreator.CreateQuadMesh(ref _selectionMesh);
        }

        public override void OrderedUpdate()
        {
            switch (_currentState)
            {
                case State.None:
                    HandleSelection();
                    break;
                case State.PlacingNewChips:
                    HandleNewChipPlacement();
                    break;
                case State.PasteNewChips:
                    HandlePasteChipPlacement();
                    break;
                case State.SelectingChips:
                    HandleSelectionBox();
                    break;
                case State.MovingOldChips:
                    HandleChipMovement();
                    break;
            }
            DrawSelectedChipBounds();
        }

        public Pin[] UnconnectedInputPins
        {
            get
            {
                List<Pin> unconnected = new List<Pin>();
                foreach (Chip chip in AllChips)
                {
                    foreach (Pin pin in chip.InputPins)
                    {
                        if (pin.WType == Pin.WireType.Simple && !pin.HasParent)
                        {
                            unconnected.Add(pin);
                        }
                    }
                }
                return unconnected.ToArray();
            }
        }

        public Pin[] UnconnectedOutputPins
        {
            get
            {
                List<Pin> unconnected = new List<Pin>();
                foreach (Chip chip in AllChips)
                {
                    foreach (Pin pin in chip.OutputPins)
                    {
                        if (pin.ChildPins.Count == 0)
                        {
                            unconnected.Add(pin);
                        }
                    }
                }
                return unconnected.ToArray();
            }
        }

        public void LoadChip(Chip chip)
        {
            chip.transform.parent = ChipHolder;
            AllChips.Add(chip);
            VisiblePins.AddRange(chip.InputPins);
            VisiblePins.AddRange(chip.OutputPins);
            foreach (Pin pin in chip.OutputPins)
            {
                pin.UpdateColor();
            }
        }

        public List<Chip> PasteChips(List<KeyValuePair<Chip, Vector3>> clipboard)
        {
            _currentState = State.PasteNewChips;
            if (_newChipsToPaste.Count == 0)
                s_SelectedChips.Clear();
            // newChipsToPaste.Clear();
            // chipsToPaste.Clear();

            foreach (KeyValuePair<Chip, Vector3> clipboardItem in clipboard)
            {
                var newChip = Instantiate(clipboardItem.Key, clipboardItem.Value, Quaternion.identity);
                newChip.transform.SetParent(ChipHolder);
                newChip.gameObject.SetActive(true);
                newChip.GetComponent<ChipPackage>().SetSizeAndSpacing(newChip);
                s_SelectedChips.Add(newChip);
                _newChipsToPaste.Add(
                    new KeyValuePair<Chip, Vector3>(newChip, clipboardItem.Value));
                _chipsToPaste.Add(newChip);
            }
            return _chipsToPaste;
        }

        public void ChipButtonInteraction(Chip chip)
        {
            if (RequestFocus())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    // Spawn chip
                    _currentState = State.PlacingNewChips;
                    if (_newChipsToPlace.Count == 0)
                    {
                        s_SelectedChips.Clear();
                    }
                    var newChip = Instantiate(chip, parent: ChipHolder);
                    newChip.gameObject.SetActive(true);
                    newChip.GetComponent<ChipPackage>().SetSizeAndSpacing(newChip);
                    s_SelectedChips.Add(newChip);
                    _newChipsToPlace.Add(newChip);
                }
                else if (Input.GetMouseButtonDown(1) && chip.Editable)
                {
                    UI.UIManager.Instance.OpenMenu(UI.MenuType.EditChipMenu);
                }
            }
        }

        private void HandleSelection()
        {
            Vector2 mousePos = InputHelper.MouseWorldPos;

            // Left mouse down. Handle selecting a chip, or starting to draw a selection
            // box.
            if (Input.GetMouseButtonDown(0) && !InputHelper.MouseOverUIObject())
            {
                if (RequestFocus())
                {
                    selectionBoxStartPos = mousePos;
                    GameObject objectUnderMouse =
                        InputHelper.GetObjectUnderMouse2D(ChipMask);

                    // If clicked on nothing, clear selected items and start drawing
                    // selection box
                    if (objectUnderMouse == null)
                    {
                        _currentState = State.SelectingChips;
                        s_SelectedChips.Clear();
                    }
                    // If clicked on a chip, select that chip and allow it to be moved
                    else
                    {
                        _currentState = State.MovingOldChips;
                        Chip chipUnderMouse = objectUnderMouse.GetComponent<Chip>();
                        // If object is already selected, then selection of any other chips
                        // should be maintained so they can be moved as a group. But if object
                        // is not already selected, then any currently selected chips should
                        // be deselected.
                        if (!s_SelectedChips.Contains(chipUnderMouse))
                        {
                            s_SelectedChips.Clear();
                            s_SelectedChips.Add(chipUnderMouse);
                        }
                        // Record starting positions of all selected chips for movement
                        _selectedChipsOriginalPos = new Vector3[s_SelectedChips.Count];
                        for (int i = 0; i < s_SelectedChips.Count; i++)
                        {
                            _selectedChipsOriginalPos[i] = s_SelectedChips[i].transform.position;
                        }
                    }
                }
            }
        }

        public void DeleteChip(Chip chip)
        {
            OnDeleteChip?.Invoke(chip);
            AllChips.Remove(chip);

            foreach (Pin pin in chip.InputPins)
                VisiblePins.Remove(pin);
            foreach (Pin pin in chip.OutputPins)
                VisiblePins.Remove(pin);

            Destroy(chip.gameObject);
        }

        private void HandleSelectionBox()
        {
            Vector2 mousePos = InputHelper.MouseWorldPos;
            // While holding mouse down, keep drawing selection box
            if (Input.GetMouseButton(0))
            {
                var pos =
                    (Vector3)(selectionBoxStartPos + mousePos) / 2 + Vector3.back * 0.5f;
                var scale =
                    new Vector3(Mathf.Abs(mousePos.x - selectionBoxStartPos.x),
                                Mathf.Abs(mousePos.y - selectionBoxStartPos.y), 1);
                SelectionBoxMaterial.color = SelectionBoxCol;
                UnityEngine.Graphics.DrawMesh(_selectionMesh,
                                Matrix4x4.TRS(pos, Quaternion.identity, scale),
                                SelectionBoxMaterial, 0);
            }
            // Mouse released, so selected all chips inside the selection box
            if (Input.GetMouseButtonUp(0))
            {
                _currentState = State.None;

                // Select all objects under selection box
                Vector2 boxSize =
                    new Vector2(Mathf.Abs(mousePos.x - selectionBoxStartPos.x),
                                Mathf.Abs(mousePos.y - selectionBoxStartPos.y));
                var allObjectsInBox = Physics2D.OverlapBoxAll(
                    (selectionBoxStartPos + mousePos) / 2, boxSize, 0, ChipMask);
                s_SelectedChips.Clear();
                foreach (var item in allObjectsInBox)
                {
                    if (item.GetComponent<Chip>())
                    {
                        s_SelectedChips.Add(item.GetComponent<Chip>());
                    }
                }
            }
        }

        private void HandleChipMovement()
        {
            var mousePos = InputHelper.MouseWorldPos;

            if (Input.GetMouseButton(0))
            {
                // Move selected objects
                Vector2 deltaMouse = mousePos - selectionBoxStartPos;
                for (int i = 0; i < s_SelectedChips.Count; i++)
                {
                    s_SelectedChips[i].transform.position =
                        (Vector2)_selectedChipsOriginalPos[i] + deltaMouse;
                    SetDepth(s_SelectedChips[i], _dragDepth + _selectedChipsOriginalPos[i].z);
                }
            }
            // Mouse released, so stop moving chips
            if (Input.GetMouseButtonUp(0))
            {
                _currentState = State.None;

                if (s_SelectedChipsWithinPlacementArea())
                {
                    const float chipMoveThreshold = 0.001f;
                    Vector2 deltaMouse = mousePos - selectionBoxStartPos;

                    // If didn't end up moving the chips, then select just the one under the
                    // mouse
                    if (s_SelectedChips.Count > 1 &&
                        deltaMouse.magnitude < chipMoveThreshold)
                    {
                        var objectUnderMouse = InputHelper.GetObjectUnderMouse2D(ChipMask);
                        if (objectUnderMouse?.GetComponent<Chip>())
                        {
                            s_SelectedChips.Clear();
                            s_SelectedChips.Add(objectUnderMouse.GetComponent<Chip>());
                        }
                    }
                    else
                    {
                        for (int i = 0; i < s_SelectedChips.Count; i++)
                        {
                            SetDepth(s_SelectedChips[i], _selectedChipsOriginalPos[i].z);
                        }
                    }
                }
                // If any chip ended up outside of placement area, then put all chips back
                // to their original positions
                else
                {
                    for (int i = 0; i < _selectedChipsOriginalPos.Length; i++)
                    {
                        s_SelectedChips[i].transform.position = _selectedChipsOriginalPos[i];
                    }
                }
            }
        }

        // Handle placement of newly spawned chips
        private void HandleNewChipPlacement()
        {
            // Cancel placement if esc or right mouse down
            if (InputHelper.AnyOfTheseKeysDown(KeyCode.Escape, KeyCode.Backspace, KeyCode.Delete) || Input.GetMouseButtonDown(1))
            {
                CancelPlacement(_newChipsToPlace.ToArray());
                _newChipsToPlace.Clear();
            }
            // Move selected chip/s and place them on left mouse down
            else
            {
                Vector2 mousePos = InputHelper.MouseWorldPos;
                float offsetY = 0;

                for (int i = 0; i < _newChipsToPlace.Count; i++)
                {
                    Chip chipToPlace = _newChipsToPlace[i];
                    chipToPlace.transform.position = mousePos + Vector2.down * offsetY;
                    SetDepth(chipToPlace, _dragDepth);
                    offsetY += chipToPlace.BoundsSize.y + ScalingManager.ChipStackSpace;
                }

                // Place object
                if (Input.GetMouseButtonDown(0) && s_SelectedChipsWithinPlacementArea() &&
                    !InputHelper.MouseOverUIObject())
                {
                    PlaceNewChips(_newChipsToPlace.ToArray());
                    _newChipsToPlace.Clear();
                }
            }
        }

        private void HandlePasteChipPlacement()
        {
            // Cancel placement if esc or right mouse down
            if (InputHelper.AnyOfTheseKeysDown(KeyCode.Escape, KeyCode.Backspace,
                                            KeyCode.Delete) ||
                Input.GetMouseButtonDown(1))
            {
                CancelPlacement(_chipsToPaste.ToArray());
                _newChipsToPaste.Clear();
                _chipsToPaste.Clear();
            }
            // Move selected chip/s and place them on left mouse down
            else
            {
                Vector3 mousePos = new Vector3(InputHelper.MouseWorldPos.x,
                                            InputHelper.MouseWorldPos.y, 0);

                foreach (KeyValuePair<Chip, Vector3> chipToPaste in _newChipsToPaste)
                {
                    chipToPaste.Key.transform.position = chipToPaste.Value + mousePos;
                    SetDepth(chipToPaste.Key, _dragDepth);
                }

                // Place object
                if (Input.GetMouseButtonDown(0) && s_SelectedChipsWithinPlacementArea() &&
                    !InputHelper.MouseOverUIObject())
                {
                    PlaceNewChips(_chipsToPaste.ToArray());
                    _newChipsToPaste.Clear();
                    _chipsToPaste.Clear();
                }
            }
        }

        private void PlaceNewChips(Chip[] chipsToPlace)
        {
            float startDepth = (AllChips.Count > 0)
                                ? AllChips[AllChips.Count - 1].transform.position.z
                                : 0;
            for (int i = 0; i < chipsToPlace.Length; i++)
            {
                SetDepth(chipsToPlace[i],
                        startDepth + (_newChipsToPlace.Count - i) * _chipDepth);
            }

            AllChips.AddRange(chipsToPlace);
            foreach (Chip chip in chipsToPlace)
            {
                VisiblePins.AddRange(chip.InputPins);
                VisiblePins.AddRange(chip.OutputPins);
                foreach (Pin pin in chip.OutputPins)
                {
                    pin.UpdateColor();
                }
            }

            s_SelectedChips.Clear();
            _currentState = State.None;
        }

        private void CancelPlacement(Chip[] chipsToPlace)
        {
            for (int i = chipsToPlace.Length - 1; i >= 0; i--)
            {
                Destroy(chipsToPlace[i].gameObject);
            }
            s_SelectedChips.Clear();
            _currentState = State.None;
        }

        private void DrawSelectedChipBounds()
        {

            if (s_SelectedChipsWithinPlacementArea())
            {
                SelectionBoxMaterial.color = SelectionBoxCol;
            }
            else
            {
                SelectionBoxMaterial.color = InvalidPlacementCol;
            }

            foreach (var item in s_SelectedChips)
            {
                var pos = item.transform.position + Vector3.forward * -0.5f;
                float sizeX =
                    item.BoundsSize.x +
                    (Pin.Radius + ScalingManager.ChipInteractionBoundsBorder * 0.75f);
                float sizeY =
                    item.BoundsSize.y + ScalingManager.ChipInteractionBoundsBorder;
                Matrix4x4 matrix =
                    Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(sizeX, sizeY, 1));
                UnityEngine.Graphics.DrawMesh(_selectionMesh, matrix, SelectionBoxMaterial, 0);
            }
        }

        private bool s_SelectedChipsWithinPlacementArea()
        {
            float bufferX =
                Pin.Radius + ScalingManager.ChipInteractionBoundsBorder * 0.75f;
            float bufferY = ScalingManager.ChipInteractionBoundsBorder;
            Bounds area = ChipArea.bounds;

            for (int i = 0; i < s_SelectedChips.Count; i++)
            {
                Chip chip = s_SelectedChips[i];
                float left =
                    chip.transform.position.x - (chip.BoundsSize.x + bufferX) / 2;
                float right =
                    chip.transform.position.x + (chip.BoundsSize.x + bufferX) / 2;
                float top = chip.transform.position.y + (chip.BoundsSize.y + bufferY) / 2;
                float bottom =
                    chip.transform.position.y - (chip.BoundsSize.y + bufferY) / 2;

                if (left < area.min.x || right > area.max.x || top > area.max.y ||
                    bottom < area.min.y)
                {
                    return false;
                }
            }
            return true;
        }

        private void SetDepth(Chip chip, float depth)
        {
            chip.transform.position = new Vector3(chip.transform.position.x,
                                                chip.transform.position.y, depth);
        }

        public override bool CanReleaseFocus() =>
            _currentState != State.PlacingNewChips && _currentState != State.MovingOldChips;

        public override void FocusLostHandler()
        {
            _currentState = State.None;
            s_SelectedChips.Clear();
        }

        public override void DeleteCommand()
        {
            if (!UI.UIManager.Instance.IsAnyMenuOpen)
                HandleDeletion();
        }

        private void HandleDeletion()
        {
            // Delete any selected chips
            foreach (var SelectedChip in s_SelectedChips)
                DeleteChip(SelectedChip);

            s_SelectedChips.Clear();
            _newChipsToPlace.Clear();
            _newChipsToPaste.Clear();
            _chipsToPaste.Clear();
        }
    }
}