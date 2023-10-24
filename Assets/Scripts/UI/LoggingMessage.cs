using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.UI
{
    public class LoggingMessage : MonoBehaviour
    {
        public Image IconImage;
        public TMP_Text HeaderText;
        public TMP_Text ContentText;

        public GameObject ContentHolder;

        public Sprite ArrowDown;
        public Sprite ArrowUp;

        public Button DropDownButon;

        private bool _open = false;

        public void ToggleOpen()
        {
            _open = !_open;
            DropDownButon.image.sprite = _open ? ArrowUp : ArrowDown;
            ContentHolder.gameObject.SetActive(_open);
        }
    }
}