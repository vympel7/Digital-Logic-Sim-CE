using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    public class MenuBarButton : MonoBehaviour,
                                IPointerEnterHandler,
                                IPointerExitHandler
    {

        public GameObject Dropdown;

        public GameObject[] SubMenus;

        private void Start()
        {
            Dropdown.SetActive(false);
            foreach (GameObject subMenu in SubMenus)
            {
                subMenu.SetActive(false);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Dropdown.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Dropdown.SetActive(false);
            foreach (GameObject subMenu in SubMenus)
                subMenu.SetActive(false);
        }
    }
}