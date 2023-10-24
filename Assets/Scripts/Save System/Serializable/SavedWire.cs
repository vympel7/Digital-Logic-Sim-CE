using UnityEngine;

namespace Assets.Scripts.SaveSystem.Serializable
{
    [System.Serializable]
    public class SavedWire
    {
        public int ParentChipIndex;
        public int ParentChipOutputIndex;
        public int ChildChipIndex;
        public int ChildChipInputIndex;
        public Vector2[] AnchorPoints;

        public SavedWire(ChipSaveData chipSaveData, Graphics.Wire wire)
        {
            Chip.Pin parentPin = wire.StartPin;
            Chip.Pin childPin = wire.EndPin;

            ParentChipIndex = chipSaveData.ComponentChipIndex(parentPin.Chip);
            ParentChipOutputIndex = parentPin.Index;

            ChildChipIndex = chipSaveData.ComponentChipIndex(childPin.Chip);
            ChildChipInputIndex = childPin.Index;

            AnchorPoints = wire.AnchorPoints.ToArray();
        }
    }
}