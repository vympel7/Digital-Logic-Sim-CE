using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

namespace Assets.Scripts.SaveSystem
{
    using Scripts.Chip;
    using Scripts.Core;
    using Scripts.Graphics;
    using Scripts.SaveSystem.Serializable;

    public static class ChipLoader
    {
        public static SavedChip[] GetAllSavedChips(string[] chipPaths)
        {
            var savedChips = new SavedChip[chipPaths.Length];

            // Read saved chips from file
            for (var i = 0; i < chipPaths.Length; i++)
            {
                var chipSaveString = SaveSystem.ReadFile(chipPaths[i]);
                SaveCompatibility.FixSaveCompatibility(ref chipSaveString);
                savedChips[i] = JsonUtility.FromJson<SavedChip>(chipSaveString);
            }

            foreach (var chip in savedChips)
                chip.ValidateDefaultData();

            return savedChips;
        }

        public static Dictionary<string, SavedChip> GetAllSavedChipsDic(string[] chipPaths)
        {
            return GetAllSavedChips(chipPaths).ToDictionary(chip => chip.Data.Name);
        }

        public static async void LoadAllChips(string[] chipPaths, Manager manager)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var chipsToLoadDic = GetAllSavedChipsDic(chipPaths);

            var progressBar = UI.ProgressBar.New("Loading All Chips...", wholeNumbers: true);
            progressBar.Open(0, chipsToLoadDic.Count + manager.BuiltinChips.Length);
            progressBar.SetValue(0, "Start Loading...");

            // Maintain dictionary of loaded chips (initially just the built-in chips)
            var loadedChips = new Dictionary<string, Chip>();
            var i = 0;
            for (; i < manager.BuiltinChips.Length; i++)
            {
                var builtinChip = manager.BuiltinChips[i];
                progressBar.SetValue(i, $"Loading '{builtinChip.ChipName}'...");
                loadedChips.Add(builtinChip.ChipName, builtinChip);
                await Task.Yield();
            }

            foreach (var chip in chipsToLoadDic)
            {
                progressBar.SetValue(i, $"Loading '{chip.Value.Data.Name}'...");
                if (!loadedChips.ContainsKey(chip.Key))
                {
                    try
                    {
                        ResolveDependecy(chip.Value);
                    }
                    catch (Exception e)
                    {
                        UI.DLSLogger.LogWarning($"Custom Chip '{chip.Value.Data.Name}' could not be loaded!", e.ToString());
                    }
                }

                await Task.Yield();
            }

            progressBar.SetValue(progressBar.ProgBar.maxValue, "Done!");
            progressBar.Close();
            UI.DLSLogger.Log($"Load time: {sw.ElapsedMilliseconds}ms");

            // the simulation will never create Cyclic path so simple ricorsive descending graph explore shuld be fine
            async void ResolveDependecy(SavedChip chip)
            {
                foreach (var dependancy in chip.ChipDependecies)
                {
                    if (string.Equals(dependancy, "SIGNAL IN") || string.Equals(dependancy, "SIGNAL OUT")) continue;
                    if (!loadedChips.ContainsKey(dependancy))
                    { ResolveDependecy(chipsToLoadDic[dependancy]); await Task.Yield(); i++; }
                }
                if (!loadedChips.ContainsKey(chip.Data.Name))
                {
                    Chip loadedChip = manager.LoadChip(LoadChip(chip, loadedChips, manager.WirePrefab));
                    loadedChips.Add(loadedChip.ChipName, loadedChip);
                }
            }

        }

