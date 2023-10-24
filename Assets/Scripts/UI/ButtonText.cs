using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class ButtonText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Button Button;
        public TMPro.TMP_Text ButtonTxt;
        public Color NormalCol = Color.white;
        public Color NonInteractableCol = Color.grey;
        public Color HighlightedCol = Color.white;
        private bool _highlighted;

        private void Start() { }

        private void Update()
        {
            Color col = (_highlighted) ? HighlightedCol : NormalCol;
            ButtonTxt.color = (Button.interactable) ? col : NonInteractableCol;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Button.interactable)
            {
                _highlighted = true;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _highlighted = false;
        }

        private void OnEnable()
        {
            _highlighted = false;
        }

        private void OnValidate()
        {
            if (Button == null)
            {
                Button = GetComponent<Button>();
            }
            if (ButtonTxt == null)
            {
                ButtonTxt = transform.GetComponentInChildren<TMPro.TMP_Text>();
            }
        }
    }
}