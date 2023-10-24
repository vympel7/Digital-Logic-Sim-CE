using UnityEngine;

namespace Assets.Scripts.Graphics
{
    using Scripts.Chip;
    using Scripts.Core;

    public class ChipPackage : MonoBehaviour
    {
        public enum ChipType { Compatibility, Gate, Miscellaneous, Custom };

        public ChipType Type;
        public TMPro.TextMeshPro NameText;
        public Transform Container;
        public Pin ChipPinPrefab;
        public bool OverrideWidthAndHeight = false;
        public float OverrideWidth = 1f;
        public float OverrideHeight = 1f;

        private const string _pinHolderName = "Pin Holder";

        private void Awake()
        {
            BuiltinChip builtinChip = GetComponent<BuiltinChip>();
            if (builtinChip != null)
            {
                SetSizeAndSpacing(GetComponent<Chip>());
                SetColour(builtinChip.PackageColour);
            }
            NameText.fontSize = ScalingManager.PackageFontSize;
        }

        public void PackageCustomChip(ChipEditor chipEditor)
        {
            var chipName = chipEditor.Data.Name;
            gameObject.name = chipName;
            NameText.text = chipName;
            NameText.color = chipEditor.Data.NameColour;
            SetColour(chipEditor.Data.Colour);

            // Add and set up the custom chip component
            CustomChip chip = gameObject.AddComponent<CustomChip>();
            chip.ChipName = chipName;
            chip.FolderIndex = chipEditor.Data.FolderIndex;
            Type = ChipType.Custom;
            // Set input Signals
            chip.inputSignals = new InputSignal[chipEditor.InputsEditor.Signals.Count];
            for (int i = 0; i < chip.inputSignals.Length; i++)
                chip.inputSignals[i] = (InputSignal)chipEditor.InputsEditor.Signals[i];

            // Set output Signals
            chip.outputSignals = new OutputSignal[chipEditor.OutputsEditor.Signals.Count];
            for (int i = 0; i < chip.outputSignals.Length; i++)
                chip.outputSignals[i] = (OutputSignal)chipEditor.OutputsEditor.Signals[i];

            // Create pins and set set package size
            SpawnPins(chip);
            SetSizeAndSpacing(chip);

            // Parent chip holder to the template, and hide
            Transform implementationHolder = chipEditor.ChipImplementationHolder;

            implementationHolder.parent = transform;
            //implementationHolder.localPosition = Vector3.zero;
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
                inputPin.SetScale();
            }

            for (int i = 0; i < chip.OutputPins.Length; i++)
            {
                Pin outputPin = Instantiate(ChipPinPrefab, pinHolder.position,
                                            Quaternion.identity, pinHolder);
                outputPin.PType = Pin.PinType.ChipOutput;
                outputPin.Chip = chip;
                outputPin.PinName = chip.outputSignals[i].InputPins[0].PinName;
                chip.OutputPins[i] = outputPin;
                outputPin.SetScale();
            }
        }

        public void SetSizeAndSpacing(Chip chip)
        {
            NameText.fontSize = ScalingManager.PackageFontSize;

            float containerHeightPadding = 0;
            float containerWidthPadding = 0.1f;
            float pinSpacePadding = Pin.Radius * 0.2f;
            float containerWidth = NameText.preferredWidth +
                                Pin.InteractionRadius * 2f + containerWidthPadding;

            int numPins = Mathf.Max(chip.InputPins.Length, chip.OutputPins.Length);
            float unpaddedContainerHeight =
                numPins * (Pin.Radius * 2 + pinSpacePadding);
            float containerHeight =
                Mathf.Max(unpaddedContainerHeight, NameText.preferredHeight + 0.05f) +
                containerHeightPadding;
            float topPinY = unpaddedContainerHeight / 2 - Pin.Radius;
            float bottomPinY = -unpaddedContainerHeight / 2 + Pin.Radius;
            const float z = -0.05f;

            // Input pins
            int numInputPinsToAutoPlace = chip.InputPins.Length;
            for (int i = 0; i < numInputPinsToAutoPlace; i++)
            {
                float percent = 0.5f;
                if (chip.InputPins.Length > 1)
                {
                    percent = i / (numInputPinsToAutoPlace - 1f);
                }
                if (OverrideWidthAndHeight)
                {
                    float posX = -OverrideWidth / 2f;
                    float posY = Mathf.Lerp(topPinY, bottomPinY, percent);
                    chip.InputPins[i].transform.localPosition = new Vector3(posX, posY, z);
                }
                else
                {
                    float posX = -containerWidth / 2f;
                    float posY = Mathf.Lerp(topPinY, bottomPinY, percent);
                    chip.InputPins[i].transform.localPosition = new Vector3(posX, posY, z);
                }
            }

            // Output pins
            for (int i = 0; i < chip.OutputPins.Length; i++)
            {
                float percent = 0.5f;
                if (chip.OutputPins.Length > 1)
                {
                    percent = i / (chip.OutputPins.Length - 1f);
                }

                float posX = containerWidth / 2f;
                float posY = Mathf.Lerp(topPinY, bottomPinY, percent);
                chip.OutputPins[i].transform.localPosition = new Vector3(posX, posY, z);
            }

            // Set container size
            if (OverrideWidthAndHeight)
            {
                Container.transform.localScale =
                    new Vector3(OverrideWidth, OverrideHeight, 1);
                GetComponent<BoxCollider2D>().size =
                    new Vector2(OverrideWidth, OverrideHeight);
            }
            else
            {
                Container.transform.localScale =
                    new Vector3(containerWidth, containerHeight, 1);
                GetComponent<BoxCollider2D>().size =
                    new Vector2(containerWidth, containerHeight);
            }
        }

        private void SetColour(Color colour)
        {
            Container.GetComponent<MeshRenderer>().material.color = colour;
        }
    }
}