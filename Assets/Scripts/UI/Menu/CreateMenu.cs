using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Menu
{
    using Scripts.Core;

    public class CreateMenu : MonoBehaviour
    {
        public TMP_InputField ChipNameField;
        public TMP_Dropdown FolderDropdown;
        public Button DoneButton;
        public Slider HueSlider;
        public Slider SaturationSlider;
        public Slider ValueSlider;
        [Range(0, 1)]
        public float TextColThreshold = 0.5f;

        public Color[] SuggestedColours;
        private int _suggestedColourIndex;

        private string _validChars =
            "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789()[]-";

        private List<string> _allChipNames = new List<string>();

        private void Awake()
        {
            _suggestedColourIndex = Random.Range(0, SuggestedColours.Length);
        }

        void Update()
        {
            if (UIManager.Instance.Menus[MenuType.CreateChipMenu].IsActive)
            {
                // Force name input field to remain focused
                if (!ChipNameField.isFocused)
                {
                    ChipNameField.Select();
                    // Put caret at end of text (instead of selecting the text, which is
                    // annoying in this case)
                    ChipNameField.caretPosition = ChipNameField.text.Length;
                }
            }
        }

        public void SelectFolder()
        {
            string DropDownTextValue = FolderDropdown.options[FolderDropdown.value].text;
            Manager.ActiveChipEditor.Data.FolderIndex = FolderSystem.FolderSystem.ReverseIndex(DropDownTextValue);
        }

        public void ColourSliderChanged()
        {
            Color chipCol = Color.HSVToRGB(HueSlider.value, SaturationSlider.value,
                                        ValueSlider.value);
            UpdateColour(chipCol);
        }

        public void ChipNameFieldChanged(bool endEdit = false)
        {
            string text = ChipNameField.text.ToUpper();
            string validName = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (i < 12 && _validChars.Contains(text[i].ToString()))
                {
                    validName += text[i];
                }
            }
            validName = endEdit ? validName.Trim() : validName.TrimStart();

            if (IsAvailableName(validName) && validName.Length > 0)
            {
                Manager.ActiveChipEditor.Data.Name = validName;
                DoneButton.interactable = true;
            }
            else
            {
                DoneButton.interactable = false;
            }
            ChipNameField.text = validName;
        }

        private bool IsAvailableName(string chipName)
        {
            return !_allChipNames.Contains(chipName);
        }

        public void Prepare()
        {
            _allChipNames = Manager.Instance.AllChipNames();
            FolderDropdown.ClearOptions();
            var ddopt = ChipBarUI.Instance.FolderDropdown.options;
            FolderDropdown.AddOptions(ddopt.GetRange(1, ddopt.Count - 2));
            DoneButton.interactable = false;
            ChipNameField.SetTextWithoutNotify("");
            SetSuggestedColour();
        }

        public void FinishCreation()
        {
            Manager.ActiveChipEditor.Data.FolderIndex = FolderSystem.FolderSystem.ReverseIndex(FolderDropdown.options[FolderDropdown.value].text);
            Manager.ActiveChipEditor.Data.Scale = ScalingManager.Scale;
            Manager.Instance.SaveAndPackageChip();
        }

        private void SetSuggestedColour()
        {
            Color suggestedChipColour = SuggestedColours[_suggestedColourIndex];
            suggestedChipColour.a = 1;
            _suggestedColourIndex = (_suggestedColourIndex + 1) % SuggestedColours.Length;

            float hue, sat, val;
            Color.RGBToHSV(suggestedChipColour, out hue, out sat, out val);
            HueSlider.SetValueWithoutNotify(hue);
            SaturationSlider.SetValueWithoutNotify(sat);
            ValueSlider.SetValueWithoutNotify(val);
            UpdateColour(suggestedChipColour);
        }

        private void UpdateColour(Color chipCol)
        {
            var cols = ChipNameField.colors;
            cols.normalColor = chipCol;
            cols.highlightedColor = chipCol;
            cols.selectedColor = chipCol;
            cols.pressedColor = chipCol;
            ChipNameField.colors = cols;

            float luma = chipCol.r * 0.213f + chipCol.g * 0.715f + chipCol.b * 0.072f;
            Color chipNameCol = (luma > TextColThreshold) ? Color.black : Color.white;
            ChipNameField.textComponent.color = chipNameCol;

            Manager.ActiveChipEditor.Data.Colour = chipCol;
            Manager.ActiveChipEditor.Data.NameColour = ChipNameField.textComponent.color;
        }
    }
}