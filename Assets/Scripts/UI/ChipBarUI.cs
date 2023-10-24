using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    using Scripts.Chip;
    using Scripts.Core;
    using Scripts.FolderSystem;
    using Scripts.Graphics;

    public class ChipBarUI : MonoBehaviour
    {
        public static ChipBarUI Instance;
        public Manager Manager;

        public RectTransform Bar;
        public GameObject ChipButtonHolderPrefab;
        public CustomButton ButtonPrefab;

        public float ButtonSpacing = 15f;
        public float ButtonWidthPadding = 10;

        public List<string> HideList;

        public Scrollbar HorizontalScroll;
        public ScrollRect ScrollRect;
        public Transform ScrollRectViewport;
        public TMP_Dropdown FolderDropdown;
        public TMP_Text Label;

        public Sprite BuiltInSprite;
        public Sprite UserSprite;
        public Sprite NewFolderIcon;

        public List<CustomButton> CustomButton = new List<CustomButton>();
        public Dictionary<int, (RectTransform Holder, int Value)> ChipButtonHolders = new Dictionary<int, (RectTransform Holder, int Value)>();


        public static int CurrentFolderIndex = 0;
        private TMP_Dropdown.OptionData _newFolderOption = new TMP_Dropdown.OptionData("New Folder");

        private void Awake()
        {
            Instance = this;
            Manager = FindObjectOfType<Manager>();
        }

        private void Start()
        {
            Manager.CustomChipCreated += AddChipButton;
            Manager.CustomChipUpdated += UpdateChipButton;
            _newFolderOption = new TMP_Dropdown.OptionData()
            {
                text = "New Folder",
                image = NewFolderIcon
            };
            FolderDropdown.AddOptions(new List<TMP_Dropdown.OptionData> { _newFolderOption });


            ReloadFolder();
            FolderDropdown.value = 1;
        }

        public void ReloadChipButton()
        {

            foreach (var button in CustomButton)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }

            CustomButton.Clear();

            foreach (var BuiltInChip in Manager.BuiltinChips)
                AddChipButton(BuiltInChip);

            foreach (var Customchip in Manager.SpawnableCustomChips)
                AddChipButton(Customchip);

            Canvas.ForceUpdateCanvases();
        }


        private void ReloadFolder()
        {
            foreach (var Holder in ChipButtonHolders)
                DestroyImmediate(Holder.Value.Holder.gameObject);
            ChipButtonHolders.Clear();

            FolderDropdown.options.Clear();

            foreach (var kv in FolderSystem.Enum)
                AddFolderView(kv.Key, kv.Key > 2 ? UserSprite : BuiltInSprite);

            ReloadChipButton();
        }

        public void NotifyRemovedFolder(string folderName)
        {
            var DeletedWhileOnFolder = string.Equals(FolderDropdown.options[FolderDropdown.value].text, folderName);

            ReloadFolder();
            if (DeletedWhileOnFolder)
                FolderDropdown.value = 0;

            FolderDropdown.onValueChanged?.Invoke(FolderDropdown.value);
        }

        public void NotifyFolderNameChanged()
        {
            ReloadFolder();
            FolderDropdown.onValueChanged?.Invoke(FolderDropdown.value);
            Label.text = FolderDropdown.options[FolderDropdown.value].text;
        }

        private void LateUpdate() { UpdateBarPos(); }

        private void UpdateBarPos()
        {
            float barPosY = HorizontalScroll.gameObject.activeSelf ? 16 : 0;
            Bar.localPosition = new Vector3(0, barPosY, 0);
        }

        private void AddChipButton(Chip chip)
        {
            if (HideList.Contains(chip.ChipName))
                return;

            ChipPackage package = chip.GetComponent<ChipPackage>();
            Transform holder;
            switch (package.Type)
            {
                case ChipPackage.ChipType.Compatibility:
                    holder = ChipButtonHolders[(int)DefaultKays.Comp].Holder.transform;
                    break;
                case ChipPackage.ChipType.Gate:
                    holder = ChipButtonHolders[(int)DefaultKays.Gate].Holder.transform;
                    break;
                case ChipPackage.ChipType.Miscellaneous:
                    holder = ChipButtonHolders[(int)DefaultKays.Misc].Holder.transform;
                    break;
                default:

                    int index = (int)DefaultKays.Comp;
                    if (chip is CustomChip customChip)
                    {
                        if (FolderSystem.ContainsIndex(customChip.FolderIndex))
                            index = customChip.FolderIndex;
                    }
                    holder = ChipButtonHolders[index].Holder.transform;
                    break;
            }

            CustomButton button = Instantiate(ButtonPrefab);
            button.gameObject.name = "Create (" + chip.ChipName + ")";
            // Set button text
            var buttonTextUI = button.GetComponentInChildren<TMP_Text>();
            buttonTextUI.text = chip.ChipName;

            // Set button size
            var buttonRect = button.GetComponent<RectTransform>();
            buttonRect.sizeDelta =
                new Vector2(buttonTextUI.preferredWidth + ButtonWidthPadding,
                            buttonRect.sizeDelta.y);

            // Set button position
            buttonRect.SetParent(holder, false);

            // Set button event
            button.AddListener(() => Manager.ChipButtonHanderl(chip));

            CustomButton.Add(button);
        }

        private void UpdateChipButton(Chip chip)
        {
            if (HideList.Contains(chip.ChipName))
                return;

            CustomButton button =
                CustomButton.Find(g => g.name == "Create (" + chip.ChipName + ")");
            if (button != null)
            {
                button.ClearEvents();
                button.AddListener(() => Manager.ChipButtonHanderl(chip));
            }
        }

        public void SelectFolder()
        {
            if (FolderDropdown.value == FolderDropdown.options.Count - 1)
            {
                UIManager.Instance.OpenMenu(MenuType.NewFolderMenu);
                FolderDropdown.value = ChipButtonHolders[CurrentFolderIndex].Value; // TODO set Last Used Folder
                return;
            }

            CurrentFolderIndex = FolderSystem.ReverseIndex(FolderDropdown.options[FolderDropdown.value].text);

            foreach (var chipHolder in ChipButtonHolders)
                chipHolder.Value.Holder.gameObject.SetActive(false);

            var HolderSelecter = ChipButtonHolders[CurrentFolderIndex].Holder;
            HolderSelecter.gameObject.SetActive(true);

            ScrollRect.content = HolderSelecter;
            UpdateBarPos();
        }

        public void AddFolderView(int FolderIndex, Sprite sprite)
        {
            var folderName = FolderSystem.GetFolderName(FolderIndex);
            TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(folderName, sprite);


            FolderDropdown.options.Remove(_newFolderOption);
            FolderDropdown.options.Add(newOption);
            FolderDropdown.options.Add(_newFolderOption);
            NewChipButtonHolder(FolderIndex, folderName);
        }

        void NewChipButtonHolder(int FolderIdex, string FolderName)
        {
            RectTransform newHolder = Instantiate(ChipButtonHolderPrefab).GetComponent<RectTransform>();
            newHolder.gameObject.name = FolderName + "Chips";
            newHolder.gameObject.SetActive(false);
            ChipButtonHolders.Add(FolderIdex, (newHolder, FolderDropdown.options.Count - 2));
            newHolder.SetParent(ScrollRectViewport, false);
        }
    }
}