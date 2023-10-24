using System.Linq;


namespace Assets.Scripts.SaveSystem.Serializable
{
    [System.Serializable]
    // Composite chip is a custom chip made up from other chips ("components")
    public class SavedChip
    {
        public ChipData Data;

        // Names of all chips used as components in this new chip (each name appears
        // only once)
        public string[] ChipDependecies;
        // Data about all the chips used as components in this chip (positions,
        // connections, etc) Array is ordered: first come input signals, then output
        // signals, then remaining component chips
        public SavedComponentChip[] SavedComponentChips;

        public SavedChip(ChipSaveData chipSaveData)
        {
            Data = chipSaveData.Data;

            // Create list of (unique) names of all chips used to make this chip
            ChipDependecies = chipSaveData.ComponentChips.Select(x => x.ChipName)
                                    .Distinct()
                                    .ToArray();

            // Create serializable chips
            SavedComponentChips = new SavedComponentChip[chipSaveData.ComponentChips.Length];

            for (int i = 0; i < chipSaveData.ComponentChips.Length; i++)
                SavedComponentChips[i] = new SavedComponentChip(chipSaveData, chipSaveData.ComponentChips[i]);
        }

        public void ValidateDefaultData()
        {
            Data.ValidateDefaultData();
        }
    }
}