        // Instantiates all components that make up the given chip, and connects them
        // up with wires The components are parented under a single "holder" object,
        // which is returned from the function
        static ChipSaveData LoadChip(SavedChip chipToLoad, Dictionary<string, Chip> previouslyLoadedChips, Wire WirePrefab)
        {

            bool WouldLoad(out List<string> ComponentsMissing)
            {
                ComponentsMissing = new List<string>();
                foreach (var dependency in chipToLoad.ChipDependecies)
                {
                    if (string.Equals(dependency, "SIGNAL IN") || string.Equals(dependency, "SIGNAL OUT")) continue;
                    if (!previouslyLoadedChips.ContainsKey(dependency))
                        ComponentsMissing.Add(dependency);
                }
                return ComponentsMissing.Count <= 0;
            }


            if (!WouldLoad(out List<string> miss))
            {
                string MissingComp = "";
                for (int i = 0; i < miss.Count; i++)
                {
                    MissingComp += miss[i];
                    if (i < miss.Count - 1)
                        MissingComp += ",";
                }
                UI.DLSLogger.LogError($"Failed to load {chipToLoad.Data.Name} sub component: {MissingComp} was missing");

                return null;
            }

            ChipSaveData loadedChipData = new ChipSaveData();
            int numComponents = chipToLoad.SavedComponentChips.Length;
            loadedChipData.ComponentChips = new Chip[numComponents];
            loadedChipData.Data = chipToLoad.Data;


            // Spawn component chips (the chips used to create this chip)
            // These will have been loaded already, and stored in the
            // previouslyLoadedChips dictionary
            for (int i = 0; i < numComponents; i++)
            {
                SavedComponentChip componentToLoad = chipToLoad.SavedComponentChips[i];
                string componentName = componentToLoad.ChipName;
                Vector2 pos = new Vector2(componentToLoad.PosX, componentToLoad.PosY);


                Chip loadedComponentChip = GameObject.Instantiate(
                    previouslyLoadedChips[componentName], pos, Quaternion.identity);
                loadedChipData.ComponentChips[i] = loadedComponentChip;

                // Load input pin names
                for (int inputIndex = 0;
                    inputIndex < componentToLoad.InputPins.Length &&
                    inputIndex < loadedChipData.ComponentChips[i].InputPins.Length;
                    inputIndex++)
                {
                    loadedChipData.ComponentChips[i].InputPins[inputIndex].PinName =
                        componentToLoad.InputPins[inputIndex].Name;
                    loadedChipData.ComponentChips[i].InputPins[inputIndex].WType =
                        componentToLoad.InputPins[inputIndex].WireType;
                }

                // Load output pin names
                for (int ouputIndex = 0; ouputIndex < componentToLoad.OutputPins.Length;
                    ouputIndex++)
                {
                    loadedChipData.ComponentChips[i].OutputPins[ouputIndex].PinName =
                        componentToLoad.OutputPins[ouputIndex].Name;
                    loadedChipData.ComponentChips[i].OutputPins[ouputIndex].WType =
                        componentToLoad.OutputPins[ouputIndex].WireType;
                }
            }

            // Connect pins with wires
            for (int chipIndex = 0; chipIndex < chipToLoad.SavedComponentChips.Length;
                chipIndex++)
            {
                Chip loadedComponentChip = loadedChipData.ComponentChips[chipIndex];
                for (int inputPinIndex = 0;
                    inputPinIndex < loadedComponentChip.InputPins.Length &&
                    inputPinIndex <
                        chipToLoad.SavedComponentChips[chipIndex].InputPins.Length;
                    inputPinIndex++)
                {
                    SavedInputPin savedPin =
                        chipToLoad.SavedComponentChips[chipIndex].InputPins[inputPinIndex];
                    Pin pin = loadedComponentChip.InputPins[inputPinIndex];

                    // If this pin should receive input from somewhere, then wire it up to
                    // that pin
                    if (savedPin.ParentChipIndex != -1)
                    {
                        Pin connectedPin =
                            loadedChipData.ComponentChips[savedPin.ParentChipIndex]
                                .OutputPins[savedPin.ParentChipOutputIndex];
                        pin.Cyclic = savedPin.IsCylic;
                        Pin.TryConnect(connectedPin, pin);
                    }
                }
            }

            return loadedChipData;
        }

