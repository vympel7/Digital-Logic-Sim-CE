using UnityEngine;

namespace Assets.Scripts.SaveSystem.Serializable
{
    [System.Serializable]
    public struct ChipData
    {
        public string Name;
        public int CreationIndex;
        public Color Colour;
        public Color NameColour;
        public int FolderIndex;
        public float Scale;

        public void ValidateDefaultData()
        {
            if (float.IsNaN(FolderIndex))
                FolderIndex = 0;
            if (float.IsNaN(Scale))
                Scale = 1f;
        }
    }
}