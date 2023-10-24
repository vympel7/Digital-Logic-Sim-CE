using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Menu
{
    using Scripts.Chip;

    public class ClockMenu : MonoBehaviour
    {
        private Clock _currentEditingClock;
        public TMP_InputField _HzInputField;
        public Button DoneButton;

        private void Start()
        {
            DoneButton.onClick.AddListener(Done);
            _HzInputField.onEndEdit.AddListener(FinishedEdit);
        }

        public void FinishedEdit(string str)
        {
            var HzStr = Regex.Match(str, @"^\d+([\.,]\d+)?").Value;
            _HzInputField.text = (HzStr == "" ? _currentEditingClock.Hz.ToString() : HzStr) + "Hz";
        }

        public void SetClockToEdit(Clock Clock)
        {
            _currentEditingClock = Clock;
            _HzInputField.text = $"{Clock.Hz} Hz";
        }

        public void Done()
        {
            if (_currentEditingClock == null) return;

            var HzStr = Regex.Match(_HzInputField.text, @"^\d+([\.,]\d+)?").Value.Replace(",", ".");
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";
            _currentEditingClock.Hz = float.Parse(HzStr, NumberStyles.Any, ci);
        }
    }
}