using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Interaction
{
    using Scripts.Chip;
    using Scripts.Core;
    using Scripts.Graphics;
    using Scripts.Utility;
    using Scripts.UI;

    // Allows player to add/remove/move/rename inputs or outputs of a chip.
    public class ChipInterfaceEditor : Interactable
    {
        private const int _maxGroupSize = 16;

        public event System.Action<Chip> OnDeleteChip;
        public event System.Action OnChipsAddedOrDeleted;

        public enum EditorType { Input, Output }
        public enum HandleState { Default, Highlighted, Selected, SelectedAndFocused }
        private const float _forwardDepth = -0.1f;

        public List<ChipSignal> Signals { get; private set; }

        public EditorType EditType;

        [Header("References")]
        public Transform ChipContainer;
        public ChipSignal SignalPrefab;

        public UI.Menu.ChipPropertiesMenu PropertiesMenu;

        public Transform SignalHolder;
        public Transform BarGraphic;
        public ChipInterfaceEditor OtherEditor;

        [Header("Appearance")]
        public Color HandleCol;
        public Color HighlightedHandleCol;
        public Color SelectedHandleCol;
        public Color SelectedAndFocusedHandleCol;

        public bool ShowPreviewSignal;

        [HideInInspector]
        public List<Pin> VisiblePins;

        private const float _handleSizeX = 0.15f;

        private string _currentEditorName;
        public ChipEditor CurrentEditor
        {
            set => _currentEditorName = value.Data.Name;
        }

        private ChipSignal _highlightedSignal;
        public List<ChipSignal> SelectedSignals { get; private set; }
        private ChipSignal[] _previewSignals;

        private BoxCollider2D _inputBounds;

        private Mesh _quadMesh;
        private Material _handleMat;
        private Material _highlightedHandleMat;
        private Material _selectedHandleMat;
        private Material _selectedAndhighlightedHandle;
        private bool _mouseInInputBounds;

        // Dragging
        private bool _isDragging;
        private float _dragHandleStartY;
        private float _dragMouseStartY;

        // Grouping
        private int _currentGroupSize = 1;
        private int _currentGroupID;
        private Dictionary<int, ChipSignal[]> _groupsByID;


        private void Awake()
        {
            Signals = new List<ChipSignal>();
            SelectedSignals = new List<ChipSignal>();
            _groupsByID = new Dictionary<int, ChipSignal[]>();
            VisiblePins = new List<Pin>();

            _inputBounds = GetComponent<BoxCollider2D>();
            MeshShapeCreator.CreateQuadMesh(ref _quadMesh);
            _handleMat = MaterialUtility.CreateUnlitMaterial(HandleCol);
            _highlightedHandleMat = MaterialUtility.CreateUnlitMaterial(HighlightedHandleCol);
            _selectedHandleMat = MaterialUtility.CreateUnlitMaterial(SelectedHandleCol);
            _selectedAndhighlightedHandle = MaterialUtility.CreateUnlitMaterial(SelectedAndFocusedHandleCol);

            _previewSignals = new ChipSignal[_maxGroupSize];
            for (int i = 0; i < _maxGroupSize; i++)
            {
                var previewSignal = Instantiate(SignalPrefab);
                previewSignal.SetInteractable(false);
                previewSignal.gameObject.SetActive(false);
                previewSignal.signalName = "Preview";
                previewSignal.transform.SetParent(transform, true);
                _previewSignals[i] = previewSignal;
            }

            FindObjectOfType<CreateGroup>().OnGroupSizeSettingPressed += SetGroupSize;

        }

        public void Start()
        {
            PropertiesMenu.DisableUI();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && !InputHelper.MouseOverUIObject())
                ReleaseFocus();
        }

        public override void FocusLostHandler()
        {

            _highlightedSignal = null;
            PropertiesMenu.DisableUI();
            ClearSelectedSignals();

            HidePreviews();
            //currentGroupSize = 1;
        }

        // Event handler when changed input or output pin wire type
        public void ChangeWireType(int mode)
        {
            if (!IsSomethingSelected)
                return;

            // Change output pin wire mode
            foreach (var sig in SelectedSignals)
                sig.wireType = (Pin.WireType)mode;

            foreach (var pin in SelectedSignals.SelectMany(x => x.InputPins))
                pin.WType = (Pin.WireType)mode;

            // Change input pin wire mode
            if (SelectedSignals[0] is InputSignal)
            {
                foreach (InputSignal signal in SelectedSignals)
                {
                    var pin = signal.OutputPins[0];
                    if (pin == null) return;
                    pin.WType = (Pin.WireType)mode;
                    signal.SetState(0);
                }
            }
        }

        public override void OrderedUpdate()
        {
            if (!InputHelper.MouseOverUIObject())
            {
                HandleInput();
            }
            else if (HasFocus)
            {
                ReleaseFocusNotHandled();
                HidePreviews();
            }
            DrawSignalHandles();

        }

        private void SetGroupSize(int groupSize) => _currentGroupSize = groupSize;

        public void LoadSignal(InputSignal signal)
        {
            signal.transform.parent = SignalHolder;
            Signals.Add(signal);

            signal.signalName = signal.OutputPins[0].PinName;
            VisiblePins.Add(signal.OutputPins[0]);
        }

        public void LoadSignal(OutputSignal signal)
        {
            signal.transform.parent = SignalHolder;
            Signals.Add(signal);

            signal.signalName = signal.InputPins[0].PinName;
            VisiblePins.Add(signal.InputPins[0]);
        }

        private void HandleInput()
        {
            Vector2 mousePos = InputHelper.MouseWorldPos;

            _mouseInInputBounds = _inputBounds.OverlapPoint(mousePos);



            _highlightedSignal = GetSignalUnderMouse();

            if (_mouseInInputBounds && _highlightedSignal != null && Input.GetMouseButtonDown(0))
                RequestFocus();
            else if (!IsSomethingSelected)
            {
                ReleaseFocusNotHandled();
                _isDragging = false;
            }

            if (HasFocus)
            {
                OtherEditor.ClearSelectedSignals();

                if (Input.GetMouseButtonDown(0))
                    SelectSignal(_highlightedSignal);

                // If a signal is selected, handle movement/renaming/deletion
                if (IsSomethingSelected)
                {
                    if (_isDragging)
                    {
                        float handleNewY = mousePos.y + (_dragHandleStartY - _dragMouseStartY);
                        bool cancel = Input.GetKeyDown(KeyCode.Escape);

                        if (cancel) handleNewY = _dragHandleStartY;

                        for (int i = 0; i < SelectedSignals.Count; i++)
                        {
                            float y = CalcY(handleNewY, SelectedSignals.Count, i);
                            SelectedSignals[i].transform.SetYPos(y);
                        }

                        if (Input.GetMouseButtonUp(0)) _isDragging = false;


                        // Cancel drag and deselect
                        if (cancel) FocusLostHandler();
                    }

                    UpdatePropertyUIPosition();

                    // Finished with selected signal, so deselect it
                    if (Input.GetKeyDown(KeyCode.Return)) FocusLostHandler();

                }

            }
            HidePreviews();
            if (_highlightedSignal == null && !_isDragging)
            {
                if (_mouseInInputBounds && !InputHelper.MouseOverUIObject())
                {

                    if (InputHelper.AnyOfTheseKeysDown(KeyCode.Plus, KeyCode.KeypadPlus,
                                                    KeyCode.Equals))
                    {
                        _currentGroupSize =
                            Mathf.Clamp(_currentGroupSize + 1, 1, _maxGroupSize);
                    }
                    else if (InputHelper.AnyOfTheseKeysDown(KeyCode.Minus,
                                                            KeyCode.KeypadMinus,
                                                            KeyCode.Underscore))
                    {
                        _currentGroupSize =
                            Mathf.Clamp(_currentGroupSize - 1, 1, _maxGroupSize);
                    }

                    HandleSpawning();
                }
            }
        }

        public void ClearSelectedSignals() { SelectedSignals.Clear(); }

        private float CalcY(float mouseY, int groupSize, int index)
        {
            float centreY = mouseY;
            float halfExtent = ScalingManager.GroupSpacing * (groupSize - 1f);
            float maxY = centreY + halfExtent + ScalingManager.HandleSizeY / 2f;
            float minY = centreY - halfExtent - ScalingManager.HandleSizeY / 2f;

            if (maxY > BoundsTop)
            {
                centreY -= maxY - BoundsTop;
            }
            else if (minY < BoundsBottom)
            {
                centreY += BoundsBottom - minY;
            }

            float t = (groupSize > 1) ? index / (groupSize - 1f) : 0.5f;
            t = t * 2 - 1;
            float posY = centreY - t * halfExtent;
            return posY;
        }

        private float ClampY(float y)
        {
            return Mathf.Clamp(y, BoundsBottom + ScalingManager.HandleSizeY / 2f,
                            BoundsTop - ScalingManager.HandleSizeY / 2f);
        }

        public ChipSignal[][] GetGroups()
        {
            var keys = _groupsByID.Keys;
            ChipSignal[][] groups = new ChipSignal[keys.Count][];
            int i = 0;
            foreach (var key in keys)
            {
                groups[i] = _groupsByID[key];
                i++;
            }
            return groups;
        }

        // Handles spawning if user clicks, otherwise displays preview
        private void HandleSpawning()
        {

            if (InputHelper.MouseOverUIObject())
                return;

            float containerX = ChipContainer.position.x +
                            ChipContainer.localScale.x / 2 *
                                ((EditType == EditorType.Input) ? -1 : 1);
            float centreY = ClampY(InputHelper.MouseWorldPos.y);

            // Spawn on mouse down
            if (Input.GetMouseButtonDown(0))
            {
                ChipSignal[] spawnedSignals = new ChipSignal[_currentGroupSize];

                var isGroup = _currentGroupSize > 1;

                for (int i = 0; i < _currentGroupSize; i++)
                {
                    float posY = CalcY(InputHelper.MouseWorldPos.y, _currentGroupSize, i);
                    Vector3 spawnPos = new Vector3(containerX, posY, ChipContainer.position.z + _forwardDepth);

                    ChipSignal spawnedSignal = Instantiate(SignalPrefab, spawnPos, Quaternion.identity, SignalHolder);
                    spawnedSignal.GetComponent<IOScaler>().UpdateScale();
                    if (isGroup)
                    {
                        spawnedSignal.GroupID = _currentGroupID;
                        spawnedSignal.displayGroupDecimalValue = true;
                    }
                    Signals.Add(spawnedSignal);
                    VisiblePins.AddRange(spawnedSignal.InputPins);
                    VisiblePins.AddRange(spawnedSignal.OutputPins);
                    spawnedSignals[i] = spawnedSignal;

                }

                if (isGroup)
                {
                    _groupsByID.Add(_currentGroupID, spawnedSignals);
                    // Reset group size after spawning
                    _currentGroupSize = 1;
                    // Generate new ID for next group
                    // This will be used to identify which signals were created together as
                    // a group
                    _currentGroupID++;
                }
                SelectSignal(Signals[Signals.Count - 1]);
                OnChipsAddedOrDeleted?.Invoke();
            }
            // Draw handle and signal previews
            else
            {
                for (int i = 0; i < _currentGroupSize; i++)
                {
                    float posY = CalcY(InputHelper.MouseWorldPos.y, _currentGroupSize, i);
                    Vector3 spawnPos = new Vector3(containerX, posY, ChipContainer.position.z + _forwardDepth);
                    DrawHandle(posY, HandleState.Highlighted);
                    if (ShowPreviewSignal)
                    {
                        _previewSignals[i].gameObject.SetActive(true);
                        _previewSignals[i].transform.position =
                            spawnPos - Vector3.forward * _forwardDepth;
                    }
                }
            }
        }

        private void HidePreviews()
        {
            foreach (ChipSignal PrevSig in _previewSignals)
                PrevSig.gameObject.SetActive(false);
        }

        private float BoundsTop => transform.position.y + (transform.localScale.y / 2);

        private float BoundsBottom => transform.position.y - transform.localScale.y / 2f;

        public bool IsSomethingSelected => SelectedSignals.Count > 0;

        public override bool CanReleaseFocus() => !_isDragging && !_mouseInInputBounds;

        private void UpdatePropertyUIPosition()
        {
            if (IsSomethingSelected)
            {
                Vector3 centre =
                    (SelectedSignals[0].transform.position + SelectedSignals[SelectedSignals.Count - 1].transform.position) / 2;

                PropertiesMenu.SetPosition(centre, EditType);
            }
        }

        public void UpdateGroupProperty(string NewName, bool twosComplementToggle)
        {
            // Update signal properties
            foreach (ChipSignal Signal in SelectedSignals)
            {
                Signal.UpdateSignalName(NewName);
                Signal.useTwosComplement = twosComplementToggle;
            }
        }

        private void DrawSignalHandles()
        {
            foreach (ChipSignal singnal in Signals)
            {
                HandleState handleState = HandleState.Default;

                if (SelectedSignals.Contains(singnal))
                    handleState = HasFocus ? HandleState.SelectedAndFocused : HandleState.Selected;
                else if (singnal == _highlightedSignal)
                    handleState = HandleState.Highlighted;

                DrawHandle(singnal.transform.position.y, handleState);
            }
        }

        private ChipSignal GetSignalUnderMouse()
        {
            ChipSignal signalUnderMouse = null;
            float nearestDst = float.MaxValue;

            for (int i = 0; i < Signals.Count; i++)
            {
                ChipSignal currentSignal = Signals[i];
                float handleY = currentSignal.transform.position.y;

                Vector2 handleCentre = new Vector2(transform.position.x, handleY);
                Vector2 mousePos = InputHelper.MouseWorldPos;

                const float selectionBufferY = 0.1f;

                float halfSizeX = _handleSizeX;
                float halfSizeY = (ScalingManager.HandleSizeY + selectionBufferY) / 2f;
                bool insideX = mousePos.x >= handleCentre.x - halfSizeX &&
                            mousePos.x <= handleCentre.x + halfSizeX;
                bool insideY = mousePos.y >= handleCentre.y - halfSizeY &&
                            mousePos.y <= handleCentre.y + halfSizeY;

                if (insideX && insideY)
                {
                    float dst = Mathf.Abs(mousePos.y - handleY);
                    if (dst < nearestDst)
                    {
                        nearestDst = dst;
                        signalUnderMouse = currentSignal;
                    }
                }
            }
            return signalUnderMouse;
        }

        // Select signal (starts dragging, shows rename field)
        private void SelectSignal(ChipSignal signalToDrag)
        {
            if (signalToDrag == null) return;
            // Dragging
            SelectAllSignalsInTheSameGroup(signalToDrag);

            _isDragging = true;


            _dragMouseStartY = InputHelper.MouseWorldPos.y;
            if (SelectedSignals.Count % 2 == 0)
            {
                int indexA = Mathf.Max(0, SelectedSignals.Count / 2 - 1);
                int indexB = SelectedSignals.Count / 2;
                _dragHandleStartY = (SelectedSignals[indexA].transform.position.y +
                                    SelectedSignals[indexB].transform.position.y) /
                                2f;
            }
            else
            {
                _dragHandleStartY = SelectedSignals[SelectedSignals.Count / 2].transform.position.y;
            }

            PropertiesMenu?.EnableUI(this,
                    SelectedSignals[0].signalName, SelectedSignals.Count > 1,
                    SelectedSignals[0].useTwosComplement, _currentEditorName,
                    signalToDrag.signalName, (int)SelectedSignals[0].wireType);
            RequestFocus();

            UpdatePropertyUIPosition();
        }

        private void SelectAllSignalsInTheSameGroup(ChipSignal signalToDrag)
        {
            ClearSelectedSignals();

            foreach (ChipSignal sig in Signals)
            {
                if (sig == signalToDrag || ChipSignal.InSameGroup(sig, signalToDrag))
                    SelectedSignals.Add(sig);
            }
        }

        private void DrawHandle(float y, HandleState handleState = HandleState.Default)
        {
            float renderZ = _forwardDepth;
            Material currentHandleMat;
            switch (handleState)
            {
                case HandleState.Highlighted:
                    currentHandleMat = _highlightedHandleMat;
                    break;
                case HandleState.Selected:
                    currentHandleMat = _selectedHandleMat;
                    renderZ = _forwardDepth * 2;
                    break;
                case HandleState.SelectedAndFocused:
                    currentHandleMat = _selectedAndhighlightedHandle;
                    renderZ = _forwardDepth * 2;
                    break;
                default:
                    currentHandleMat = _handleMat;
                    break;
            }

            Vector3 scale = new Vector3(_handleSizeX, ScalingManager.HandleSizeY, 1);
            Vector3 pos3D = new Vector3(transform.position.x, y, transform.position.z + renderZ);
            Matrix4x4 handleMatrix = Matrix4x4.TRS(pos3D, Quaternion.identity, scale);
            UnityEngine.Graphics.DrawMesh(_quadMesh, handleMatrix, currentHandleMat, 0);
        }

        public void UpdateColours()
        {
            _handleMat.color = HandleCol;
            _highlightedHandleMat.color = HighlightedHandleCol;
            _selectedHandleMat.color = SelectedHandleCol;
            _selectedAndhighlightedHandle.color = SelectedAndFocusedHandleCol;
        }

        public void UpdateScale()
        {
            transform.localPosition =
                new Vector3(ScalingManager.IoBarDistance *
                                (EditType == EditorType.Input ? -1f : 1f),
                            transform.localPosition.y, transform.localPosition.z);
            BarGraphic.localScale = new Vector3(ScalingManager.IoBarGraphicWidth, 1, 1);
            GetComponent<BoxCollider2D>().size = new Vector2(ScalingManager.IoBarGraphicWidth, 1);

            foreach (ChipSignal chipSignal in _previewSignals)
            {
                chipSignal.GetComponent<IOScaler>().UpdateScale();
            }

            foreach (ChipSignal[] group in _groupsByID.Values)
            {
                float yPos = 0;
                foreach (ChipSignal sig in group)
                {
                    yPos += sig.transform.localPosition.y;
                }
                float handleNewY = yPos /= group.Length;

                for (int i = 0; i < group.Length; i++)
                {
                    float y = CalcY(handleNewY, group.Length, i);
                    group[i].transform.SetYPos(y);
                }
            }
            UpdatePropertyUIPosition();
        }

        public override void DeleteCommand()
        {
            if (!Input.GetKeyDown(KeyCode.Backspace))
                DeleteSelected();
        }

        private void DeleteSelected()
        {
            foreach (ChipSignal selectedSignal in SelectedSignals)
            {
                if (_groupsByID.ContainsKey(selectedSignal.GroupID))
                    _groupsByID.Remove(selectedSignal.GroupID);

                OnDeleteChip?.Invoke(selectedSignal);
                Signals.Remove(selectedSignal);

                foreach (Pin pin in selectedSignal.InputPins)
                    VisiblePins.Remove(pin);
                foreach (Pin pin in selectedSignal.OutputPins)
                    VisiblePins.Remove(pin);

                Destroy(selectedSignal.gameObject);
            }
            OnChipsAddedOrDeleted?.Invoke();
            ReleaseFocus();
        }
    }
}