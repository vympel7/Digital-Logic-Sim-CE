using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.UI
{
    using Scripts.Core;
    using Scripts.Graphics;

    public class ChipEditorOptions : MonoBehaviour
    {
        public static ChipEditorOptions Instance;

        public enum PinNameDisplayMode
        {
            AltHover = 0,
            Hover = 1,
            AlwaysMain = 2,
            AlwaysAll = 3
        }

        public PinNameDisplayMode ActivePinNameDisplayMode;

        public Slider ScaleSlider;
        public TMP_Text DisplayPinNamesLabel;
        public Toggle ShowZoomHelperToggle;
        public Slider MouseWheelSensitivitySlider;
        public Slider CamMoveSpeedSlider;

        private void Awake() { Instance = this; }

        public void SetUIValues(ChipEditor editor)
        {
            OnDisplayPinNamesChanged(PlayerPrefs.GetInt("PinNameDisplayMode", 3));
            ShowZoomHelperToggle.SetIsOnWithoutNotify(
                PlayerPrefs.GetInt("ShowZoomHelper", 1) == 1);
            MouseWheelSensitivitySlider.SetValueWithoutNotify(
                PlayerPrefs.GetFloat("MouseSensitivity", 0.1f));
            CamMoveSpeedSlider.SetValueWithoutNotify(
                PlayerPrefs.GetFloat("CamMoveSpeed", 12f));
            ScaleSlider.SetValueWithoutNotify(editor.Data.Scale);
            ScalingManager.UpdateScale();
        }

        public void OnScaleChanged()
        {
            ScalingManager.Scale = ScaleSlider.value;
            ScalingManager.UpdateScale();
        }

        public void OnDisplayPinNamesChanged(int value)
        {
            switch (value)
            {
                case 0:
                    ActivePinNameDisplayMode = PinNameDisplayMode.AltHover;
                    DisplayPinNamesLabel.text = "Alt + Mouse Over";
                    break;
                case 1:
                    ActivePinNameDisplayMode = PinNameDisplayMode.Hover;
                    DisplayPinNamesLabel.text = "Mouse Over";
                    break;
                case 2:
                    ActivePinNameDisplayMode = PinNameDisplayMode.AlwaysMain;
                    DisplayPinNamesLabel.text = "Always Main";
                    break;
                case 3:
                    ActivePinNameDisplayMode = PinNameDisplayMode.AlwaysAll;
                    DisplayPinNamesLabel.text = "Always All";
                    break;
            }
            PlayerPrefs.SetInt("PinNameDisplayMode", value);
        }

        public void OnShowZoomHelperChanged()
        {
            ZoomManager.Instance.ShowZoomHelper = ShowZoomHelperToggle.isOn;
            PlayerPrefs.SetInt("ShowZoomHelper", ShowZoomHelperToggle.isOn ? 1 : 0);
        }

        public void OnMouseWheelSensitivityChanged()
        {
            ZoomManager.Instance.MouseWheelSensitivity =
                MouseWheelSensitivitySlider.value;
            PlayerPrefs.SetFloat("MouseSensitivity", MouseWheelSensitivitySlider.value);
        }

        public void OnCamMoveSpeedChanged()
        {
            ZoomManager.Instance.CamMoveSpeed = CamMoveSpeedSlider.value;
            PlayerPrefs.SetFloat("CamMoveSpeed", CamMoveSpeedSlider.value);
        }
    }
}