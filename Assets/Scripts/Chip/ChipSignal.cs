using UnityEngine;

namespace Assets.Scripts.Chip
{
    // Base class for input and output signals
    public class ChipSignal : Chip
    {
        public int CurrentState;

        [SerializeField]
        private Graphics.Palette _palette;
        [SerializeField]
        private MeshRenderer _indicatorRenderer;
        [SerializeField]
        private MeshRenderer _pinRenderer;
        [SerializeField]
        private MeshRenderer _wireRenderer;

        public bool displayGroupDecimalValue { get; set; } = false;
        public bool useTwosComplement { get; set; } = true;
        public Pin.WireType wireType = Pin.WireType.Simple;
        public int GroupID { get; set; } = -1;

        [HideInInspector]
        public string signalName;
        protected bool interactable = true;

        public virtual void SetInteractable(bool interactable)
        {
            this.interactable = interactable;

            if (!interactable)
            {
                _indicatorRenderer.material.color = _palette.NonInteractableCol;
                _pinRenderer.material.color = _palette.NonInteractableCol;
                _wireRenderer.material.color = _palette.NonInteractableCol;
            }
        }

        public void SetDisplayState(int state)
        {

            if (_indicatorRenderer && interactable)
                _indicatorRenderer.material.color = (state == 1) ? _palette.OnCol : _palette.OffCol;
        }

        public static bool InSameGroup(ChipSignal signalA, ChipSignal signalB) => (signalA.GroupID == signalB.GroupID) && (signalA.GroupID != -1);

        public virtual void UpdateSignalName(string newName) => signalName = newName;
    }
}