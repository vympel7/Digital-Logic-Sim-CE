using System.Linq;

namespace Assets.Scripts.Chip
{
    public class CustomChip : Chip
    {
        public InputSignal[] inputSignals;
        public OutputSignal[] outputSignals;

        public Pin pseudoInput;

        public int FolderIndex = 0;

        [UnityEngine.HideInInspector]
        public Pin[] unconnectedInputs;

        protected override void Start()
        {
            base.Start();
        }

        public void Init()
        {
            //GameObject pseudoPins = Instantiate(new GameObject("Pseudo Pins"),
            //parent: this.transform, false);
            Editable = true;
        }

        // Applies wire types from signals to pins
        public void ApplyWireModes()
        {
            foreach (var (pin, sig) in InputPins.Zip(inputSignals, (x, y) => (x, y)))
            {
                pin.WType = sig.OutputPins[0].WType;
            }
            foreach (var (pin, sig)
                        in OutputPins.Zip(outputSignals, (x, y) => (x, y)))
            {
                pin.WType = sig.InputPins[0].WType;
            }
        }

        public bool HasNoInputs
        {
            get { return InputPins.Length == 0; }
        }

        public override void ReceiveInputSignal(Pin pin)
        {
            base.ReceiveInputSignal(pin);
        }

        protected override void ProcessOutput()
        {
            // Send signals from input pins through the chip
            for (int i = 0; i < InputPins.Length; i++)
            {
                inputSignals[i].SendSignal(InputPins[i].State);
            }
            foreach (Pin pin in unconnectedInputs)
            {
                pin.ReceiveSignal(0);
                pin.Chip.ReceiveInputSignal(pin);
            }

            // Pass processed signals on to ouput pins
            for (int i = 0; i < OutputPins.Length; i++)
            {
                int outputState = outputSignals[i].InputPins[0].State;
                OutputPins[i].ReceiveSignal(outputState);
            }
        }

        public void ProcessOutputNoInputs() { ProcessOutput(); }
    }
}