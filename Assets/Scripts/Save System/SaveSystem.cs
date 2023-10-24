using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Assets.Scripts.SaveSystem
{
    using Serializable;

    public static class SaveSystem
    {
        private static string _activeProjectName = "Untitled";
        private const string _fileExtension = ".txt";
        private static string _customFoldersFileName = "CustomFolders";

        private static string _foldersFilePath => Path.Combine(_currentSaveProfileDirectoryPath, _customFoldersFileName + ".json");
        private static string _currentSaveProfileDirectoryPath => Path.Combine(SaveDataDirectoryPath, _activeProjectName);

        public static string SaveDataDirectoryPath => Path.Combine(Application.persistentDataPath, "SaveData");
        private static string _currentSaveProfileWireLayoutDirectoryPath => Path.Combine(_currentSaveProfileDirectoryPath, "WireLayout");
        private static string _HDDSaveFilePath => Path.Combine(_currentSaveProfileDirectoryPath, "HDDContents.json");
        public static string GetPathToSaveFile(string saveFileName) => Path.Combine(_currentSaveProfileDirectoryPath, saveFileName + _fileExtension);

        public static string GetPathToWireSaveFile(string saveFileName) => Path.Combine(_currentSaveProfileWireLayoutDirectoryPath, saveFileName + _fileExtension);


        public static void SetActiveProject(string projectName)
        {
            _activeProjectName = projectName;
        }

        public static void Init()
        {
            // Create save directory (if doesn't exist already)
            Directory.CreateDirectory(_currentSaveProfileDirectoryPath);
            Directory.CreateDirectory(_currentSaveProfileWireLayoutDirectoryPath);
            FolderLoader.CreateDefault(_foldersFilePath);
        }

        public static string[] GetChipSavePaths()
        {
            DirectoryInfo directory =
                new DirectoryInfo(_currentSaveProfileDirectoryPath);
            FileInfo[] files = directory.GetFiles("*" + _fileExtension);
            var filtered =
                files.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));
            List<string> result = new List<string>(); foreach (var f in filtered)
            {
                result.Add(f.ToString());
            }
            return result.ToArray();
            // return Directory.GetFiles(CurrentSaveProfileDirectoryPath, "*" +
            // fileExtension);
        }

        public static void LoadAllChips(Core.Manager manager)
        {
            // Load any saved chips
            ChipLoader.LoadAllChips(GetChipSavePaths(), manager);
        }

        public static SavedChip[] GetAllSavedChips()
        {
            // Load any saved chips
            return ChipLoader.GetAllSavedChips(GetChipSavePaths());
        }

        public static IDictionary<string, SavedChip> GetAllSavedChipsDic()
        {
            // Load any saved chips but is Dic
            return ChipLoader.GetAllSavedChipsDic(GetChipSavePaths());
        }

        public static string[] GetSaveNames()
        {
            string[] savedProjectPaths = new string[0];
            if (Directory.Exists(SaveDataDirectoryPath))
            {
                savedProjectPaths = Directory.GetDirectories(SaveDataDirectoryPath);
            }
            for (int i = 0; i < savedProjectPaths.Length; i++)
            {
                string[] pathSections =
                    savedProjectPaths[i].Split(Path.DirectorySeparatorChar);
                savedProjectPaths[i] = pathSections[pathSections.Length - 1];
            }
            return savedProjectPaths;
        }

        public static Dictionary<string, List<int>> LoadHDDContents()
        {
            if (File.Exists(_HDDSaveFilePath))
            {
                string jsonString = ReadFile(_HDDSaveFilePath);
                return JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(
                    jsonString);
            }
            return new Dictionary<string, List<int>> { };
        }

        public static void SaveHDDContents(Dictionary<string, List<int>> contents)
        {
            string jsonStr = JsonConvert.SerializeObject(contents, Formatting.Indented);
            WriteFile(_HDDSaveFilePath, jsonStr);
        }

        public static Dictionary<int, string> LoadCustomFolders()
        {
            return FolderLoader.LoadCustomFolders(_foldersFilePath);
        }

        public static void SaveCustomFolders(Dictionary<int, string> folders)
        {
            FolderLoader.SaveCustomFolders(_foldersFilePath, folders);
        }

        public static string ReadFile(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                return reader.ReadToEnd();
            }
        }

        public static void WriteFile(string path, string content)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(content);
            }
        }

        public static SavedChip ReadChip(string chipName) => JsonUtility.FromJson<SavedChip>(ReadFile(GetPathToSaveFile(chipName)));
        public static SavedWireLayout ReadWire(string wireFile) => JsonUtility.FromJson<SavedWireLayout>(ReadFile(GetPathToWireSaveFile(wireFile)));

        public static void WriteChip(string chipName, string saveString) => WriteFile(GetPathToSaveFile(chipName), saveString);
        public static void WriteWire(string chipName, string saveContent) => WriteFile(GetPathToWireSaveFile(chipName), saveContent);
        public static void WriteFoldersFile(string folderFileStr) => WriteFile(_foldersFilePath, folderFileStr);
    }
}