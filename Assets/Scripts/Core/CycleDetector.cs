using System.Collections.Generic;

namespace Assets.Scripts.Core
{
    using Scripts.Chip;

    public static class CycleDetector
    {
        private static bool _currentChipHasCycle;

        public static List<Chip> MarkAllCycles(Graphics.ChipEditor chipEditor)
        {
            var chipsWithCycles = new List<Chip>();

            HashSet<Chip> examinedChips = new HashSet<Chip>();
            Chip[] chips = chipEditor.ChipInteraction.AllChips.ToArray();

            // Clear all cycle markings
            foreach (Chip Chip in chips)
                foreach (Pin pin in Chip.InputPins)
                    pin.Cyclic = false;

            // Mark cycles
            for (int i = 0; i < chips.Length; i++)
            {
                examinedChips.Clear();
                _currentChipHasCycle = false;
                MarkCycles(chips[i], chips[i], examinedChips);

                if (_currentChipHasCycle)
                    chipsWithCycles.Add(chips[i]);
            }
            return chipsWithCycles;
        }

        private static void MarkCycles(Chip originalChip, Chip currentChip, HashSet<Chip> examinedChips)
        {
            if (examinedChips.Contains(currentChip)) return;

            examinedChips.Add(currentChip);

            foreach (var outputPin in currentChip.OutputPins)
            {
                foreach (var childPin in outputPin.ChildPins)
                {
                    var childChip = childPin.Chip;
                    if (childChip != null)
                    {
                        if (childChip == originalChip)
                        {
                            _currentChipHasCycle = true;
                            childPin.Cyclic = true;
                        }
                        // Don't continue down this path if the pin has already been marked as cyclic
                        // (doing so would lead to multiple pins along the cycle path being marked, when
                        // only the first pin responsible for the cycle should be)
                        else if (!childPin.Cyclic)
                        {
                            MarkCycles(originalChip, childChip, examinedChips);
                        }
                    }
                }
            }
        }
    }
}