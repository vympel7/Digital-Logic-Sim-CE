using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Menu
{
    using Scripts.FolderSystem;

    public class NewFolderMenu : MonoBehaviour
    {
        private ChipBarUI _chipBarUI;
        public TMP_InputField NewFolderNameField;
        public Button SubmitNewFolder;

        private void Start()
        {
            _chipBarUI = ChipBarUI.Instance;
        }

        public void NewFolder()
        {
            string newFolderName = NewFolderNameField.text;

            NewFolderNameField.SetTextWithoutNotify("");
            SubmitNewFolder.interactable = false;

            _chipBarUI.AddFolderView(FolderSystem.AddFolder(newFolderName), _chipBarUI.UserSprite);
        }

        public void CheckFolderName(bool endEdit = false)
        {
            var validName = FolderNameValidator.ValidateFolderName(NewFolderNameField.text, endEdit);

            SubmitNewFolder.interactable = validName.Length > 0 && FolderSystem.FolderNameAvailable(validName);
            NewFolderNameField.SetTextWithoutNotify(validName);
        }
    }
}