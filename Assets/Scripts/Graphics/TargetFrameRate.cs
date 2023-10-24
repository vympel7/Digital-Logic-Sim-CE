using UnityEngine;

public class TargetFrameRate : MonoBehaviour
{
	private void Awake()
	{
		// It's a dead simple app. There's no need for 120 fps
		// By default the vSyncCount is 2 (1/2 of max fps, ex. 120/2 = 60)

		QualitySettings.vSyncCount = PlayerPrefs.GetInt("vSyncRate", 2);
		Application.targetFrameRate = PlayerPrefs.GetInt("fpsTarget", 0);
	}
}
