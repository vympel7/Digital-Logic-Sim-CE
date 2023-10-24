using System;
using System.Linq;

namespace Assets.Scripts.Chip
{
	public class BusDecoder : BuiltinChip
	{
		protected override void Awake()
		{
			base.Awake();
		}

		protected override void ProcessOutput()
		{
			var inputSignal = InputPins[0].State;
			foreach (var outputPin in OutputPins.Reverse())
			{
				outputPin.ReceiveSignal(inputSignal & 1);
				inputSignal >>= 1;
			}
		}
	}
}
