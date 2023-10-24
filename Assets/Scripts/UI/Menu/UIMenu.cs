using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.UI.Menu
{
    public class UIMenu : MonoBehaviour
    {
        public bool ShowBG;
        public MenuType MenuType;
        public UnityEvent OnClickBG;
        [HideInInspector]
        public bool IsActive = false;

        public void Open()
        {
            IsActive = true;
            gameObject.SetActive(IsActive);
        }

        public void Close()
        {
            IsActive = false;
            gameObject.SetActive(IsActive);
        }
    }
}