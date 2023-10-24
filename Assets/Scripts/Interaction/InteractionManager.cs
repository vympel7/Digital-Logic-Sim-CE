using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Interaction
{
    public class InteractionManager : MonoBehaviour
    {
        public static InteractionManager Instance;

        private Interactable _interactableWhitFocus;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (_interactableWhitFocus == null) return;

            if (InputHelper.AnyOfTheseKeysDown(KeyCode.Backspace, KeyCode.Delete) || Input.GetMouseButton(2))
                _interactableWhitFocus.DeleteCommand();
        }

        public bool HadFocus(Interactable interactable) => _interactableWhitFocus == interactable;
        public void ReleaseFocus(Interactable interactable)
        {
            if (HadFocus(interactable))
            {
                _interactableWhitFocus.HasFocus = false;
                _interactableWhitFocus = null;
            }
        }

        public bool RequestFocus(Interactable interactable)
        {

            if (_interactableWhitFocus == null)
                SetInteragibleWhitFocus(interactable);
            else if (interactable != _interactableWhitFocus)
            {
                if (_interactableWhitFocus.CanReleaseFocus())
                {
                    _interactableWhitFocus.HasFocus = false;
                    _interactableWhitFocus.FocusLostHandler();
                    SetInteragibleWhitFocus(interactable);
                }
                else
                    return false;
            }

            return true;
        }

        private void SetInteragibleWhitFocus(Interactable interactable)
        {
            _interactableWhitFocus = interactable;
            _interactableWhitFocus.HasFocus = true;
        }
    }
}