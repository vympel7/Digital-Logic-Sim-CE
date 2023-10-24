using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class InputFieldValidator : MonoBehaviour
    {
        public TMP_InputField InputField;
        public string ValidChars = "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789()[]<>";

        private void Awake()
        {
            InputField.onValueChanged.AddListener(OnEdit);
        }

        private void OnEdit(string newString)
        {
            string validString = "";
            for (int i = 0; i < newString.Length; i++)
                if (ValidChars.Contains(newString[i].ToString()))
                    validString += newString[i];

            InputField.SetTextWithoutNotify(validString);
        }

        private void OnValidate()
        {
            if (InputField == null)
                InputField = GetComponent<TMP_InputField>();

        }
    }
}