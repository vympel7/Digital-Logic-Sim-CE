using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Interaction
{
    using Scripts.Chip;
    using Scripts.Core;
    using Scripts.Graphics;

    public class WireInformation
    {
        public Wire wire;

        public int startChipIndex;
        public int startChipPinIndex;

        public int endChipIndex;
        public int endChipPinIndex;

        public Vector2[] anchorPoints;

        public WireInformation() { }
    }

    public class CopyPaste : MonoBehaviour
    {
        List<KeyValuePair<Chip, Vector3>> clipboard =
            new List<KeyValuePair<Chip, Vector3>>();
        List<WireInformation> wires = new List<WireInformation>();

        void Update()
        {
            if (Input.GetKey(KeyCode.LeftCommand) && Input.GetKeyDown(KeyCode.C))
                Copy();

            if (Input.GetKey(KeyCode.LeftCommand) && Input.GetKeyDown(KeyCode.V))
                Paste();
        }

        public void Copy()
        {
            if (Manager.ActiveChipEditor.PinAndWireInteraction.CurrentState !=
                PinAndWireInteraction.State.PasteWires)
            {
                clipboard.Clear();
                List<Vector3> positions = new List<Vector3>();
                List<Chip> selected = Manager.ActiveChipEditor.ChipInteraction.SelectedChips;

                wires.Clear();

                foreach (Wire wire in Manager.ActiveChipEditor.PinAndWireInteraction
                            .allWires)
                {
                    WireInformation info = RequiredWire(wire, selected);
                    if (info != null)
                    {
                        wires.Add(info);
                    }
                }

                foreach (Chip Chip in selected)
                {
                    positions.Add(Chip.transform.position);
                }
                Vector3 center = Utility.MathUtility.Center(positions);
                foreach (Chip Chip in selected)
                {
                    clipboard.Add(new KeyValuePair<Chip, Vector3>(
                        Manager.Instance.GetChipPrefab(Chip),
                        Chip.transform.position - center));
                }
            }
        }

        public void Paste()
        {
            if (Manager.ActiveChipEditor.PinAndWireInteraction.CurrentState !=
                PinAndWireInteraction.State.PasteWires)
            {
                foreach (KeyValuePair<Chip, Vector3> clipboardItem in clipboard)
                {
                    if (clipboardItem.Key is CustomChip custom)
                        custom.ApplyWireModes();
                }
                List<Chip> newChips =
                    Manager.ActiveChipEditor.ChipInteraction.PasteChips(clipboard);
                Manager.ActiveChipEditor.PinAndWireInteraction.PasteWires(wires,
                                                                        newChips);
            }
        }

        public WireInformation RequiredWire(Wire wire, List<Chip> chips)
        {
            List<Pin> inputs = new List<Pin>();
            List<Pin> outputs = new List<Pin>();

            foreach (Chip chip in chips)
            {
                inputs.AddRange(chip.InputPins);
                outputs.AddRange(chip.OutputPins);
            }

            if (inputs.Contains(wire.EndPin) && outputs.Contains(wire.StartPin))
            {
                WireInformation info = new WireInformation
                {
                    wire = wire
                };

                List<Vector2> anchorPoints = new List<Vector2>();
                for (int i = 0; i < wire.LineRenderer.positionCount; i++)
                {
                    anchorPoints.Add(wire.LineRenderer.GetPosition(i));
                }
                info.anchorPoints = anchorPoints.ToArray();

                info.endChipIndex = chips.IndexOf(wire.EndPin.Chip);
                info.startChipIndex = chips.IndexOf(wire.StartPin.Chip);

                info.endChipPinIndex =
                    Array.IndexOf(chips[info.endChipIndex].InputPins, wire.EndPin);
                info.startChipPinIndex =
                    Array.IndexOf(chips[info.startChipIndex].OutputPins, wire.StartPin);

                return info;
            }
            return null;
        }
    }
}