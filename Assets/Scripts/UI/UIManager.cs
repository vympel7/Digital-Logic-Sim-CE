using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.UI
{
    using Scripts.Core;
    using Scripts.Interaction;
    using Scripts.Graphics;
    using Menu;

    public enum MenuType
    {
        None = -1,
        CreateChipMenu = 0,
        EditChipMenu = 1,
        LoggingMenu = 2,
        NewFolderMenu = 3,
        SubmitMenu = 4,
        ClockMenu = 5,
        EditFolderMenu = 6,
    };

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        [Header("References")]
        public GameObject CreateButton;
        public GameObject UpdateButton;
        public GameObject OutsideMenuArea;
        public TMP_Text ChipName;

        public Utility.MenuDictionary Menus;

        public Palette Palette;
        public bool IsAnyMenuOpen => _openedMenu != null;
        private MenuType _currentMenuType = MenuType.None;

        private UIMenu _openedMenu;

        private ClockMenu _clockMenu;
        private EditChipMenu _editChipMenu;


        private void Awake()
        {
            Instance = this;
            foreach (var menu in Menus)
                menu.Value.Close();
            OutsideMenuArea.SetActive(false);

            _clockMenu = FindObjectOfType<ClockMenu>(true);
            _editChipMenu = FindObjectOfType<EditChipMenu>(true);

        }

        public static void NewSubmitMenu(string header, string text,
                                        UnityAction onSubmit)
        {
            SubmitMenu submitMenu = Instance.Menus[MenuType.SubmitMenu].GetComponent<SubmitMenu>();
            submitMenu.SetHeaderText(header);
            submitMenu.SetContentText(text);
            submitMenu.SetOnSubmitAction(onSubmit);
            Instance.OpenMenu(MenuType.SubmitMenu);
        }

        public void OpenCreateChipMenu() => OpenMenu(MenuType.CreateChipMenu);
        public void OpenMenu(MenuType menuType)
        {
            UIMenu newMenu = Menus[menuType];
            if (_openedMenu && _openedMenu != newMenu)
                CloseMenu();
            SetCurrentMenuState(newMenu, menuType);

            if (_openedMenu.ShowBG)
                OutsideMenuArea.SetActive(true);

            SetMenuPosition();
            _openedMenu.Open();

            SetActiveInteraction(false);
        }
        public void CloseMenu()
        {
            if (_openedMenu)
            {
                _openedMenu.Close();
                SetCurrentMenuState(null, MenuType.None);
            }
            OutsideMenuArea.SetActive(false);
            SetActiveInteraction(true);
        }

        private void SetActiveInteraction(bool IsActive)
        {
            FindObjectOfType<ChipInteraction>(true).gameObject.SetActive(IsActive);
            FindObjectOfType<PinAndWireInteraction>(true).gameObject.SetActive(IsActive);
        }

        public void SetEditorMode(ChipEditorMode newMode, string s = null)
        {
            CreateButton.SetActive(newMode == ChipEditorMode.Create);
            UpdateButton.SetActive(newMode == ChipEditorMode.Update);
            ChipName.text = newMode == ChipEditorMode.Update && s != null ? s : "";

        }

        private void SetCurrentMenuState(UIMenu newMenu, MenuType menuType)
        {
            _openedMenu = newMenu;
            _currentMenuType = menuType;
        }
        private void SetMenuPosition()
        {
            if (_currentMenuType == MenuType.EditChipMenu)
                SetChipEditMenuPosition();
            if (_currentMenuType == MenuType.ClockMenu)
                SetClockMenuPosition();
        }

        private void SetChipEditMenuPosition()
        {
            if (_currentMenuType != MenuType.EditChipMenu) return;
            foreach (GameObject obj in InputHelper.GetUIObjectsUnderMouse())
            {
                ButtonText buttonText = obj.GetComponent<ButtonText>();
                if (buttonText != null)
                {
                    _editChipMenu.EditChipInit(buttonText.ButtonTxt.text);
                    _openedMenu.transform.position = new Vector3(obj.transform.position.x, _openedMenu.transform.position.y, _openedMenu.transform.position.z);
                    RectTransform rect = _openedMenu.GetComponent<RectTransform>();
                    rect.anchoredPosition = new Vector2(Mathf.Clamp(rect.anchoredPosition.x, -800, 800), rect.anchoredPosition.y);
                    break;
                }
            }
        }

        private void SetClockMenuPosition()
        {
            var clock = InputHelper.GetObjectUnderMouse2D(1 << LayerMask.NameToLayer("Chip")).GetComponent<Chip.Clock>();
            if (clock != null)
            {
                _clockMenu.SetClockToEdit(clock);
                _openedMenu.transform.position = new Vector3(clock.transform.position.x, clock.transform.position.y - 2, _openedMenu.transform.position.z);
            }
        }

        public void OnClickOutsideMenu()
        {
            if (_openedMenu != null && _openedMenu.OnClickBG != null)
                _openedMenu.OnClickBG.Invoke();
        }
    }
}