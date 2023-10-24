using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
	public class MainMenu : MonoBehaviour
	{
		public static MainMenu Instance;

		public TMP_InputField ProjectNameField;
		public Button ConfirmProjectButton;
		public Toggle FullscreenToggle;
		public TMP_Dropdown VSyncRate;
		public TMP_Dropdown FpsTarget;

		private void Awake()
		{
			Instance = this;
			FullscreenToggle.onValueChanged.AddListener(SetFullScreen);

			// We opt to use vSync by default with vSyncCount of 1/2
			VSyncRate.value = PlayerPrefs.GetInt("vSyncRate", 2);
			// By default TargetFrames is 0 (-1)
			FpsTarget.value = PlayerPrefs.GetInt("fpsTarget", 0);
		}

		public void SetVSyncRatio(int value)
		{
			// Clear fpsTarget
			PlayerPrefs.SetInt("fpsTarget", 0);
			FpsTarget.value = 0;
			Application.targetFrameRate = -1;

			if (value == 0)
			{
				SetFpsTarget(3);
			}

			PlayerPrefs.SetInt("vSyncRate", value);
			VSyncRate.value = value;

			QualitySettings.vSyncCount = value;
		}

		public void SetFpsTarget(int value)
		{
			// Clear vSync Count
			PlayerPrefs.SetInt("vSyncRate", 0);
			VSyncRate.value = 0;
			QualitySettings.vSyncCount = 0;

			if (value == 0)
			{
				SetVSyncRatio(2);
			}

			PlayerPrefs.SetInt("fpsTarget", value);
			FpsTarget.value = value;

			Application.targetFrameRate = value != 0 ? value * 10 : -1;
		}

		private void LateUpdate()
		{
			ConfirmProjectButton.interactable = ProjectNameField.text.Trim().Length > 0;
			if (FullscreenToggle.isOn != Screen.fullScreen)
			{
				FullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
			}
		}

		public void StartNewProject()
		{
			string projectName = ProjectNameField.text;
			SaveSystem.SaveSystem.SetActiveProject(projectName);
			UnityEngine.SceneManagement.SceneManager.LoadScene(1);
		}

		public void SetResolution16x9(int width)
		{
			Screen.SetResolution(width, Mathf.RoundToInt(width * (9 / 16f)),
								Screen.fullScreenMode);
		}

		public void SetFullScreen(bool fullscreenOn)
		{
			// Screen.fullScreen = fullscreenOn;
			var nativeRes = Screen.resolutions[Screen.resolutions.Length - 1];
			if (fullscreenOn)
			{
				Screen.SetResolution(nativeRes.width, nativeRes.height, FullScreenMode.FullScreenWindow);
			}
			else
			{
				float windowedScale = 0.75f;
				int x = nativeRes.width / 16;
				int y = nativeRes.height / 9;
				int m = (int)(Mathf.Min(x, y) * windowedScale);
				Screen.SetResolution(16 * m, 9 * m, FullScreenMode.Windowed);
			}
		}

		public void Quit() { Application.Quit(); }
  	}
}