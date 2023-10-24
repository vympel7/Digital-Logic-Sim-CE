using UnityEngine;

namespace Assets.Scripts.Core
{
    using Scripts.Chip;

    public class ScalingManager : MonoBehaviour
    {
        public static ScalingManager Instance;

        public static float Scale = 1f;

        private const float _maxPinSize = 0.4f;
        private const float _minPinSize = 0.1f;
        public static float PinSize = 0.4f;
        public static float HandleSizeY = 0.4f;

        private const float _maxFontSize = 1.75f;
        private const float _minFontSize = 0.3f;
        public static float FontSize = 1.75f;

        private const float _maxPinDisplayFontSize = 1.75f;
        private const float _minPinDisplayFontSize = 0.3f;
        public static float PinDisplayFontSize = 1.75f;

        private const float _maxPackageFontSize = 2.5f;
        private const float _minPackageFontSize = 0.5f;
        public static float PackageFontSize = 2.5f;

        private const float _maxChipInteractionBoundsBorder = 0.25f;
        private const float _minChipInteractionBoundsBorder = 0.05f;
        public static float ChipInteractionBoundsBorder = 0.25f;

        private const float _maxChipStackSpace = 0.15f;
        private const float _minChipStackSpace = 0.05f;
        public static float ChipStackSpace = 0.15f;

        private const float _maxWireThickness = 0.5f;
        private const float _minWireThickness = 0.1f;
        public static float WireThickness = 0.5f;
        public static float WireSelectedThickness = 0.5f;

        private const float _maxPinDisplayPadding = 0.1f;
        private const float _minPinDisplayPadding = 0.02f;
        public static float PinDisplayPadding = 0.1f;

        private const float _maxPinDisplayTextOffset = 0f;
        private const float _minPinDisplayTextOffset = -0.005f;
        public static float PinDisplayTextOffset = 0f;

        private const float _maxIOBarDistance = 8.15f;
        private const float _minIOBarDistance = 7.85f;
        public static float IoBarDistance = 8.15f;

        private const float _maxIOBarGraphicWidth = 1f;
        private const float _minIOBarGraphicWidth = 0.5f;
        public static float IoBarGraphicWidth = 1f;

        private const float _maxGroupSpacing = 0.22f;
        private const float _minGroupSpacing = 0.055f;
        public static float GroupSpacing = 0.22f;

        private const float _maxPropertiesUIX = 1.45f;
        private const float _minPropertiesUIX = 1.1f;
        public static float PropertiesUIX = 1.45f;

        private const float _maxPropertiesUIXZoom = 0.8f;
        private const float _minPropertiesUIXZoom = 0;
        private float _propertiesUIXZoom = 0f;

        void Awake() { Instance = this; }

        void Update()
        {
            _propertiesUIXZoom = Mathf.Lerp(_minPropertiesUIXZoom, _maxPropertiesUIXZoom,
                                        ZoomManager.Zoom);
            PropertiesUIX = Mathf.Lerp(_minPropertiesUIX, _maxPropertiesUIX, Scale) -
                            _propertiesUIXZoom;
        }

        private static void CalcValues()
        {
            Scale = Mathf.Clamp01(Scale);

            PinSize = Mathf.Lerp(_minPinSize, _maxPinSize, Scale);
            FontSize = Mathf.Lerp(_minFontSize, _maxFontSize, Scale);
            ChipInteractionBoundsBorder = Mathf.Lerp(
                _minChipInteractionBoundsBorder, _maxChipInteractionBoundsBorder, Scale);
            ChipStackSpace = Mathf.Lerp(_minChipStackSpace, _maxChipStackSpace, Scale);
            PinDisplayPadding =
                Mathf.Lerp(_minPinDisplayPadding, _maxPinDisplayPadding, Scale);
            PinDisplayTextOffset =
                Mathf.Lerp(_minPinDisplayTextOffset, _maxPinDisplayTextOffset, Scale);
            IoBarDistance = Mathf.Lerp(_minIOBarDistance, _maxIOBarDistance, Scale);
            IoBarGraphicWidth =
                Mathf.Lerp(_minIOBarGraphicWidth, _maxIOBarGraphicWidth, Scale);
            GroupSpacing = Mathf.Lerp(_minGroupSpacing, _maxGroupSpacing, Scale);

            PinDisplayFontSize =
                Mathf.Clamp(FontSize, _minPinDisplayFontSize, _maxPinDisplayFontSize);
            PackageFontSize =
                Mathf.Clamp(FontSize * 1.5f, _minPackageFontSize, _maxPackageFontSize);
            WireThickness =
                Mathf.Clamp(PinSize * 1.5f, _minWireThickness, _maxWireThickness);
            WireSelectedThickness = WireThickness * 1.5f;

            HandleSizeY = PinSize;
        }

        public static void UpdateScale()
        {
            Graphics.ChipEditor chipEditor = Manager.ActiveChipEditor;
            if (chipEditor)
            {
                CalcValues();

                chipEditor.UpdateChipSizes();
                chipEditor.PinNameDisplayManager.UpdateTextSize(PinDisplayFontSize);
                chipEditor.InputsEditor.UpdateScale();
                chipEditor.OutputsEditor.UpdateScale();

                foreach (Chip chip in chipEditor.ChipInteraction.AllChips)
                {
                    foreach (Pin pin in chip.InputPins)
                    {
                        pin.SetScale();
                    }
                    foreach (Pin pin in chip.OutputPins)
                    {
                        pin.SetScale();
                    }
                }
                foreach (IOScaler scaler in FindObjectsOfType<IOScaler>())
                {
                    scaler.UpdateScale();
                }
            }
        }
    }
}