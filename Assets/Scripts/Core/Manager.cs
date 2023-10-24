using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Core
{
    using Scripts.Chip;
    using Scripts.Graphics;
    using Scripts.SaveSystem;
    using Scripts.UI;

    public enum ChipEditorMode { Create, Update };

    public class Manager : MonoBehaviour
    {
        public static ChipEditorMode ChipEditorMode;

        public event Action<Chip> CustomChipCreated;
        public event Action<Chip> CustomChipUpdated;

        public ChipEditor ChipEditorPrefab;
        public ChipPackage ChipPackagePrefab;
        public Wire WirePrefab;
        public Chip[] BuiltinChips;
        public List<Chip> SpawnableCustomChips;
        public UIManager UIManager;

        private ChipEditor _activeChipEditor;
        private int _currentChipCreationIndex;
        public static Manager Instance;

        private void Awake()
        {
            Instance = this;
            SaveSystem.Init();
            FolderSystem.FolderSystem.Init();
        }

        private void Start()
        {
            SpawnableCustomChips = new List<Chip>();
            _activeChipEditor = FindObjectOfType<ChipEditor>();
            SaveSystem.LoadAllChips(this);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                Pin[] unconnectedInputs =
                    _activeChipEditor.ChipInteraction.UnconnectedInputPins;
                Pin[] unconnectedOutputs =
                    _activeChipEditor.ChipInteraction.UnconnectedOutputPins;
                if (unconnectedInputs.Length > 0)
                {
                    Debug.Log("Found " + unconnectedInputs.Length.ToString() +
                            " unconnected input pins!");
                }
                if (unconnectedOutputs.Length > 0)
                {
                    Debug.Log("Found " + unconnectedOutputs.Length.ToString() +
                            " unconnected output pins!");
                }
            }
        }

        public static ChipEditor ActiveChipEditor => Instance._activeChipEditor;

        public Chip GetChipPrefab(Chip chip)
        {
            foreach (Chip prefab in BuiltinChips)
            {
                if (chip.ChipName == prefab.ChipName)
                {
                    return prefab;
                }
            }
            foreach (Chip prefab in SpawnableCustomChips)
            {
                if (chip.ChipName == prefab.ChipName)
                {
                    return prefab;
                }
            }
            return null;
        }

        public static Chip GetChipByName(string name)
        {
            foreach (Chip chip in Instance.SpawnableCustomChips)
            {
                if (name == chip.ChipName)
                {
                    return chip;
                }
            }
            return null;
        }

        public Chip LoadChip(ChipSaveData loadedChipData)
        {
            if (loadedChipData == null) return null;
            _activeChipEditor.LoadFromSaveData(loadedChipData);
            _currentChipCreationIndex = _activeChipEditor.Data.CreationIndex;

            Chip loadedChip = PackageChip();
            if (loadedChip is CustomChip custom)
                custom.ApplyWireModes();

            ClearEditor();
            return loadedChip;
        }

        public void ViewChip(Chip chip)
        {
            ChipSaveData chipSaveData = ChipLoader.GetChipSaveData(chip, WirePrefab, _activeChipEditor);
            ClearEditor();
            ChipEditorMode = ChipEditorMode.Update;
            UIManager.SetEditorMode(ChipEditorMode, chipSaveData.Data.Name);
            _activeChipEditor.LoadFromSaveData(chipSaveData);
        }

        public void SaveAndPackageChip()
        {
            ChipSaver.Save(_activeChipEditor);
            PackageChip();
            ClearEditor();
        }

        public void UpdateChip()
        {
            Chip updatedChip = TryPackageAndReplaceChip(_activeChipEditor.Data.Name);
            ChipSaver.Update(_activeChipEditor, updatedChip);
            ChipEditorMode = ChipEditorMode.Create;
            ClearEditor();
        }

        internal void DeleteChip(string nameBeforeChanging)
        {
            SpawnableCustomChips = SpawnableCustomChips.Where(x => !string.Equals(x.ChipName, nameBeforeChanging)).ToList();
        }

        internal void RenameChip(string nameBeforeChanging, string nameAfterChanging)
        {
            SpawnableCustomChips.Where(x => string.Equals(x.ChipName, nameBeforeChanging)).First().ChipName = nameAfterChanging;
        }

        private void SetupPseudoInput(Chip customChip)
        {
            // TODO: Implement this
            //  if (customChip is CustomChip custom) {
            //  	custom.unconnectedInputs =
            //  activeChipEditor.ChipInteraction.UnconnectedInputPins; 	Pin pseudoPin =
            //  Instantiate(chipPackagePrefab.chipPinPrefab.gameObject, parent:
            //  customChip.transform).GetComponent<Pin>(); 	pseudoPin.pinName =
            //  "PseudoInput"; 	pseudoPin.wireType = Pin.WireType.Simple;
            //  	custom.pseudoInput = pseudoPin;
            //  	pseudoPin.chip = customChip;
            //  	foreach (Pin pin in custom.unconnectedInputs) {
            //  		Pin.MakeConnection(pseudoPin, pin);
            //  	}
            //  }
        }

        private Chip PackageChip()
        {
            Chip customChip = GeneratePackageAndChip();

            CustomChipCreated?.Invoke(customChip);
            _currentChipCreationIndex++;
            SpawnableCustomChips.Add(customChip);
            return customChip;
        }

        private Chip TryPackageAndReplaceChip(string original)
        {
            ChipPackage oldPackage = Array.Find(
                GetComponentsInChildren<ChipPackage>(true), cp => cp.name == original);
            if (oldPackage != null) { Destroy(oldPackage.gameObject); }

            Chip customChip = GeneratePackageAndChip();

            int index = SpawnableCustomChips.FindIndex(c => c.ChipName == original);
            if (index >= 0)
            {
                SpawnableCustomChips[index] = customChip;
                CustomChipUpdated?.Invoke(customChip);
            }
            return customChip;
        }

        private Chip GeneratePackageAndChip()
        {
            ChipPackage package = Instantiate(ChipPackagePrefab, transform);

            package.PackageCustomChip(_activeChipEditor);
            package.gameObject.SetActive(false);

            var customChip = package.GetComponent<Chip>();
            SetupPseudoInput(customChip);
            if (customChip is CustomChip c)
                c.Init();

            return customChip;
        }

        public void ResetEditor()
        {
            ChipEditorMode = ChipEditorMode.Create;
            UIManager.SetEditorMode(ChipEditorMode);
            ClearEditor();
        }

        private void ClearEditor()
        {
            if (_activeChipEditor)
            {
                Destroy(_activeChipEditor.gameObject);
                UIManager.SetEditorMode(ChipEditorMode, UIManager.ChipName.text);
            }
            _activeChipEditor =
                Instantiate(ChipEditorPrefab, Vector3.zero, Quaternion.identity);

            _activeChipEditor.InputsEditor.CurrentEditor = _activeChipEditor;
            _activeChipEditor.OutputsEditor.CurrentEditor = _activeChipEditor;

            _activeChipEditor.Data.CreationIndex = _currentChipCreationIndex;

            Simulation.Instance.ResetSimulation();
            ScalingManager.Scale = 1;
            ChipEditorOptions.Instance.SetUIValues(_activeChipEditor);
        }

        public void ChipButtonHanderl(Chip chip)
        {
            if (chip is CustomChip custom)
                custom.ApplyWireModes();

            _activeChipEditor.ChipInteraction.ChipButtonInteraction(chip);
        }

        public void LoadMainMenu()
        {
            if (ChipEditorMode == ChipEditorMode.Update)
            {
                ChipEditorMode = ChipEditorMode.Create;
                ClearEditor();
            }
            else
            {
                FolderSystem.FolderSystem.Reset();
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }

        public List<string> AllChipNames(bool builtin = true, bool custom = true)
        {
            List<string> allChipNames = new List<string>();
            if (builtin)
                foreach (Chip chip in BuiltinChips)
                    allChipNames.Add(chip.ChipName);
            if (custom)
                foreach (Chip chip in SpawnableCustomChips)
                    allChipNames.Add(chip.ChipName);

            return allChipNames;
        }

        public Dictionary<string, Chip> AllSpawnableChipDic()
        {
            Dictionary<string, Chip> allChipDic = new Dictionary<string, Chip>();

            foreach (Chip chip in BuiltinChips)
                allChipDic.Add(chip.ChipName, chip);
            foreach (Chip chip in SpawnableCustomChips)
                allChipDic.Add(chip.ChipName, chip);
            return allChipDic;
        }

        public void ChangeFolderToChip(string ChipName, int index)
        {
            if (SpawnableCustomChips.Where(x => string.Equals(x.name, ChipName)).First() is CustomChip customChip)
                customChip.FolderIndex = index;
            ChipSaver.ChangeFolder(ChipName, index);
        }
    }
}