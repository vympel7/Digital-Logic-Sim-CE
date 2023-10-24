namespace Assets.Scripts.Chip
{
	// Output signal of a chip.
	public class OutputSignal : ChipSignal
	{
		protected override void Start()
		{
			base.Start();
			SetDisplayState(0);
		}

		public override void ReceiveInputSignal(Pin inputPin)
		{
			CurrentState = inputPin.State;
			SetDisplayState(inputPin.State);
		}

		public override void UpdateSignalName(string newName)
		{
			base.UpdateSignalName(newName);
			InputPins[0].PinName = newName;
		}
	}
}