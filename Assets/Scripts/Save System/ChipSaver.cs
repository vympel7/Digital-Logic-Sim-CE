using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System;

namespace Assets.Scripts.SaveSystem
{
    using Scripts.Chip;
    using Scripts.Core;
    using Scripts.Graphics;
    using Serializable;

    public static class ChipSaver
    {
        private const bool _usePrettyPrint = true;

        public static void Save(ChipEditor chipEditor)
        {
            ChipSaveData chipSaveData = new ChipSaveData(chipEditor);

            // Generate new chip save string
            var compositeChip = new SavedChip(chipSaveData);
            string saveString = JsonUtility.ToJson(compositeChip, _usePrettyPrint);

            // Generate save string for wire layout
            var wiringSystem = new SavedWireLayout(chipSaveData);
            string wiringSaveString = JsonUtility.ToJson(wiringSystem, _usePrettyPrint);

            // Write to file
            string savePath = SaveSystem.GetPathToSaveFile(chipEditor.Data.Name);
            SaveSystem.WriteFile(savePath, saveString);

            string wireLayoutSavePath =
                SaveSystem.GetPathToWireSaveFile(chipEditor.Data.Name);
            SaveSystem.WriteFile(wireLayoutSavePath, wiringSaveString);
        }

        public static void Export(Chip exportedChip, string destinationPath)
        {
            Dictionary<int, string> chipsToExport =
                FindChildrenChips(exportedChip.ChipName);

            using (StreamWriter writer = new StreamWriter(destinationPath))
            {
                writer.WriteLine(chipsToExport.Count);

                foreach (KeyValuePair<int, string> chip in chipsToExport.OrderBy(x => x.Key))
                {
                    string chipSaveFile = SaveSystem.GetPathToSaveFile(chip.Value);
                    string chipWireSaveFile = SaveSystem.GetPathToWireSaveFile(chip.Value);

                    using (StreamReader reader = new StreamReader(chipSaveFile))
                    {
                        string saveString = reader.ReadToEnd();

                        using (StreamReader wireReader = new StreamReader(chipWireSaveFile))
                        {
                            string wiringSaveString = wireReader.ReadToEnd();

                            writer.WriteLine(chip.Value);
                            writer.WriteLine(saveString.Split('\n').Length);
                            writer.WriteLine(wiringSaveString.Split('\n').Length);
                            writer.WriteLine(saveString);
                            writer.WriteLine(wiringSaveString);
                        }
                    }
                }
            }
        }

        static Dictionary<int, string> FindChildrenChips(string chipName)
        {
            Dictionary<int, string> childrenChips = new Dictionary<int, string>();

            Manager manager = GameObject.FindObjectOfType<Manager>();
            SavedChip[] allChips = SaveSystem.GetAllSavedChips();
            SavedChip currentChip = Array.Find(allChips, c => c.Data.Name == chipName);
            if (currentChip == null) return childrenChips;

            childrenChips.Add(currentChip.Data.CreationIndex, chipName);

            foreach (SavedComponentChip scc in currentChip.SavedComponentChips)
            {
                if (Array.FindIndex(manager.BuiltinChips,
                                    c => c.ChipName == scc.ChipName) != -1) continue;

                foreach (var chip in FindChildrenChips(scc.ChipName))
                {
                    if (childrenChips.ContainsKey(chip.Key)) continue;
                    childrenChips.Add(chip.Key, chip.Value);
                }
            }

            return childrenChips;
        }

        public static void Update(ChipEditor chipEditor, Chip chip)
        {
            ChipSaveData chipSaveData = new ChipSaveData(chipEditor);

            // Generate new chip save string
            string saveString = JsonUtility.ToJson(new SavedChip(chipSaveData), _usePrettyPrint);

            // Generate save string for wire layout
            string wiringSaveString = JsonUtility.ToJson(new SavedWireLayout(chipSaveData), _usePrettyPrint);

            // Write to file
            SaveSystem.WriteChip(chipEditor.Data.Name, saveString);
            SaveSystem.WriteWire(chipEditor.Data.Name, wiringSaveString);


            // Update parent chips using this chip
            string currentChipName = chipEditor.Data.Name;
            SavedChip[] savedChips = SaveSystem.GetAllSavedChips();
            for (int i = 0; i < savedChips.Length; i++)
            {
                if (savedChips[i].ChipDependecies.Contains(currentChipName))
                {
                    int currentChipIndex =
                        Array.FindIndex(savedChips[i].SavedComponentChips,
                                        c => c.ChipName == currentChipName);
                    SavedComponentChip updatedComponentChip = new SavedComponentChip(chipSaveData, chip);
                    SavedComponentChip oldComponentChip =
                        savedChips[i].SavedComponentChips[currentChipIndex];

                    // Update component chip I/O
                    for (int j = 0; j < updatedComponentChip.InputPins.Length; j++)
                    {
                        for (int k = 0; k < oldComponentChip.InputPins.Length; k++)
                        {
                            if (updatedComponentChip.InputPins[j].Name ==
                                oldComponentChip.InputPins[k].Name)
                            {
                                updatedComponentChip.InputPins[j].ParentChipIndex =
                                    oldComponentChip.InputPins[k].ParentChipIndex;

                                updatedComponentChip.InputPins[j].ParentChipOutputIndex =
                                    oldComponentChip.InputPins[k].ParentChipOutputIndex;

                                updatedComponentChip.InputPins[j].IsCylic =
                                    oldComponentChip.InputPins[k].IsCylic;
                            }
                        }
                    }

                    // Write to file
                    SaveSystem.WriteChip(savedChips[i].Data.Name, JsonUtility.ToJson(savedChips[i], _usePrettyPrint));
                }
            }
        }

