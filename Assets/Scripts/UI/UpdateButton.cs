using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class UpdateButton : MonoBehaviour
    {
        public event System.Action OnChipUpdatePressed;

        public Button UpdatButton;

        public void Start()
        {
            UpdatButton.onClick.AddListener(ChipUpdatePressed);
        }

        private void ChipUpdatePressed()
        {
            OnChipUpdatePressed?.Invoke();
        }
    }
}