        static ChipSaveData LoadChipWithWires(SavedChip chipToLoad, Wire WirePrefab, ChipEditor chipEditor)
        {
            var previouslyLoadedChips = Manager.Instance.AllSpawnableChipDic();
            ChipSaveData loadedChipData = new ChipSaveData();
            int numComponents = chipToLoad.SavedComponentChips.Length;
            loadedChipData.ComponentChips = new Chip[numComponents];
            loadedChipData.Data = chipToLoad.Data;
            List<Wire> wiresToLoad = new List<Wire>();

            // Spawn component chips (the chips used to create this chip)
            // These will have been loaded already, and stored in the
            // previouslyLoadedChips dictionary
            for (int i = 0; i < numComponents; i++)
            {
                SavedComponentChip componentToLoad = chipToLoad.SavedComponentChips[i];
                string componentName = componentToLoad.ChipName;
                Vector2 pos = new Vector2(componentToLoad.PosX, componentToLoad.PosY);

                if (!previouslyLoadedChips.ContainsKey(componentName))
                    UI.DLSLogger.LogError($"Failed to load sub component: {componentName} While loading {chipToLoad.Data.Name}");

                Chip loadedComponentChip = GameObject.Instantiate(previouslyLoadedChips[componentName], pos, Quaternion.identity, chipEditor.ChipImplementationHolder);

                loadedComponentChip.gameObject.SetActive(true);
                loadedChipData.ComponentChips[i] = loadedComponentChip;

                // Load input pin names
                for (int inputIndex = 0;
                    inputIndex < componentToLoad.InputPins.Length &&
                    inputIndex < loadedChipData.ComponentChips[i].InputPins.Length;
                    inputIndex++)
                {
                    loadedChipData.ComponentChips[i].InputPins[inputIndex].PinName =
                        componentToLoad.InputPins[inputIndex].Name;
                }

                // Load output pin names
                for (int ouputIndex = 0;
                    ouputIndex < componentToLoad.OutputPins.Length &&
                    ouputIndex < loadedChipData.ComponentChips[i].OutputPins.Length;
                    ouputIndex++)
                {
                    loadedChipData.ComponentChips[i].OutputPins[ouputIndex].PinName =
                        componentToLoad.OutputPins[ouputIndex].Name;
                }
            }

            // Connect pins with wires
            for (int chipIndex = 0; chipIndex < chipToLoad.SavedComponentChips.Length;
                chipIndex++)
            {
                Chip loadedComponentChip = loadedChipData.ComponentChips[chipIndex];
                for (int inputPinIndex = 0;
                    inputPinIndex < loadedComponentChip.InputPins.Length &&
                    inputPinIndex < chipToLoad.SavedComponentChips[chipIndex].InputPins.Length;
                    inputPinIndex++)
                {
                    SavedInputPin savedPin =
                        chipToLoad.SavedComponentChips[chipIndex].InputPins[inputPinIndex];
                    Pin pin = loadedComponentChip.InputPins[inputPinIndex];

                    // If this pin should receive input from somewhere, then wire it up to
                    // that pin
                    if (savedPin.ParentChipIndex != -1)
                    {
                        Pin connectedPin =
                            loadedChipData.ComponentChips[savedPin.ParentChipIndex]
                                .OutputPins[savedPin.ParentChipOutputIndex];
                        pin.Cyclic = savedPin.IsCylic;
                        if (Pin.TryConnect(connectedPin, pin))
                        {
                            Wire loadedWire = GameObject.Instantiate(WirePrefab, chipEditor.WireHolder);
                            loadedWire.Connect(connectedPin, pin);
                            wiresToLoad.Add(loadedWire);
                        }
                    }
                }
            }

            loadedChipData.Wires = wiresToLoad.ToArray();

            return loadedChipData;
        }

