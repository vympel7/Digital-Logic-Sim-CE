using System.Linq;

namespace Assets.Scripts.Chip
{
    // Provides input signal (0 or 1) to a chip.
    // When designing a chip, this input signal can be manually set to 0 or 1 by the player.
    public class InputSignal : ChipSignal
    {
        protected override void Start()
        {
            base.Start();
            SetCol();
        }

        public void ToggleActive()
        {
            CurrentState = 1 - CurrentState;
            SetCol();
        }

        public void SetState(int state)
        {
            CurrentState = state >= 1 ? 1 : 0;
            SetCol();
        }

        public void SendSignal(int signal)
        {
            CurrentState = signal;
            OutputPins[0].ReceiveSignal(signal);
            SetCol();
        }

        public void SendOffSignal()
        {
            OutputPins[0].ReceiveSignal(0);
            SetCol();
        }

        public void SendSignal()
        {
            OutputPins[0].ReceiveSignal(CurrentState);
        }

        private void SetCol()
        {
            SetDisplayState(CurrentState);
        }

        public override void UpdateSignalName(string newName)
        {
            base.UpdateSignalName(newName);
            OutputPins[0].PinName = newName;
        }

        private void OnMouseDown()
        {
            // Allow only to click on single wires, not on bus wires
            if (OutputPins.All(x => x.WType == Pin.WireType.Simple))
                ToggleActive();
        }
    }
}