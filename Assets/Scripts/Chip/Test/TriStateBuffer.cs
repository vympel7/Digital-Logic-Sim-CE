namespace Assets.Scripts.Chip.Test
{
	public class TriStateBuffer : Chip
	{
		protected override void Awake()
		{
			base.Awake ();
		}

		protected override void ProcessOutput()
		{
			int data = InputPins[0].State;
			int enable = InputPins[1].State;

			OutputPins[0].ReceiveSignal(enable == 1 ? data : -1);
		}
	}
}