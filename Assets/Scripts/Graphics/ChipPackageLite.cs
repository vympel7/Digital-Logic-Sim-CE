using UnityEngine;

namespace Assets.Scripts.Graphics
{
    using Scripts.Chip;

    public class ChipPackageLite : MonoBehaviour
    {

        public enum ChipType { User, Basic, Advanced };

        public ChipType Type;
        public Transform Container;
        public Pin ChipPinPrefab;

        protected const string _pinHolderName = "Pin Holder";

        protected virtual void Awake()
        {
            if (Type != ChipType.User)
            {
                BuiltinChip builtinChip = GetComponent<BuiltinChip>();
            }
        }

        public virtual void PackageCustomChip(ChipEditor chipEditor)
        {
            gameObject.name = chipEditor.Data.Name;

            // Add and set up the custom chip component
            CustomChip chip = gameObject.AddComponent<CustomChip>();
            chip.ChipName = chipEditor.Data.Name;
            chip.FolderIndex = chipEditor.Data.FolderIndex;

            // Set input Signals
            chip.inputSignals = new InputSignal[chipEditor.InputsEditor.Signals.Count];
            for (int i = 0; i < chip.inputSignals.Length; i++)
            {
                chip.inputSignals[i] = (InputSignal)chipEditor.InputsEditor.Signals[i];
            }

            // Set output Signals
            chip.outputSignals =
                new OutputSignal[chipEditor.OutputsEditor.Signals.Count];
            for (int i = 0; i < chip.outputSignals.Length; i++)
            {
                chip.outputSignals[i] = (OutputSignal)chipEditor.OutputsEditor.Signals[i];
            }

            // Create pins and set set package size
            SpawnPins(chip);

            // Parent chip holder to the template, and hide
            Transform implementationHolder = chipEditor.ChipImplementationHolder;

            implementationHolder.parent = transform;
            implementationHolder.localPosition = Vector3.zero;
            implementationHolder.gameObject.SetActive(false);
        }

        public void SpawnPins(CustomChip chip)
        {
            Transform pinHolder = new GameObject(_pinHolderName).transform;
            pinHolder.parent = transform;
            pinHolder.localPosition = Vector3.zero;

            chip.InputPins = new Pin[chip.inputSignals.Length];
            chip.OutputPins = new Pin[chip.outputSignals.Length];

            for (int i = 0; i < chip.InputPins.Length; i++)
            {
                Pin inputPin = Instantiate(ChipPinPrefab, pinHolder.position,
                                        Quaternion.identity, pinHolder);
                inputPin.PType = Pin.PinType.ChipInput;
                inputPin.Chip = chip;
                inputPin.PinName = chip.inputSignals[i].OutputPins[0].PinName;
                chip.InputPins[i] = inputPin;
            }

            for (int i = 0; i < chip.OutputPins.Length; i++)
            {
                Pin outputPin = Instantiate(ChipPinPrefab, pinHolder.position,
                                            Quaternion.identity, pinHolder);
                outputPin.PType = Pin.PinType.ChipOutput;
                outputPin.Chip = chip;
                outputPin.PinName = chip.outputSignals[i].InputPins[0].PinName;
                chip.OutputPins[i] = outputPin;
            }
        }
    }
}