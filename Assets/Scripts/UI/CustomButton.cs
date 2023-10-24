using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class CustomButton : Button, IPointerDownHandler
    {
        public event System.Action OnPointerDownEvent;
        public List<System.Action> Events = new List<System.Action>();

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            OnPointerDownEvent?.Invoke();
        }

        public void AddListener(System.Action action)
        {
            OnPointerDownEvent += action;
            Events.Add(action);
        }

        public void ClearEvents()
        {
            foreach (System.Action a in Events)
                OnPointerDownEvent -= a;

            Events.Clear();
        }
    }
}