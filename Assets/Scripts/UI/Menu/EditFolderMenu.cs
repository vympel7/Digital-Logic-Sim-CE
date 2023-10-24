using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Menu
{
    using Scripts.FolderSystem;

    public class EditFolderMenu : MonoBehaviour
    {
        private ChipBarUI _chipBarUI;
        public TMP_InputField RenamingFolderField;
        public TMP_Text RenamingTextLabel;
        public Button OKRenameFolder;
        private string _folderName = "";

        private void Start()
        {
            _chipBarUI = ChipBarUI.Instance;
        }

        public void RenameFolder()
        {
            string newFolderName = RenamingFolderField.text;
            RenamingFolderField.SetTextWithoutNotify("");
            OKRenameFolder.interactable = false;

            FolderSystem.RenameFolder(_folderName, newFolderName);
            _chipBarUI.NotifyFolderNameChanged();
        }

        public void SubmitDeleteFolder()
        {
            UIManager.NewSubmitMenu(header: "Delete Folder",
                            text: "Are you sure you want to delete the folder '" +
                                _folderName +
                                "'?\nIt will be lost forever!",
                            onSubmit: DeleteFolder);

        }

        public void DeleteFolder()
        {
            FolderSystem.DeleteFolder(_folderName);
            _chipBarUI.NotifyRemovedFolder(_folderName);
        }

        public void InitMenu(string name) // call from Editor
        {
            _folderName = name;
            RenamingTextLabel.text = name;
            RenamingFolderField.Select();
        }

        public void CheckFolderName(bool endEdit = false)
        {
            var validName = FolderNameValidator.ValidateFolderName(RenamingFolderField.text, endEdit);

            OKRenameFolder.interactable = validName.Length > 0 && FolderSystem.FolderNameAvailable(validName);
            RenamingFolderField.SetTextWithoutNotify(validName);
        }
    }
}