        public static ChipSaveData GetChipSaveData(Chip chip, Wire WirePrefab, ChipEditor chipEditor)
        {
            // @NOTE: chipEditor can be removed here if:
            //     * Chip & wire instatiation is inside their respective implementation
            //     holders is inside the chipEditor
            //     * the wire connections are done inside ChipEditor.LoadFromSaveData
            //     instead of ChipLoader.LoadChipWithWires

            SavedChip chipToTryLoad = SaveSystem.ReadChip(chip.ChipName);

            if (chipToTryLoad == null)
                return null;

            ChipSaveData loadedChipData = LoadChipWithWires(chipToTryLoad, WirePrefab, chipEditor);
            SavedWireLayout wireLayout = SaveSystem.ReadWire(loadedChipData.Data.Name);

            //Work Around solution. it just Work but maybe is worth to change the entire way to save WireLayout (idk i don't think so)
            for (int i = 0; i < loadedChipData.Wires.Length; i++)
            {
                Wire wire = loadedChipData.Wires[i];
                wire.EndPin.PinName = wire.EndPin.PinName + i;
            }

            // Set wires anchor points
            foreach (SavedWire wire in wireLayout.SerializableWires)
            {
                string startPinName;
                string endPinName;

                // This fixes a bug which caused chips to be unable to be viewed/edited if
                // some of input/output pins were swaped.
                try
                {
                    startPinName = loadedChipData.ComponentChips[wire.ParentChipIndex]
                                    .OutputPins[wire.ParentChipOutputIndex]
                                    .PinName;
                    endPinName = loadedChipData.ComponentChips[wire.ChildChipIndex]
                                    .InputPins[wire.ChildChipInputIndex]
                                    .PinName;
                }
                catch (IndexOutOfRangeException)
                {
                    // Swap input pins with output pins.
                    startPinName = loadedChipData.ComponentChips[wire.ParentChipIndex]
                                    .InputPins[wire.ParentChipOutputIndex]
                                    .PinName;
                    endPinName = loadedChipData.ComponentChips[wire.ChildChipIndex]
                                    .OutputPins[wire.ChildChipInputIndex]
                                    .PinName;
                }
                int wireIndex = Array.FindIndex(loadedChipData.Wires, w => w.StartPin.PinName == startPinName && w.EndPin.PinName == endPinName);
                if (wireIndex >= 0)
                    loadedChipData.Wires[wireIndex].SetAnchorPoints(wire.AnchorPoints);
            }

            for (int i = 0; i < loadedChipData.Wires.Length; i++)
            {
                Wire wire = loadedChipData.Wires[i];
                wire.EndPin.PinName = wire.EndPin.PinName.Remove(wire.EndPin.PinName.Length - 1);
            }

            return loadedChipData;
        }

        public static void Import(string path)
        {
            var allChips = SaveSystem.GetAllSavedChips();
            var nameUpdateLookupTable = new Dictionary<string, string>();

            using var reader = new StreamReader(path);
            var numberOfChips = Int32.Parse(reader.ReadLine());

            for (var i = 0; i < numberOfChips; i++)
            {
                string ChipName = reader.ReadLine();
                int saveDataLength = Int32.Parse(reader.ReadLine());
                int wireSaveDataLength = Int32.Parse(reader.ReadLine());

                string saveData = "";
                string wireSaveData = "";

                for (int j = 0; j < saveDataLength; j++)
                {
                    saveData += reader.ReadLine() + "\n";
                }
                for (int j = 0; j < wireSaveDataLength; j++)
                {
                    wireSaveData += reader.ReadLine() + "\n";
                }

                // Rename chip if already exist
                if (Array.FindIndex(allChips, c => c.Data.Name == ChipName) >= 0)
                {
                    int nameCounter = 2;
                    string newName;
                    do
                    {
                        newName = ChipName + nameCounter.ToString();
                        nameCounter++;
                    } while (Array.FindIndex(allChips, c => c.Data.Name == newName) >= 0);

                    nameUpdateLookupTable.Add(ChipName, newName); ChipName = newName;
                }

                // Update Name inside file if there was some names changed
                foreach (KeyValuePair<string, string> nameToReplace in nameUpdateLookupTable)
                {
                    saveData = saveData
                        .Replace("\"Name\": \"" + nameToReplace.Key + "\"",
                            "\"Name\": \"" + nameToReplace.Value + "\"")
                        .Replace("\"ChipName\": \"" + nameToReplace.Key + "\"",
                            "\"ChipName\": \"" + nameToReplace.Value + "\"");
                }

                string chipSaveFile = SaveSystem.GetPathToSaveFile(ChipName);

                SaveSystem.WriteChip(ChipName, saveData);
                SaveSystem.WriteWire(ChipName, wireSaveData);
            }
        }
    }
}