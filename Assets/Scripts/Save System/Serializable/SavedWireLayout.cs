namespace Assets.Scripts.SaveSystem.Serializable
{
	[System.Serializable]
	public class SavedWireLayout
	{
		public SavedWire[] SerializableWires;

		public SavedWireLayout(ChipSaveData chipSaveData)
		{
			Graphics.Wire[] allWires = chipSaveData.Wires;
			SerializableWires = new SavedWire[allWires.Length];

			for (int i = 0; i < allWires.Length; i++)
			{
				SerializableWires[i] = new SavedWire(chipSaveData, allWires[i]);
			}
		}
	}
}