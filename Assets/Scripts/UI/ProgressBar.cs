using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class ProgressBar : MonoBehaviour
    {
        public static ProgressBar Instance;
        public GameObject LoadingScreen;
        public Slider ProgBar;
        public Image Fill;
        public TMP_Text TitleText;
        public TMP_Text InfoText;
        public TMP_Text IndicatorText;

        public Color[] SuggestedColours;

        private void Awake() { Instance = this; }

        public static ProgressBar New(string title = "Loading...", bool wholeNumbers = false)
        {
            int suggestedColourIndex =
                Random.Range(0, Instance.SuggestedColours.Length);
            Color randomColor = Instance.SuggestedColours[suggestedColourIndex];
            randomColor.a = 1;
            Instance.Fill.color = randomColor;
            Instance.ProgBar.wholeNumbers = wholeNumbers;

            Instance.InfoText.text = "Start Loading...";
            Instance.TitleText.text = title;
            Instance.LoadingScreen.SetActive(true);
            return Instance;
        }

        public void Open(float minValue, float maxValue)
        {
            ProgBar.minValue = minValue;
            ProgBar.maxValue = maxValue;
            LoadingScreen.SetActive(true);
        }

        public void Close() { LoadingScreen.SetActive(false); }

        public void SetValue(float value, string info = "")
        {
            InfoText.text = info;
            IndicatorText.text =
                value.ToString() + "/" + ProgBar.maxValue.ToString();
            ProgBar.value = value;
        }
    }
}