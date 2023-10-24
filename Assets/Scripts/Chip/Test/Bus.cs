using UnityEngine;

namespace Assets.Scripts.Chip.Test
{
	public class Bus : Chip
	{
		public MeshRenderer meshRenderer;
		public Graphics.Palette palette;
		const int HighZ = -1;

		protected override void ProcessOutput()
		{
			int outputSignal = -1;
			for (int i = 0; i < InputPins.Length; i++)
			{
				if (InputPins[i].HasParent)
				{
					if (InputPins[i].State != HighZ)
					{
						if (InputPins[i].State == 1)
						{
							outputSignal = 1;
						}
						else
						{
							outputSignal = 0;
						}
					}
				}
			}

			for (int i = 0; i < OutputPins.Length; i++)
			{
				OutputPins[i].ReceiveSignal(outputSignal);
			}

			SetCol(outputSignal);
		}

		private void SetCol(int signal)
		{
			meshRenderer.material.color = (signal == 1) ? palette.OnCol : palette.OffCol;
			if (signal == -1)
			{
				meshRenderer.material.color = palette.HighZCol;
			}
		}

		public Pin GetBusConnectionPin (Pin wireStartPin, Vector2 connectionPos)
		{
			Pin connectionPin = null;
			// Wire wants to put data onto bus
			if (wireStartPin != null && wireStartPin.PType == Pin.PinType.ChipOutput)
			{
				connectionPin = FindUnusedInputPin ();
			}
			else
			{
				// Wire wants to get data from bus
				connectionPin = FindUnusedOutputPin ();
			}
			var lineCentre = (Vector2) transform.position;
			var pos = Utility.MathUtility.ClosestPointOnLineSegment (lineCentre + Vector2.left * 100, lineCentre + Vector2.right * 100, connectionPos);
			connectionPin.transform.position = pos;
			return connectionPin;
		}

		Pin FindUnusedOutputPin()
		{
			for (int i = 0; i < OutputPins.Length; i++)
			{
				if (OutputPins[i].ChildPins.Count == 0)
				{
					return OutputPins[i];
				}
			}
			Debug.Log ("Ran out of pins");
			return null;
		}

		Pin FindUnusedInputPin()
		{
			for (int i = 0; i < InputPins.Length; i++)
			{
				if (InputPins[i].ParentPin == null)
				{
					return InputPins[i];
				}
			}
			Debug.Log ("Ran out of pins");
			return null;
		}
	}
}