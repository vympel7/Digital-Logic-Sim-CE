namespace Assets.Scripts.SaveSystem.Serializable
{
	[System.Serializable]
	public class SavedInputPin
	{
		public string Name;
		// An input pin receives its input from one of the output pins of some chip (called the parent chip)
		// The chipIndex is the chip's index in the array of chips being written to file
		public int ParentChipIndex;
		public int ParentChipOutputIndex;
		public bool IsCylic;
		public Chip.Pin.WireType WireType;

		public SavedInputPin(ChipSaveData chipSaveData, Chip.Pin pin)
		{
			Name = pin.PinName;
			IsCylic = pin.Cyclic;
			WireType = pin.WType;
			if (pin.ParentPin)
			{
				ParentChipIndex = chipSaveData.ComponentChipIndex(pin.ParentPin.Chip);
				ParentChipOutputIndex = pin.ParentPin.Index;
			}
			else
			{
				ParentChipIndex = -1;
				ParentChipOutputIndex = -1;
			}
		}
	}
}