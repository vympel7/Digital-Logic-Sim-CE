using UnityEngine;

namespace Assets.Scripts.UI.Menu
{
    using Scripts.Core;
    using Scripts.Interaction;
    using Scripts.SaveSystem;

    public class ChipPropertiesMenu : MonoBehaviour
    {
        [SerializeField]
        private RectTransform propertiesUI;
        public Vector2 PropertiesHeightMinMax;

        //fuctionality
        public TMPro.TMP_InputField NameField;
        public UnityEngine.UI.Button DeleteButton;
        public UnityEngine.UI.Toggle TwosComplementToggle;
        public TMPro.TMP_Dropdown ModeDropdown;

        private ChipInterfaceEditor _currentInterface;

        private bool _opened = false;

        private void Awake()
        {
            propertiesUI = (RectTransform)transform.GetChild(0);
        }
        // Start is called before the first frame update
        private void Start()
        {
            DeleteButton.onClick.AddListener(Delete);
            ModeDropdown.onValueChanged.AddListener(OnValueDropDownChange);
        }

        public void SetActive(bool b)
        {
            propertiesUI.gameObject.SetActive(b);
        }

        public void EnableUI(ChipInterfaceEditor chipInterfaceEditor, string signalName, bool isGroup, bool useTwosComplement, string currentEditorName, string signalToDragName, int wireType)
        {
            SetActive(true);
            NameField.text = signalName;
            NameField.Select();
            NameField.caretPosition = NameField.text.Length;
            TwosComplementToggle.gameObject.SetActive(isGroup);
            TwosComplementToggle.isOn = useTwosComplement;
            ModeDropdown.gameObject.SetActive(!isGroup);
            DeleteButton.interactable = ChipSaver.IsSignalSafeToDelete(currentEditorName, signalToDragName);
            ModeDropdown.SetValueWithoutNotify(wireType);
            _currentInterface = chipInterfaceEditor;

            var SizeDelta = new Vector2(propertiesUI.sizeDelta.x, isGroup ? PropertiesHeightMinMax.y : PropertiesHeightMinMax.x);
            propertiesUI.sizeDelta = SizeDelta;

            _opened = true;
        }

        public void DisableUI()
        {
            if (!_opened) return;
            SetActive(false);
            SaveProperty();
            ResetC();
        }

        private void ResetC()
        {
            NameField.text = "";
            _currentInterface = null;
            _opened = false;
        }

        public void SetPosition(Vector3 centre, ChipInterfaceEditor.EditorType editorType)
        {
            float propertiesUIX = ScalingManager.PropertiesUIX * (editorType == ChipInterfaceEditor.EditorType.Input ? 1 : -1);
            propertiesUI.transform.position = new Vector3(centre.x + propertiesUIX, centre.y, propertiesUI.transform.position.z);
        }

        private void SaveProperty()
        {
            if (_currentInterface != null)
                _currentInterface.UpdateGroupProperty(NameField.text, TwosComplementToggle.isOn);
        }

        private void Delete()
        {
            _currentInterface.DeleteCommand();
        }

        private void OnValueDropDownChange(int mode)
        {
            if (_currentInterface != null)
                _currentInterface.ChangeWireType(mode);
        }
    }
}