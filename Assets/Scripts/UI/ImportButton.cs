using UnityEngine;
using UnityEngine.UI;
using SFB;

namespace Assets.Scripts.UI
{
    using Scripts.SaveSystem;

    public class ImportButton : MonoBehaviour
    {
        public Button ImpButton;
        public Core.Manager Manager;
        public ChipBarUI ChipBarUI;

        private void Start()
        {
            ImpButton.onClick.AddListener(ImportChip);
        }

        private void ImportChip()
        {
            var extensions = new[] {
                new ExtensionFilter("Chip design", "dls"),
            };


            StandaloneFileBrowser.OpenFilePanelAsync("Import chip design", "", extensions, true, (string[] paths) =>
            {
                if (paths[0] != null && paths[0] != "")
                {

                    ChipLoader.Import(paths[0]);
                    EditChipBar();
                }
            });
        }

        private void EditChipBar()
        {
            ChipBarUI.ReloadChipButton();
            SaveSystem.LoadAllChips(Manager);
        }
    }
}