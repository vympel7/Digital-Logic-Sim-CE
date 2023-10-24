using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.SaveSystem
{
	using Scripts.Chip;
	using Scripts.Chip.Test;
	using Scripts.Graphics;
	using Scripts.Interaction;

	public class SavedChipEditor : MonoBehaviour
	{
		public bool loadInEditMode;
		public string chipToEditName;
		public Wire wirePrefab;
		GameObject loadedChipHolder;

		public void Load(string chipName, GameObject chipHolder)
		{

			var loadedChip = Instantiate(chipHolder);
			loadedChip.transform.parent = transform;

			List<Chip> topLevelChips = new List<Chip>(GetComponentsInChildren<Chip>());

			var subChips = GetComponentsInChildren<CustomChip>(includeInactive: true);
			for (int i = 0; i < subChips.Length; i++)
			{
				if (subChips[i].transform.parent == loadedChip.transform)
				{
					subChips[i].gameObject.SetActive(true);
					topLevelChips.Add(subChips[i]);
				}
			}

			//topLevelChips.Sort ((a, b) => a.chipSaveIndex.CompareTo (b.chipSaveIndex));

			var wiringSaveData = SaveSystem.ReadWire(chipName);
			int wireIndex = 0;
			foreach (var savedWire in wiringSaveData.SerializableWires)
			{
				Wire loadedWire = GameObject.Instantiate(wirePrefab, parent: loadedChip.transform);
				loadedWire.SetDepth(wireIndex);
				Pin parentPin = topLevelChips[savedWire.ParentChipIndex].OutputPins[savedWire.ParentChipOutputIndex];
				Pin childPin = topLevelChips[savedWire.ChildChipIndex].InputPins[savedWire.ChildChipInputIndex];
				loadedWire.Connect(parentPin, childPin);
				loadedWire.SetAnchorPoints(savedWire.AnchorPoints);
				FindObjectOfType<PinAndWireInteraction>().LoadWire(loadedWire);
				//player.AddWire (loadedWire);

				if (childPin.Chip is Bus)
					childPin.transform.position = savedWire.AnchorPoints[savedWire.AnchorPoints.Length - 1];

				if (parentPin.Chip is Bus)
					parentPin.transform.position = savedWire.AnchorPoints[0];
				wireIndex++;
			}

			loadedChipHolder = loadedChip;
		}

		public void CaptureLoadedChip(GameObject chipHolder)
		{
			if (loadedChipHolder)
			{
				for (int i = loadedChipHolder.transform.childCount - 1; i >= 0; i--)
				{
					loadedChipHolder.transform.GetChild(i).parent = chipHolder.transform;
				}
				Destroy(loadedChipHolder);
			}
		}
	}
}