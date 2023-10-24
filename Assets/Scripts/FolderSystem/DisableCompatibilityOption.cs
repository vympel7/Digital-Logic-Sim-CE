using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.FolderSystem
{
    public class DisableCompatibilityOption : MonoBehaviour
    {
        private void Start()
        {
            var ChipButtonHolder = UI.ChipBarUI.Instance.ChipButtonHolders;
            if (ChipButtonHolder != null && ChipButtonHolder.Count > 0)
            {
                var chipButtonHolders = ChipButtonHolder[0];
                var CompatibilityOption = transform.GetChild(1).GetComponent<Toggle>();
                if (CompatibilityOption != null)
                    CompatibilityOption.interactable = chipButtonHolders.Holder.childCount != 0;
            }
        }
    }
}