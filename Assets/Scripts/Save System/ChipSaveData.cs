using System.Collections.Generic;

namespace Assets.Scripts.SaveSystem
{
    using Scripts.Chip;
    using Scripts.Graphics;

    public class ChipSaveData
    {
        public Serializable.ChipData Data;

        // All chips used as components in this new chip (including input and output
        // signals)
        public Chip[] ComponentChips;
        // All wires in the chip (in case saving of wire layout is desired)
        public Wire[] Wires;

        public ChipSaveData() { }

        public ChipSaveData(ChipEditor chipEditor)
        {
            List<Chip> componentChipList = new List<Chip>();

            var sortedInputs = chipEditor.InputsEditor.Signals;
            sortedInputs.Sort(
                (a, b) => b.transform.position.y.CompareTo(a.transform.position.y));
            var sortedOutputs = chipEditor.OutputsEditor.Signals; sortedOutputs.Sort(
                (a, b) => b.transform.position.y.CompareTo(a.transform.position.y));

            componentChipList.AddRange(sortedInputs);
            componentChipList.AddRange(sortedOutputs);

            componentChipList.AddRange(chipEditor.ChipInteraction.AllChips);
            ComponentChips = componentChipList.ToArray();

            Wires = chipEditor.PinAndWireInteraction.allWires.ToArray();
            Data = chipEditor.Data;
        }

        public int ComponentChipIndex(Chip componentChip)
        {
            return System.Array.IndexOf(ComponentChips, componentChip);
        }
    }
}