using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace Assets.Scripts.UI.Menu
{
    public class SubmitMenu : UIMenu
    {
        public TMP_Text HeaderText;
        public TMP_Text ContentText;
        public Button SubmitButton;

        public void SetHeaderText(string text) => HeaderText.text = text;

        public void SetContentText(string text) => ContentText.text = text;

        public void SetOnSubmitAction(UnityAction action)
        {
            SubmitButton.onClick.RemoveAllListeners();
            SubmitButton.onClick.AddListener(action);
        }
    }
}