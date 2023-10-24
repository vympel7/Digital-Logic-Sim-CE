﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Interaction
{
    public static class InputHelper
    {
        private static Camera _mainCamera = Camera.main;

        public static Vector2 MouseWorldPos => _mainCamera.ScreenToWorldPoint(Input.mousePosition);

        public static bool MouseOverUIObject() =>
            EventSystem.current.IsPointerOverGameObject();

        public static GameObject GetObjectUnderMouse2D(LayerMask mask)
        {
            Vector2 mouse = MouseWorldPos;
            var hit = Physics2D.GetRayIntersection(
                new Ray(new Vector3(mouse.x, mouse.y, -100), Vector3.forward),
                float.MaxValue, mask);
            if (hit.collider)
                return hit.collider.gameObject;
            return null;
        }

        public static List<GameObject> GetUIObjectsUnderMouse()
        {
            List<GameObject> objects = new List<GameObject>();
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            foreach (RaycastResult result in results)
            {
                objects.Add(result.gameObject);
            }
            return objects;
        }

        public static bool AnyOfTheseKeysDown(params KeyCode[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (Input.GetKeyDown(keys[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool AnyOfTheseKeysHeld(params KeyCode[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (Input.GetKey(keys[i]))
                {
                    return true;
                }
            }
            return false;
        }
    }
}