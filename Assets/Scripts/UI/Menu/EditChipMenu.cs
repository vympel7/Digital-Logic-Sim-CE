using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

namespace Assets.Scripts.UI.Menu
{
    using Scripts.Chip;
    using Scripts.Core;
    using Scripts.SaveSystem;
    using Scripts.SaveSystem.Serializable;

    public class EditChipMenu : MonoBehaviour
    {
        public TMP_InputField ChipNameField;
        public Button DoneButton;
        public Button DeleteButton;
        public Button ViewButton;
        public Button ExportButton;
        public TMP_Dropdown FolderDropdown;

        private Chip _currentChip;
        private string _nameBeforeChanging;

        private string _currentFolderText { get => FolderDropdown.options[FolderDropdown.value].text; }

        private void Awake()
        {
            ChipNameField.onValueChanged.AddListener(ChipNameFieldChanged);
            DoneButton.onClick.AddListener(FinishCreation);
            DeleteButton.onClick.AddListener(SubmitDeleteChip);
            ViewButton.onClick.AddListener(ViewChip);
            ExportButton.onClick.AddListener(ExportChip);
        }

        public void EditChipInit(string chipName)
        {

            ChipNameField.text = chipName;
            _nameBeforeChanging = chipName;
            DoneButton.interactable = true;
            var IsSafeToDelate = ChipSaver.IsSafeToDelete(_nameBeforeChanging);
            ChipNameField.interactable = IsSafeToDelate;
            DeleteButton.interactable = IsSafeToDelate;

            _currentChip = Manager.GetChipByName(chipName);
            ViewButton.interactable = true;
            ExportButton.interactable = true;

            FolderDropdown.ClearOptions();
            var FolderOption = ChipBarUI.Instance.FolderDropdown.options;
            FolderDropdown.AddOptions(FolderOption.GetRange(1, FolderOption.Count - 2));


            if (_currentChip is CustomChip customChip)
            {
                for (int i = 0; i < FolderDropdown.options.Count; i++)
                {

                    if (FolderSystem.FolderSystem.CompareValue(customChip.FolderIndex, FolderDropdown.options[i].text))
                    {
                        FolderDropdown.value = i;
                        break;
                    }
                }
            }
        }

        public void ChipNameFieldChanged(string value)
        {
            string formattedName = value.ToUpper();
            DoneButton.interactable = IsValidChipName(formattedName.Trim());
            ChipNameField.text = formattedName;
        }


        public bool IsValidRename(string chipName)
        {
            // Name has not changed
            if (string.Equals(_nameBeforeChanging, chipName))
                return true;
            // Name is either empty or in builtin chips
            if (!IsValidChipName(chipName))
                return false;

            SavedChip[] savedChips = SaveSystem.GetAllSavedChips();
            for (int i = 0; i < savedChips.Length; i++)
            {
                // Name already exists in custom chips
                if (savedChips[i].Data.Name == chipName)
                    return false;
            }
            return true;
        }

        public bool IsValidChipName(string chipName)
        {
            // If chipName is not in list of builtin chips then is a valid name
            return !Manager.Instance.AllChipNames(builtin: true, custom: false)
                        .Contains(chipName) && chipName.Length > 0;
        }

        public void SubmitDeleteChip()
        {
            UIManager.NewSubmitMenu(header: "Delete Chip",
                                    text: $"Are you sure you want to delete the chip '{_currentChip.ChipName}'? \nIt will be lost forever!",
                                    onSubmit: DeleteChip);
        }

        public void DeleteChip()
        {
            ChipSaver.Delete(_nameBeforeChanging);
            Manager.Instance.DeleteChip(_nameBeforeChanging);
            FindObjectOfType<Interaction.ChipInteraction>().DeleteChip(_currentChip);

            ReloadChipBar();


            DLSLogger.Log($"Successfully deleted chip '{_currentChip.ChipName}'");
            _currentChip = null;
        }

        public void ReloadChipBar()
        {
            ChipBarUI.Instance.ReloadChipButton();
        }

        public void FinishCreation()
        {
            if (ChipNameField.text != _nameBeforeChanging)
            {
                // Chip has been renamed
                var NameAfterChanging = ChipNameField.text.Trim();
                ChipSaver.Rename(_nameBeforeChanging, NameAfterChanging);
                Manager.Instance.RenameChip(_nameBeforeChanging, NameAfterChanging);

                ReloadChipBar();
            }
            if (_currentChip is CustomChip customChip)
            {

                var index = FolderSystem.FolderSystem.ReverseIndex(_currentFolderText);
                if (index != customChip.FolderIndex)
                {
                    Manager.Instance.ChangeFolderToChip(customChip.name, index);
                    ReloadChipBar();
                }
            }
            _currentChip = null;
        }

        public void ViewChip()
        {
            if (_currentChip != null)
            {
                Manager.Instance.ViewChip(_currentChip);
                _currentChip = null;
            }
        }

        public void ExportChip() { ImportExport.Instance.ExportChip(_currentChip); }
    }
}