using UnityEngine;
using SFB;

namespace Assets.Scripts.SaveSystem
{
    using Scripts.UI;

    public class ImportExport : MonoBehaviour
    {
        public static ImportExport Instance;
        private ChipBarUI _chipBar;

        private void Awake() { Instance = this; }

        private void Start() { _chipBar = FindObjectOfType<ChipBarUI>(); }

        public void ExportChip(Chip.Chip chip)
        {
            string path = StandaloneFileBrowser.SaveFilePanel(
                "Export chip design", "", chip.ChipName + ".dls", "dls");
            if (path.Length != 0)
                ChipSaver.Export(chip, path);
        }

        public void ImportChip()
        {
            var extensions = new[] {
        new ExtensionFilter("Chip design", "dls"),
        };

            StandaloneFileBrowser.OpenFilePanelAsync(
                "Import chip design", "", extensions, true, (string[] paths) =>
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
            _chipBar.ReloadChipButton();
            SaveSystem.LoadAllChips(Core.Manager.Instance);
        }
    }
}