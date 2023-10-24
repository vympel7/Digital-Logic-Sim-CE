namespace Assets.Scripts.Chip
{
	public class OrGate : BuiltinChip
	{
		protected override void Awake()
		{
			base.Awake();
		}

		protected override void ProcessOutput()
		{
			OutputPins[0].ReceiveSignal(InputPins[0].State | InputPins[1].State);
		}

	}
}