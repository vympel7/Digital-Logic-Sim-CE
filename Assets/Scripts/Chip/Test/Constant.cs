using UnityEngine;

namespace Assets.Scripts.Chip.Test
{
	public class Constant : Chip
	{
		public bool high;
		public MeshRenderer meshRenderer;
		public Graphics.Palette palette;
		
		public void SendSignal()
		{
			OutputPins[0].ReceiveSignal (high ? 1 : 0);
			//Debug.Log ("Send const signal to " + outputPins[0].childPins[0].pinName + " " + outputPins[0].childPins[0].chip.chipName);
		}

		private void Update()
		{
			meshRenderer.material.color = high ? palette.OnCol : palette.OffCol;
		}
	}
}