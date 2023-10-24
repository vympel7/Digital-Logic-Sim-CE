using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI
{
    using Scripts.Chip;
    using Scripts.Graphics;

    public class PinNameDisplayManager : MonoBehaviour
    {
        public PinNameDisplay pinNamePrefab;
        ChipEditor chipEditor;
        ChipEditorOptions editorDisplayOptions;
        Pin highlightedPin;

        [HideInInspector]
        public List<PinNameDisplay> pinNameDisplays;
        List<Pin> pinsToDisplay;

        void Awake()
        {
            chipEditor = FindObjectOfType<ChipEditor>();
            editorDisplayOptions = FindObjectOfType<ChipEditorOptions>();
            chipEditor.PinAndWireInteraction.onMouseOverPin += OnMouseOverPin;
            chipEditor.PinAndWireInteraction.onMouseExitPin += OnMouseExitPin;

            pinNameDisplays = new List<PinNameDisplay>();
            pinsToDisplay = new List<Pin>();
        }

        public void UpdateTextSize(float fontSize)
        {
            foreach (PinNameDisplay display in pinNameDisplays)
                display.NameUI.fontSize = fontSize;
        }

        void LateUpdate()
        {
            var mode = editorDisplayOptions.ActivePinNameDisplayMode;
            pinsToDisplay.Clear();

            if (mode == ChipEditorOptions.PinNameDisplayMode.AlwaysMain ||
                mode == ChipEditorOptions.PinNameDisplayMode.AlwaysAll)
            {
                if (mode == ChipEditorOptions.PinNameDisplayMode.AlwaysAll)
                {
                    foreach (var chip in chipEditor.ChipInteraction.AllChips)
                    {
                        pinsToDisplay.AddRange(chip.InputPins);
                        pinsToDisplay.AddRange(chip.OutputPins);
                    }
                }
                foreach (var chip in chipEditor.InputsEditor.Signals)
                {
                    if (!chipEditor.InputsEditor.SelectedSignals.Contains(chip))
                    {
                        pinsToDisplay.AddRange(chip.OutputPins);
                    }
                }
                foreach (var chip in chipEditor.OutputsEditor.Signals)
                {
                    if (!chipEditor.OutputsEditor.SelectedSignals.Contains(chip))
                    {
                        pinsToDisplay.AddRange(chip.InputPins);
                    }
                }
            }

            if (highlightedPin)
            {
                bool nameDisplayKey =
                    Interaction.InputHelper.AnyOfTheseKeysHeld(KeyCode.LeftAlt, KeyCode.RightAlt);
                if (nameDisplayKey ||
                    mode == ChipEditorOptions.PinNameDisplayMode.Hover)
                {
                    pinsToDisplay.Add(highlightedPin);
                }
            }

            DisplayPinName(pinsToDisplay);
        }

        public void DisplayPinName(List<Pin> pins)
        {
            if (pinNameDisplays.Count < pins.Count)
            {
                int numToAdd = pins.Count - pinNameDisplays.Count;
                for (int i = 0; i < numToAdd; i++)
                {
                    pinNameDisplays.Add(Instantiate(pinNamePrefab, parent: transform));
                }
            }
            else if (pinNameDisplays.Count > pins.Count)
            {
                for (int i = pins.Count; i < pinNameDisplays.Count; i++)
                {
                    pinNameDisplays[i].gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < pins.Count; i++)
            {
                pinNameDisplays[i].gameObject.SetActive(true);
                pinNameDisplays[i].Set(pins[i]);
            }
        }

        void OnMouseOverPin(Pin pin) { highlightedPin = pin; }

        void OnMouseExitPin(Pin pin)
        {
            if (highlightedPin == pin)
            {
                highlightedPin = null;
            }
        }
    }
}