namespace Assets.Scripts.Chip
{
	public class NotGate : BuiltinChip
	{
		protected override void Awake()
		{
			base.Awake();
		}

		protected override void ProcessOutput()
		{
			OutputPins[0].ReceiveSignal(1 - InputPins[0].State);
		}
	}
}