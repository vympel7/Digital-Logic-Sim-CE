using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core
{
    using Scripts.Chip;
    using Scripts.Graphics;

    public class Simulation : MonoBehaviour
    {
        public static Simulation Instance;

        public static int SimulationFrame { get; private set; }

        private InputSignal[] _inputSignals;
        private ChipEditor _chipEditor;
        public bool Active = false;

        public float MinStepTime = 0.075f;
        private float _lastStepTime;

        private List<CustomChip> _standaloneChips = new List<CustomChip>();

        public void ToogleActive()
        {
            // Method called by the "Run/Stop" button that toogles simulation
            // active/inactive
            Active = !Active;

            SimulationFrame++;
            if (Active)
                ResumeSimulation();
            else
                StopSimulation();

        }

        private void Awake()
        {
            Instance = this;
            SimulationFrame = 0;
        }

        private void Update()
        {
            // If simulation is off StepSimulation is not executed.
            if (Time.time - _lastStepTime > MinStepTime && Active)
            {
                _lastStepTime = Time.time;
                SimulationFrame++;
                StepSimulation();
            }
        }

        private void StepSimulation()
        {
            RefreshChipEditorReference();
            ClearOutputSignals();
            InitChips();
            ProcessInputs();
        }

        public void ResetSimulation()
        {
            StopSimulation();
            SimulationFrame = 0;

            if (Active)
            {
                FindObjectOfType<UI.RunButton>().SetOff();
                Active = false;
            }
        }

        private void ClearOutputSignals()
        {
            List<ChipSignal> outputSignals = _chipEditor.OutputsEditor.Signals;
            for (int i = 0; i < outputSignals.Count; i++)
            {
                outputSignals[i].SetDisplayState(0);
                outputSignals[i].CurrentState = 0;
            }
        }

        private void ProcessInputs()
        {
            List<ChipSignal> inputSignals = _chipEditor.InputsEditor.Signals;
            for (int i = 0; i < inputSignals.Count; i++)
            {
                ((InputSignal)inputSignals[i]).SendSignal();
            }
            foreach (Chip chip in _chipEditor.ChipInteraction.AllChips)
            {
                if (chip is CustomChip custom)
                {
                    // if (custom.HasNoInputs) {
                    // 	custom.ProcessOutputNoInputs();
                    // }
                    custom.pseudoInput?.ReceiveSignal(0);
                    if (custom.pseudoInput != null)
                    {
                    }
                }
            }
        }

        private void StopSimulation()
        {
            RefreshChipEditorReference();

            var allWires = _chipEditor.PinAndWireInteraction.allWires;
            // Tell all wires the simulation is inactive makes them all inactive (gray
            // colored)
            foreach (Wire wire in allWires)
                wire.tellWireSimIsOff();
            foreach (Pin pin in _chipEditor.PinAndWireInteraction.AllVisiblePins())
                pin.tellPinSimIsOff();

            // If sim is not active all output signals are set with a temporal value of
            // 0 (group signed/unsigned displayed value) and get gray colored (turned
            // off)
            ClearOutputSignals();
        }

        private void ResumeSimulation()
        {
            StepSimulation();

            foreach (Pin pin in _chipEditor.PinAndWireInteraction.AllVisiblePins())
                pin.tellPinSimIsOn();

            var allWires = _chipEditor.PinAndWireInteraction.allWires;

            // Tell all wires the simulation is active makes them all active (dynamic
            // colored based on the circuits logic)
            foreach (Wire wire in allWires)
                wire.tellWireSimIsOn();
        }

        private void InitChips()
        {
            var AllChips = _chipEditor.ChipInteraction.AllChips;

            foreach (Chip chip in AllChips)
                chip.InitSimulationFrame();
        }

        private void RefreshChipEditorReference()
        {
            if (_chipEditor == null)
                _chipEditor = Manager.ActiveChipEditor;
        }
    }
}