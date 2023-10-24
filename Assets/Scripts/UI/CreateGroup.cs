using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class CreateGroup : MonoBehaviour
    {
        public event System.Action<int> OnGroupSizeSettingPressed;

        public TMP_InputField GroupSizeInput;
        public Button SetSizeButton;
        public GameObject MenuHolder;
        private bool _menuActive;

        private int _groupSizeValue;

        private void Start()
        {
            _menuActive = false;
            _groupSizeValue = 8;
            SetSizeButton.onClick.AddListener(SetGroupSize);
            GroupSizeInput.onValueChanged.AddListener(SetCurrentText);
        }

        private void SetCurrentText(string groupSize)
        {
            if (groupSize != "" && groupSize != "-")
            {
                int result = int.Parse(groupSize);
                result = result <= 1 ? 1 : result;
                _groupSizeValue = result > 16 ? 16 : result;
                GroupSizeInput.SetTextWithoutNotify(_groupSizeValue.ToString());
            }
            else if (groupSize == "-")
            {
                GroupSizeInput.SetTextWithoutNotify("");
            }
        }

        public void CloseMenu()
        {
            OnGroupSizeSettingPressed.Invoke(_groupSizeValue);
            _menuActive = false;
            MenuHolder.SetActive(false);
        }

        public void OpenMenu()
        {
            _menuActive = true;
            MenuHolder.SetActive(true);
        }

        private void SetGroupSize()
        {
            if (_menuActive)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }
    }
}