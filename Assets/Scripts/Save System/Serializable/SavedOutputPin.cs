namespace Assets.Scripts.SaveSystem.Serializable
{
	[System.Serializable]
	public class SavedOutputPin
	{
		public string Name;
		public Chip.Pin.WireType WireType;

		public SavedOutputPin(ChipSaveData chipSaveData, Chip.Pin pin)
		{
			Name = pin.PinName;
			WireType = pin.WType;
		}
	}
}