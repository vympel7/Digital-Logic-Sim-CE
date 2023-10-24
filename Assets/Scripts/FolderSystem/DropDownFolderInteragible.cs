using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.FolderSystem
{
    using Scripts.UI;

    public class DropDownFolderInteragible : MonoBehaviour
    {
        public UnityEvent<string> OnRightClick;

        public void RightClickHandler()
        {
            FindObjectOfType<UI.Menu.EditFolderMenu>().name = gameObject.name.Split(":")[1].Trim();
        }

        private void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(1))
            {
                string FolderName = name.Split(":")[1].Trim();
                if (FolderSystem.ReverseIndex(FolderName) > 2)
                {
                    UIManager.Instance.OpenMenu(MenuType.EditFolderMenu);
                    OnRightClick?.Invoke(FolderName);
                }
            }
        }
    }
}