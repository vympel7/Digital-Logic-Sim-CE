using UnityEngine;

namespace Assets.Scripts.Chip
{
    using Scripts.Core;

    public class Chip : MonoBehaviour
    {
        public string ChipName = "Untitled";
        public Pin[] InputPins;
        public Pin[] OutputPins;
        public bool Editable = false;
        // Number of input signals received (on current simulation step)
        private int _numInputSignalsReceived;
        private int _lastSimulatedFrame;
        private int _lastSimulationInitFrame;

        // Cached components
        [HideInInspector]
        public BoxCollider2D Bounds;

        protected virtual void Awake() { Bounds = GetComponent<BoxCollider2D>(); }

        protected virtual void Start() { SetPinIndices(); }

        public void InitSimulationFrame()
        {
            if (_lastSimulationInitFrame != Simulation.SimulationFrame)
            {
                _lastSimulationInitFrame = Simulation.SimulationFrame;
                ProcessCycleAndUnconnectedInputs();
            }
        }

        // Receive input signal from pin: either pin has power, or pin does not have
        // power. Once signals from all input pins have been received, calls the
        // ProcessOutput() function.
        public virtual void ReceiveInputSignal(Pin pin)
        {
            // Reset if on new step of simulation
            if (_lastSimulatedFrame != Simulation.SimulationFrame)
            {
                _lastSimulatedFrame = Simulation.SimulationFrame;
                _numInputSignalsReceived = 0;
                InitSimulationFrame();
            }

            _numInputSignalsReceived++;

            if (_numInputSignalsReceived == InputPins.Length)
            {
                ProcessOutput();
            }
        }

        private void ProcessCycleAndUnconnectedInputs()
        {
            for (int i = 0; i < InputPins.Length; i++)
            {
                if (InputPins[i].Cyclic)
                {
                    ReceiveInputSignal(InputPins[i]);
                }
                else if (!InputPins[i].HasParent)
                {
                    InputPins[i].ReceiveSignal(0);
                    // ReceiveInputSignal (inputPins[i]);
                }
            }
        }

        // Called once all inputs to the component are known.
        // Sends appropriate output signals to output pins
        protected virtual void ProcessOutput() { }

        private void SetPinIndices()
        {
            for (int i = 0; i < InputPins.Length; i++)
            {
                InputPins[i].Index = i;
            }
            for (int i = 0; i < OutputPins.Length; i++)
            {
                OutputPins[i].Index = i;
            }
        }

        public Vector2 BoundsSize => Bounds.size;
    }
}
