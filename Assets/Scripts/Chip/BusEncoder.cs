using System.Linq;

namespace Assets.Scripts.Chip
{
	public class BusEncoder : BuiltinChip
	{
		protected override void Awake()
		{
			base.Awake();
		}

		protected override void ProcessOutput()
		{
			int outputSignal = 0;
			foreach (var inputState in InputPins.Select(x => x.State))
			{
				outputSignal <<= 1;
				outputSignal |= inputState;
			}
			OutputPins[0].ReceiveSignal(outputSignal);
		}
	}
}
