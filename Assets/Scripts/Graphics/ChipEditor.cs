using UnityEngine;

namespace Assets.Scripts.Graphics
{
    using Scripts.Chip;
    using Scripts.Core;
    using Scripts.Interaction;
    using Scripts.SaveSystem;
    using Scripts.SaveSystem.Serializable;

    public class ChipEditor : MonoBehaviour
    {
        public Transform ChipImplementationHolder;
        public Transform WireHolder;

        public ChipInterfaceEditor InputsEditor;
        public ChipInterfaceEditor OutputsEditor;
        public ChipInteraction ChipInteraction;
        public PinAndWireInteraction PinAndWireInteraction;

        public UI.PinNameDisplayManager PinNameDisplayManager;

        public ChipData Data;

        private void Awake()
        {
            Data = new ChipData()
            {
                FolderIndex = 0,
                Scale = 1
            };

            PinAndWireInteraction.Init(ChipInteraction, InputsEditor, OutputsEditor);
            PinAndWireInteraction.onConnectionChanged += OnChipNetworkModified;
            GetComponentInChildren<Canvas>().worldCamera = Camera.main;
        }

        private void LateUpdate()
        {
            InputsEditor.OrderedUpdate();
            OutputsEditor.OrderedUpdate();
            PinAndWireInteraction.OrderedUpdate();
            ChipInteraction.OrderedUpdate();
        }

        private void OnChipNetworkModified() { CycleDetector.MarkAllCycles(this); }

        public void LoadFromSaveData(ChipSaveData saveData)
        {
            Data = saveData.Data;
            ScalingManager.Scale = Data.Scale;

            // Load component chips
            foreach (Chip componentChip in saveData.ComponentChips)
            {
                if (componentChip is InputSignal inp)
                {
                    inp.wireType = inp.OutputPins[0].WType;
                    InputsEditor.LoadSignal(inp);
                }
                else if (componentChip is OutputSignal outp)
                {
                    outp.wireType = outp.InputPins[0].WType;
                    OutputsEditor.LoadSignal(outp);
                }
                else
                {
                    ChipInteraction.LoadChip(componentChip);
                }
            }

            // Load wires
            if (saveData.Wires != null)
            {
                foreach (Wire wire in saveData.Wires)
                {
                    PinAndWireInteraction.LoadWire(wire);
                }
            }

            UI.ChipEditorOptions.Instance.SetUIValues(this);
        }

        public void UpdateChipSizes()
        {
            foreach (Chip chip in ChipInteraction.AllChips)
            {
                ChipPackage package = chip.GetComponent<ChipPackage>();
                if (package)
                {
                    package.SetSizeAndSpacing(chip);
                }
            }
        }

        private void OnDestroy()
        {
            ChipInteraction.VisiblePins.Clear();
            InputsEditor.VisiblePins.Clear();
            OutputsEditor.VisiblePins.Clear();
        }
    }
}