        internal static void ChangeFolder(string chipname, int folderIndex)
        {
            var ChipToEdit = SaveSystem.GetAllSavedChipsDic()[chipname];
            if (ChipToEdit.Data.FolderIndex == folderIndex) return;
            ChipToEdit.Data.FolderIndex = folderIndex;
            SaveSystem.WriteChip(chipname, JsonUtility.ToJson(ChipToEdit, _usePrettyPrint));
        }

        public static void EditSavedChip(SavedChip savedChip, ChipSaveData chipSaveData)
        { }

        public static bool IsSafeToDelete(string chipName)
        {
            if (Manager.Instance.AllChipNames(true, false).Contains(chipName))
                return false;

            SavedChip[] savedChips = SaveSystem.GetAllSavedChips();
            foreach (SavedChip savedChip in savedChips)
                if (savedChip.ChipDependecies.Contains(chipName))
                    return false;
            return true;
        }

        public static bool IsSignalSafeToDelete(string chipName, string signalName)
        {
            SavedChip[] savedChips = SaveSystem.GetAllSavedChips();
            for (int i = 0; i < savedChips.Length; i++)
            {
                if (savedChips[i].ChipDependecies.Contains(chipName))
                {
                    SavedChip parentChip = savedChips[i];
                    int currentChipIndex = Array.FindIndex(parentChip.SavedComponentChips,
                                                        scc => scc.ChipName == chipName);
                    SavedComponentChip currentChip =
                        parentChip.SavedComponentChips[currentChipIndex];
                    int currentSignalIndex = Array.FindIndex(
                        currentChip.OutputPins, name => name.Name == signalName);

                    if (Array.Find(currentChip.InputPins,
                                pin => pin.Name == signalName && pin.ParentChipIndex >= 0) != null)
                    {
                        return false;
                    }
                    else if (currentSignalIndex >= 0 &&
                            parentChip.SavedComponentChips.Any(scc => scc.InputPins.Any(pin => pin.ParentChipIndex == currentChipIndex
                                && pin.ParentChipOutputIndex == currentSignalIndex)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static void Delete(string chipName)
        {
            File.Delete(SaveSystem.GetPathToSaveFile(chipName));
            File.Delete(SaveSystem.GetPathToWireSaveFile(chipName));
        }

        public static void Rename(string oldChipName, string newChipName)
        {
            if (oldChipName == newChipName)
            {
                return;
            }
            SavedChip[] savedChips = SaveSystem.GetAllSavedChips();
            for (int i = 0; i < savedChips.Length; i++)
            {
                bool changed = false;
                if (savedChips[i].Data.Name == oldChipName)
                {
                    savedChips[i].Data.Name = newChipName;
                    changed = true;
                }
                for (int j = 0; j < savedChips[i].ChipDependecies.Length; j++)
                {
                    string componentName = savedChips[i].ChipDependecies[j];
                    if (componentName == oldChipName)
                    {
                        savedChips[i].ChipDependecies[j] = newChipName;
                        changed = true;
                    }
                }
                for (int j = 0; j < savedChips[i].SavedComponentChips.Length; j++)
                {
                    string componentChipName =
                        savedChips[i].SavedComponentChips[j].ChipName;
                    if (componentChipName == oldChipName)
                    {
                        savedChips[i].SavedComponentChips[j].ChipName = newChipName;
                        changed = true;
                    }
                }
                if (changed)
                {
                    string saveString = JsonUtility.ToJson(savedChips[i], _usePrettyPrint);
                    // Write to file
                    SaveSystem.WriteChip(savedChips[i].Data.Name, saveString);
                }
            }
            // Rename wire layer file
            string oldWireSaveFile = SaveSystem.GetPathToWireSaveFile(oldChipName);
            string newWireSaveFile = SaveSystem.GetPathToWireSaveFile(newChipName);
            try
            {
                System.IO.File.Move(oldWireSaveFile, newWireSaveFile);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
            // Delete old chip save file
            File.Delete(SaveSystem.GetPathToSaveFile(oldChipName));
        }
    }
}