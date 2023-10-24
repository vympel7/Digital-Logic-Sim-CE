using UnityEngine;

namespace Assets.Scripts.Interaction
{
	public class NavBar : MonoBehaviour
	{
		public GameObject[] Tabs;
		public int ActiveTab = 0;

		public void NexTab()
		{
			Step(1);
		}

		public void PrevTab()
		{
			Step(-1);
		}

		private void Step(int step)
		{
			Tabs[ActiveTab].SetActive(false);
			ActiveTab = Mathf.Abs((ActiveTab + step) % Tabs.Length);
			Tabs[ActiveTab].SetActive(true);
		}
	}
}