using UnityEngine;

namespace Assets.Scripts.Graphics
{
	[CreateAssetMenu()]
	public class Palette : ScriptableObject
	{
		public Color OnCol;
		public Color OffCol;
		public Color HighZCol;
		public Color BusColor;
		public Color SelectedColor;
		public Color NonInteractableCol;
	}
}