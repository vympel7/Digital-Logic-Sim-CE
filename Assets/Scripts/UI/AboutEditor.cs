using UnityEngine;

namespace Assets.Scripts.UI
{
	[ExecuteInEditMode]
	public class AboutEditor : MonoBehaviour
	{
		public TMPro.TMP_Text Target;
		public CustomCols[] Cols;
		public CustomSizes[] Sizes;

		private TMPro.TMP_Text _source;

		private void Update()
		{
			if (!Application.isPlaying)
			{
				if (_source == null)
				{
					_source = GetComponent<TMPro.TMP_Text>();
				}
				string formattedText = _source.text;
				if (Cols != null)
				{
					for (int i = 0; i < Cols.Length; i++)
					{
						string key = $"<color={Cols[i].Name}>";
						string replace = $"<color=#{ColorUtility.ToHtmlStringRGB(Cols[i].Colour)}>";
						formattedText = formattedText.Replace(key, replace);
					}
				}

				if (Sizes != null)
				{
					for (int i = 0; i < Sizes.Length; i++)
					{
						string key = $"<size={Sizes[i].Name}>";
						string replace = $"<size={Sizes[i].FontSize}>";
						formattedText = formattedText.Replace(key, replace);
					}
				}

				Target.text = formattedText;
			}
		}

		[System.Serializable]
		public struct CustomSizes
		{
			public string Name;
			public int FontSize;
		}

		[System.Serializable]
		public struct CustomCols
		{
			public string Name;
			public Color Colour;
		}
	}
}