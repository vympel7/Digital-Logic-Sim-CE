using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class RunButton : MonoBehaviour
    {
        public Button Button;
        public Core.Simulation Sim;
        public Color OnCol;
        public Color OffCol;

        void Start() { Button.targetGraphic.color = Sim.Active ? OnCol : OffCol; }

        public void ToggleSimulationActive()
        {
            Sim.ToogleActive();
            Button.targetGraphic.color = Sim.Active ? OnCol : OffCol;
        }

        public void SetOff() { Button.targetGraphic.color = OffCol; }

        private void OnValidate()
        {
            if (Button == null)
                Button = GetComponent<Button>();
        }
    }
}