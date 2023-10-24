using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class LoadProjectMenu : MonoBehaviour
    {
        public Button ProjectButtonPrefab;
        public Transform ScrollHolder;
        [SerializeField, HideInInspector]
        private List<Button> _loadButtons;

        private void OnEnable()
        {
            string[] projectNames = SaveSystem.SaveSystem.GetSaveNames();

            for (int i = 0; i < projectNames.Length; i++)
            {
                string projectName = projectNames[i];
                if (i >= _loadButtons.Count)
                    _loadButtons.Add(Instantiate(ProjectButtonPrefab, parent: ScrollHolder));
                Button loadButton = _loadButtons[i];
                loadButton.GetComponentInChildren<TMPro.TMP_Text>().text =
                    projectName.Trim();
                loadButton.onClick.AddListener(() => LoadProject(projectName));
            }
        }

        public void LoadProject(string projectName)
        {
            SaveSystem.SaveSystem.SetActiveProject(projectName);
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }
    }
}