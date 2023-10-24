using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.UI
{
    public class DLSLogger : MonoBehaviour
    {
        public static DLSLogger Instance;

        [Header("References")]
        public Transform LoggingMsgsHolder;

        public Button OpenLogsButton;
        public TMP_Text LogsCounterText;

        public Button ShowDebugToggle;
        public Button ShowWarnToggle;
        public Button ShowErrorToggle;

        public LoggingMessage LogMsgTemplate;

        public Sprite DebugSprite;
        public Sprite WarnSprite;
        public Sprite ErrorSprite;

        public Color DisabledCol;
        public Color EnabledCol;

        public Color DebugCol;
        public Color ErrorCol;
        public Color WarnCol;

        private static bool _showDebug;
        private static bool _showWarn;
        private static bool _showError;

        public static List<GameObject> AllDebugLogs = new List<GameObject>();
        public static List<GameObject> AllWarnLogs = new List<GameObject>();
        public static List<GameObject> AllErrorLogs = new List<GameObject>();

        public static List<GameObject> AllLogs = new List<GameObject>();

        private static LoggingMessage _debugMessageTemplate;
        private static LoggingMessage _warningMessageTemplate;
        private static LoggingMessage _errorMessageTemplate;

        private void OnDestroy()
        {
            AllDebugLogs.Clear();
            AllWarnLogs.Clear();
            AllErrorLogs.Clear();
            AllLogs.Clear();
        }

        private void Awake()
        {
            Instance = this;
            _showDebug = PlayerPrefs.GetInt("LogDebug", 0) == 1;
            _showWarn = PlayerPrefs.GetInt("LogWarning", 1) == 1;
            _showError = PlayerPrefs.GetInt("LogError", 1) == 1;

            UpdateOpenLogsButton();

            ShowDebugToggle.image.color = _showDebug ? EnabledCol : DisabledCol;
            ShowWarnToggle.image.color = _showWarn ? EnabledCol : DisabledCol;
            ShowErrorToggle.image.color = _showError ? EnabledCol : DisabledCol;

            _debugMessageTemplate = Instantiate(LogMsgTemplate, transform, false)
                                    .GetComponent<LoggingMessage>();
            _debugMessageTemplate.gameObject.SetActive(false);

            _warningMessageTemplate = Instantiate(LogMsgTemplate, transform, false)
                                        .GetComponent<LoggingMessage>();
            _warningMessageTemplate.IconImage.sprite = WarnSprite;
            _warningMessageTemplate.IconImage.color = WarnCol;
            _warningMessageTemplate.HeaderText.color = WarnCol;
            _warningMessageTemplate.gameObject.SetActive(false);

            _errorMessageTemplate = Instantiate(LogMsgTemplate, transform, false)
                                    .GetComponent<LoggingMessage>();
            _errorMessageTemplate.IconImage.sprite = ErrorSprite;
            _errorMessageTemplate.IconImage.color = ErrorCol;
            _errorMessageTemplate.HeaderText.color = ErrorCol;
            _errorMessageTemplate.gameObject.SetActive(false);
        }

        public void ToggleShowDebug()
        {
            _showDebug = !_showDebug;
            ShowDebugToggle.image.color = _showDebug ? EnabledCol : DisabledCol;
            SetActiveAll(_showDebug, AllDebugLogs);
            PlayerPrefs.SetInt("LogDebug", _showDebug ? 1 : 0);
            UpdateOpenLogsButton();
        }

        public void ToggleShowWarn()
        {
            _showWarn = !_showWarn;
            ShowWarnToggle.image.color = _showWarn ? EnabledCol : DisabledCol;
            SetActiveAll(_showWarn, AllWarnLogs);
            PlayerPrefs.SetInt("LogWarning", _showWarn ? 1 : 0);
            UpdateOpenLogsButton();
        }

        public void ToggleShowError()
        {
            _showError = !_showError;
            ShowErrorToggle.image.color = _showError ? EnabledCol : DisabledCol;
            SetActiveAll(_showError, AllErrorLogs);
            PlayerPrefs.SetInt("LogError", _showError ? 1 : 0);
            UpdateOpenLogsButton();
        }

        private static void SetActiveAll(bool active, List<GameObject> collection)
        {
            foreach (GameObject obj in collection)
            {
                obj.SetActive(active);
            }
        }

        public void ClearLogs()
        {
            foreach (GameObject msg in AllLogs)
            {
                GameObject.Destroy(msg);
            }
            AllDebugLogs.Clear();
            AllWarnLogs.Clear();
            AllErrorLogs.Clear();
            AllLogs.Clear();

            UpdateOpenLogsButton();
        }

        private static GameObject NewLogMessage(LoggingMessage template, string message, string details)
        {
            bool detailed = !String.IsNullOrEmpty(details);
            template.HeaderText.text = message;
            template.DropDownButon.interactable = detailed;
            template.ContentText.text = detailed ? details : "";
            GameObject newMessage =
                Instantiate(template.gameObject, Instance.LoggingMsgsHolder, false);
            AllLogs.Add(newMessage);
            return newMessage;
        }

        private static void UpdateOpenLogsButton()
        {
            if (_showDebug && AllWarnLogs.Count == 0 && AllErrorLogs.Count == 0)
            {
                Instance.OpenLogsButton.image.color = Instance.DebugCol;
                Instance.OpenLogsButton.image.sprite = Instance.DebugSprite;
                Instance.LogsCounterText.text =
                    AllDebugLogs.Count < 100 ? AllDebugLogs.Count.ToString() : "99+";
            }
            else if (_showWarn && AllErrorLogs.Count == 0)
            {
                Instance.OpenLogsButton.image.color =
                    AllWarnLogs.Count > 0 ? Instance.WarnCol : Instance.DebugCol;
                Instance.OpenLogsButton.image.sprite = Instance.WarnSprite;
                Instance.LogsCounterText.text =
                    AllWarnLogs.Count < 100 ? AllWarnLogs.Count.ToString() : "99+";
            }
            else
            {
                Instance.OpenLogsButton.image.color =
                    AllErrorLogs.Count > 0 ? Instance.ErrorCol : Instance.DebugCol;
                Instance.OpenLogsButton.image.sprite = Instance.ErrorSprite;
                Instance.LogsCounterText.text =
                    AllErrorLogs.Count < 100 ? AllErrorLogs.Count.ToString() : "99+";
            }
        }

        public static void Log(string message, string details = "")
        {
            Debug.Log(!String.IsNullOrEmpty(details) ? message + ": " + details
                                                    : message);
            GameObject newMessage =
                NewLogMessage(_debugMessageTemplate, message, details);
            AllDebugLogs.Add(newMessage);
            newMessage.SetActive(_showDebug);
            UpdateOpenLogsButton();
        }

        public static void LogWarning(string message, string details = "")
        {
            Debug.LogWarning(!String.IsNullOrEmpty(details) ? message + ": " + details
                                                            : message);
            GameObject newMessage =
                NewLogMessage(_warningMessageTemplate, message, details);
            AllWarnLogs.Add(newMessage);
            newMessage.SetActive(_showWarn);
            UpdateOpenLogsButton();
        }

        public static void LogError(string message, string details = "")
        {
            Debug.LogError(!String.IsNullOrEmpty(details) ? message + ": " + details
                                                        : message);
            GameObject newMessage =
                NewLogMessage(_errorMessageTemplate, message, details);
            AllErrorLogs.Add(newMessage);
            newMessage.SetActive(_showError);
            UpdateOpenLogsButton();
            if (_showError)
            {
                UIManager.Instance.OpenMenu(MenuType.LoggingMenu);
            }
        }
    }
}