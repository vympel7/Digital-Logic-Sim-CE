using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    using Scripts.Core;
    public class PinNameDisplay : MonoBehaviour
    {
        public TMP_Text NameUI;
        public Transform Background;
        public Vector2 BackgroundPadding;

        public void Set(Chip.Pin pin)
        {
            NameUI.fontSize = ScalingManager.PinDisplayFontSize;

            if (string.IsNullOrEmpty(pin.PinName))
            {
                NameUI.text = "UNNAMED PIN";
            }
            else
            {
                NameUI.text = pin.PinName;
            }

            BackgroundPadding.x = ScalingManager.PinDisplayPadding;
            NameUI.rectTransform.localPosition =
                new Vector3(NameUI.rectTransform.localPosition.x,
                            ScalingManager.PinDisplayTextOffset,
                            NameUI.rectTransform.localPosition.z);

            float backgroundSizeX = NameUI.preferredWidth + BackgroundPadding.x;
            float backgroundSizeY = NameUI.preferredHeight + BackgroundPadding.y;
            Background.localScale = new Vector3(backgroundSizeX, backgroundSizeY, 1);

            float spacingFromPin = (backgroundSizeX / 2 + Chip.Pin.InteractionRadius * 1.5f);
            spacingFromPin *= (pin.PType == Chip.Pin.PinType.ChipInput) ? -1 : 1;
            transform.position = pin.transform.position +
                                Vector3.right * spacingFromPin + Vector3.forward * -1;
        }
    }
}