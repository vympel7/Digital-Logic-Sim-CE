using UnityEngine;

namespace Assets.Scripts.Core
{
    using Scripts.Chip;

    public class IOScaler : MonoBehaviour
    {
        public enum Mode { Input, Output }
        public Mode mode;
        public Pin pin;
        public Transform wire;
        public Transform indicator;

        private CircleCollider2D _col;

        private void Awake() { _col = GetComponent<CircleCollider2D>(); }

        public void UpdateScale()
        {
            wire.transform.localScale = new Vector3(
                ScalingManager.PinSize, ScalingManager.WireThickness / 10, 1);
            float xPos = mode == Mode.Input ? ScalingManager.PinSize
                                            : ScalingManager.PinSize * -1;
            pin.transform.localPosition = new Vector3(xPos, 0, -0.1f);
            indicator.transform.localScale =
                new Vector3(ScalingManager.PinSize, ScalingManager.PinSize, 1);
            _col.radius = ScalingManager.PinSize / 2 * 1.25f;
            pin.SetScale();
        }
    }
}