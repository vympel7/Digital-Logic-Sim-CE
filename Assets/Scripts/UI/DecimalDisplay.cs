using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    using Scripts.Chip;
    using Scripts.Interaction;

    public class DecimalDisplay : MonoBehaviour
    {
        public TMP_Text TextPrefab;
        private ChipInterfaceEditor _signalEditor;

        private List<SignalGroup> _displayGroups;

        private void Start()
        {
            _displayGroups = new List<SignalGroup>();

            _signalEditor = GetComponent<ChipInterfaceEditor>();
            _signalEditor.OnChipsAddedOrDeleted += RebuildGroups;
        }

        private void Update()
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            foreach (SignalGroup signalGroup in _displayGroups)
                signalGroup.UpdateDisplay(_signalEditor);
        }

        private void RebuildGroups()
        {
            for (int i = 0; i < _displayGroups.Count; i++)
            {
                Destroy(_displayGroups[i].Text.gameObject);
            }
            _displayGroups.Clear();

            var groups = _signalEditor.GetGroups();

            foreach (var group in groups)
            {
                if (group[0].displayGroupDecimalValue)
                {
                    TMP_Text text = Instantiate(TextPrefab);
                    text.transform.SetParent(transform, true);
                    _displayGroups.Add(new SignalGroup() { Signals = group, Text = text });
                }
            }

            UpdateDisplay();
        }

        public class SignalGroup
        {
            public ChipSignal[] Signals;
            public TMP_Text Text;

            public void UpdateDisplay(ChipInterfaceEditor editor)
            {
                if (editor.SelectedSignals.Contains(Signals[0]))
                {
                    Text.gameObject.SetActive(false);
                }
                else
                {
                    Text.gameObject.SetActive(true);
                    float yPos = (Signals[0].transform.position.y + Signals[Signals.Length - 1].transform.position.y) / 2f;
                    Text.transform.position = new Vector3(editor.transform.position.x, yPos, -0.5f);

                    bool useTwosComplement = Signals[0].useTwosComplement;

                    int decimalValue = 0;
                    for (int i = 0; i < Signals.Length; i++)
                    {
                        int signalState = Signals[Signals.Length - 1 - i].CurrentState;
                        if (useTwosComplement && i == Signals.Length - 1)
                            decimalValue |= -(signalState << i);
                        else
                            decimalValue |= signalState << i;
                    }
                    Text.text = decimalValue + "";
                }
            }
        }
    }
}