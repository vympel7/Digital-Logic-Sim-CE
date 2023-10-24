using System.Collections.Generic;

namespace Assets.Scripts.Chip
{
	public class HardDrive : BuiltinChip
	{
		public static Dictionary<string, List<int>> Contents;

		protected override void Awake()
		{
			base.Awake();
			Contents = SaveSystem.SaveSystem.LoadHDDContents();
		}

		protected override void ProcessOutput()
		{
			switch (InputPins[0].State)
			{
				case 0:
					string binary = "";
					for (int i = 1; i < 5; i++)
					{
						binary += InputPins[i].State.ToString();
					}
					if (Contents.ContainsKey(binary))
					{
						for (int i = 0; i < OutputPins.Length; i++)
						{
							OutputPins[i].ReceiveSignal(Contents[binary][i]);
						}
					}
					else
					{
						for (int i = 0; i < OutputPins.Length; i++)
						{
							OutputPins[0].ReceiveSignal(0);
						}
					}
					break;
				case 1:
					bool updateFile = false;
					string address = "";
					List<int> store = new List<int>();
					for (int i = 5; i < 13; i++)
					{
						store.Add(InputPins[i].State);
					}
					for (int i = 1; i < 5; i++)
					{
						address += InputPins[i].State;
					}
					if (Contents.ContainsKey(address))
					{
						if (Contents[address] != store)
						{
							updateFile = true;
						}
						Contents.Remove(address);
					}
					else
					{
						updateFile = true;
					}
					Contents.Add(address, store);
					if (updateFile)
					{
						SaveSystem.SaveSystem.SaveHDDContents(Contents);
					}
					break;
				default:
					foreach (Pin i in OutputPins)
					{
						i.ReceiveSignal(0);
					}
					break;
			}
		}
	}
}