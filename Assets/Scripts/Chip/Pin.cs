using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Chip
{
    using Scripts.UI;

    public class Pin : MonoBehaviour
    {
        public enum WireType { Simple, Bus4, Bus8, Bus16, Bus32 }
        public enum PinType { ChipInput, ChipOutput }
        public PinType PType;

        public WireType WType;

        // The chip that this pin is attached to (either as an input or output
        // terminal)
        public Chip Chip;
        public string PinName;

        [HideInInspector]
        public bool Cyclic;
        // Index of this pin in its associated chip's input or output pin array
        [HideInInspector]
        public int Index;
        // The pin from which this pin receives its input signal
        // (multiple inputs not allowed in order to simplify simulation)
        [HideInInspector]
        public Pin ParentPin;
        // The pins which this pin forwards its signal to
        [HideInInspector]
        public List<Pin> ChildPins = new List<Pin>();
        // Current state of the pin: 0 == LOW, 1 == HIGH
        private int CurrentState;
        private bool Interact;

        // Appearance
        private Color _defaultCol = Color.black;
        private Color _interactCol;
        private Color _onCol;
        private Color _onColBus;
        private Material _material;
        private bool _simActive;

        public static float Radius => Core.ScalingManager.PinSize / 2 / 2;

        public static float InteractionRadius => Radius * 1.1f;

        private void Awake()
        {
            _material = GetComponent<MeshRenderer>().material;
            _material.color = _defaultCol;
            Interact = false;
        }

        private void Start()
        {
            _simActive = Core.Simulation.Instance.Active;
            _interactCol = UIManager.Instance.Palette.SelectedColor;
            _onCol = UIManager.Instance.Palette.OnCol;
            _onColBus = UIManager.Instance.Palette.BusColor;
            SetScale();
        }

        public void SetScale() { transform.localScale = Vector3.one * Radius * 2; }

        public void TellPinSimIsOff()
        {
            _simActive = false;
            UpdateColor();
        }

        public void TellPinSimIsOn()
        {
            _simActive = true;
            UpdateColor();
        }

        public void UpdateColor()
        {
            if (_material)
            {
                Color newColor = new Color();
                if (Interact)
                {
                    newColor = _interactCol;
                }
                else if (_simActive && CurrentState == 1)
                {
                    newColor = WType == WireType.Simple ? _onCol : _onColBus;
                }
                else
                {
                    newColor = _defaultCol;
                }
                if (_material.color != newColor)
                {
                    _material.color = newColor;
                }
            }
        }

        // Get the current state of the pin: 0 == LOW, 1 == HIGH
        public int State
        {
            get { return CurrentState; }
        }

        // Note that for ChipOutput pins, the chip itself is considered the parent, so
        // will always return true Otherwise, only true if the parentPin of this pin
        // has been set
        public bool HasParent
        {
            get { return ParentPin != null || PType == PinType.ChipOutput; }
        }

        // Receive signal: 0 == LOW, 1 = HIGH
        // Sets the current state to the signal
        // Passes the signal on to any connected pins / electronic component
        public void ReceiveSignal(int signal)
        {
            CurrentState = signal;
            if (PType == PinType.ChipInput && !Cyclic)
            {
                Chip.ReceiveInputSignal(this);
            }
            else if (PType == PinType.ChipOutput)
            {
                for (int i = 0; i < ChildPins.Count; i++)
                {
                    ChildPins[i].ReceiveSignal(signal);
                }
            }
            UpdateColor();
        }

        public static void MakeConnection(Pin pinA, Pin pinB)
        {
            if (IsValidConnection(pinA, pinB))
            {
                Pin parentPin = (pinA.PType == PinType.ChipOutput) ? pinA : pinB;
                Pin childPin = (pinA.PType == PinType.ChipInput) ? pinA : pinB;

                parentPin.ChildPins.Add(childPin);
                childPin.ParentPin = parentPin;
            }
        }

        public static void RemoveConnection(Pin pinA, Pin pinB)
        {
            Pin parentPin = (pinA.PType == PinType.ChipOutput) ? pinA : pinB;
            Pin childPin = (pinA.PType == PinType.ChipInput) ? pinA : pinB;

            parentPin.ChildPins.Remove(childPin);
            childPin.ParentPin = null;
        }

        public static bool IsValidConnection(Pin pinA, Pin pinB)
        {
            // Connection failes when pin wire types are different
            if (pinA.WType != pinB.WType)
                return false;
            // Connection is valid if one pin is an output pin, and the other is an
            // input pin
            return pinA.PType != pinB.PType;
        }

        public static bool TryConnect(Pin pinA, Pin pinB)
        {

            if (pinA.PType != pinB.PType)
            {
                Pin parentPin = (pinA.PType == PinType.ChipOutput) ? pinA : pinB;
                Pin childPin = (parentPin == pinB) ? pinA : pinB;
                parentPin.ChildPins.Add(childPin);
                childPin.ParentPin = parentPin;
                return true;
            }
            return false;
        }

        public void MouseEnter()
        {
            Interact = true;
            transform.localScale = Vector3.one * InteractionRadius * 2;
            UpdateColor();
        }

        public void MouseExit()
        {
            Interact = false;
            transform.localScale = Vector3.one * Radius * 2;
            UpdateColor();
        }
    }
}