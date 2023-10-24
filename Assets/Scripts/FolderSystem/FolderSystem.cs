using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.FolderSystem
{
    public enum DefaultKays { Comp = 0, Gate = 1, Misc = 2 }

    public class FolderSystem
    {
        private static bool _initialized = false;
        public static IEnumerable<KeyValuePair<int, string>> Enum => Folders.AsEnumerable();

        public static Dictionary<int, string> DefaultFolder
        {
            get => new Dictionary<int, string>()
            {
                { (int)DefaultKays.Comp , "Comp" },
                { (int)DefaultKays.Gate , "Gate" },
                { (int)DefaultKays.Misc , "Misc"}
            };
        }

        private static Dictionary<int, string> Folders;

        public static void Init()
        {
            Folders = new Dictionary<int, string>(DefaultFolder);

            foreach (var kv in SaveSystem.SaveSystem.LoadCustomFolders())
                Folders.TryAdd(kv.Key, kv.Value);

            _initialized = true;
        }

        public static void Reset()
        {
            Folders = null;
            _initialized = false;
        }

        public static int ReverseIndex(string DicValue) => _initialized ? Folders.FirstOrDefault(x => x.Value == DicValue).Key : -1;

        public static bool ContainsIndex(int i) => _initialized && Folders.ContainsKey(i);
        public static string GetFolderName(int i)
        {
            if (!ContainsIndex(i)) return "";
            return Folders[i];
        }

        public static int AddFolder(string newFolderName)
        {
            if (!_initialized) return -1;

            Folders[Folders.Count] = newFolderName;
            SaveSystem.SaveSystem.SaveCustomFolders(Folders);
            return Folders.Count - 1;
        }

        public static void DeleteFolder(string folderName)
        {
            if (!_initialized) return;

            DeleteFolder(ReverseIndex(folderName));
        }
        public static void DeleteFolder(int index)
        {
            if (!_initialized) return;
            Folders.Remove(index);
        }

        public static bool FolderNameAvailable(string name)
        {
            if (!_initialized) return false;

            foreach (string f in Folders.Values)
            {
                if (string.Equals(name.ToUpper(), f.ToUpper()))
                    return false;
            }
            return true;
        }


        public static bool CompareValue(int index, string value) => _initialized && ContainsIndex(index) && string.Equals(Folders[index], value);

        public static void RenameFolder(string oldFolderName, string newFolderName)
        {
            if (!_initialized) return;

            if (!Folders.ContainsValue(oldFolderName)
                || string.Equals(oldFolderName, newFolderName))
                return;

            var index = ReverseIndex(oldFolderName);
            Folders[index] = newFolderName;

            SaveSystem.SaveSystem.SaveCustomFolders(Folders);
        }
    }
}