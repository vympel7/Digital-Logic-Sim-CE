using System.Collections;
using UnityEngine;
using TMPro;

namespace Assets.Scripts.Chip
{
    public class Clock : BuiltinChip
    {
        private WaitForSeconds _waiter;
        [SerializeField]
        private float _hz = 1f;
        public float Hz
        {
            get => _hz;
            set
            {
                _hz = value;
                _hzText.text = $"{_hz} Hz";
                _waiter = new WaitForSeconds(1 / (2 * Hz));
                StopAllCoroutines();
                StartCoroutine(ClockTick());
            }
        }

        [SerializeField]
        private TMP_Text _hzText;
        [SerializeField]
        private GameObject _hzEditor;

        protected override void Start()
        {
            base.Start();
            _hzText.text = $"{_hz} Hz";
            StartCoroutine(ClockTick());
            _waiter = new WaitForSeconds(1 / (2 * Hz));
        }

        protected override void ProcessOutput() { }

        private IEnumerator ClockTick()
        {
            yield return _waiter;
            OutputPins[0].ReceiveSignal(1);
            yield return _waiter;
            OutputPins[0].ReceiveSignal(0);
            StartCoroutine(ClockTick());
        }

        private void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(1))
                UI.UIManager.Instance.OpenMenu(UI.MenuType.ClockMenu);
        }
    }
}