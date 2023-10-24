namespace Assets.Scripts.SaveSystem.Serializable
{
	[System.Serializable]
	public class SavedComponentChip
	{
		public string ChipName;
		public float PosX;
		public float PosY;

		public SavedInputPin[] InputPins;
		public SavedOutputPin[] OutputPins;

		public SavedComponentChip(ChipSaveData chipSaveData, Chip.Chip chip)
		{
			ChipName = chip.ChipName;

			PosX = chip.transform.position.x;
			PosY = chip.transform.position.y;

			// Input pins
			InputPins = new SavedInputPin[chip.InputPins.Length];
			for (int i = 0; i < InputPins.Length; i++)
				InputPins[i] = new SavedInputPin(chipSaveData, chip.InputPins[i]);

			// Output pins
			OutputPins = new SavedOutputPin[chip.OutputPins.Length];
			for (int i = 0; i < chip.OutputPins.Length; i++)
				OutputPins[i] = new SavedOutputPin(chipSaveData, chip.OutputPins[i]);
		}

	}
}