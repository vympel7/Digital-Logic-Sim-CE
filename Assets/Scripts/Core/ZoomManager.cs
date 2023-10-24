using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core
{
    using Scripts.Interaction;

    public class ZoomManager : MonoBehaviour
    {
        public static ZoomManager Instance;

        private const float _maxCamOrthoSize = 4.7f;
        private const float _minCamOrthoSize = 0.7f;

        public static float Zoom = 0f;

        [Header("Main Settings")]
        [Range(0, 1)]
        public float TargetZoom = 0f;
        public bool ShowZoomHelper = true;

        [Header("Mouse Zoom Settings")]
        public float MouseWheelSensitivity = 0.1f;
        public float MouseWheelDeadzone = 0.01f;
        public float MouseZoomSpeed = 12f;
        public float CamMoveSpeed = 12f;

        [Header("References")]
        public Camera Ccam;
        public GameObject ZoomHelpPanel;
        public RectTransform ZoomHelpViewport;
        public Camera ZoomHelpCam;

        private Vector2 _maxZoomViewportSize = new Vector2(320, 180);
        private Vector2 _minZoomViewportSize = new Vector2(64, 36);

        private Vector2 _zoomMoveRange = new Vector2(7f, 3.4f);
        private Vector3 _targetCamPosition = Vector3.zero;

        private Vector3 _focusOffset = new Vector3(0, -0.2f, 0);
        private Vector2 _zoomViewportMoveRange = new Vector2(132, 68);

        private void Awake() { Instance = this; }

        private void Update()
        {
            if (!InputHelper.MouseOverUIObject())
            {
                if (Input.GetKey(KeyCode.F))
                {
                    if (ChipInteraction.s_SelectedChips.Count > 0)
                    {
                        List<Vector3> chipPositions = new List<Vector3>();
                        foreach (Chip.Chip chip in ChipInteraction.s_SelectedChips)
                        {
                            chipPositions.Add(chip.transform.position);
                        }
                        _targetCamPosition = Utility.MathUtility.Center(chipPositions) + _focusOffset;

                        // TODO: set target zoom based on selection world size
                        TargetZoom = 1;
                    }
                    else
                    {
                        _targetCamPosition = InputHelper.MouseWorldPos;
                    }

                }
                else
                {
                    Vector3 moveVec = new Vector3();
                    moveVec.x +=
                        InputHelper.AnyOfTheseKeysHeld(KeyCode.RightArrow, KeyCode.D) ? 1
                                                                                    : 0;
                    moveVec.x -=
                        InputHelper.AnyOfTheseKeysHeld(KeyCode.LeftArrow, KeyCode.A) ? 1
                                                                                    : 0;
                    moveVec.y +=
                        InputHelper.AnyOfTheseKeysHeld(KeyCode.UpArrow, KeyCode.W) ? 1 : 0;
                    moveVec.y -=
                        InputHelper.AnyOfTheseKeysHeld(KeyCode.DownArrow, KeyCode.S) ? 1
                                                                                    : 0;
                    _targetCamPosition =
                        _targetCamPosition + (moveVec * CamMoveSpeed) * 0.01f;
                }

                if (Input.GetKeyDown(KeyCode.G))
                {
                    TargetZoom = 0;
                    _targetCamPosition = Vector3.zero;
                }

                float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
                if ((scrollAmount > MouseWheelDeadzone || scrollAmount <
                                                            -MouseWheelDeadzone) &&
                    !InputHelper.MouseOverUIObject())
                {
                    // targetCamPosition = InputHelper.MouseWorldPos;
                    TargetZoom = Mathf.Clamp01(Zoom + scrollAmount * MouseWheelSensitivity);
                    if (ChipInteraction.s_SelectedChips.Count > 0)
                    {
                        List<Vector3> chipPositions = new List<Vector3>();
                        foreach (Chip.Chip chip in ChipInteraction.s_SelectedChips)
                        {
                            chipPositions.Add(chip.transform.position);
                        }
                        _targetCamPosition = Utility.MathUtility.Center(chipPositions) + _focusOffset;
                    }
                }
                Zoom = Mathf.Lerp(Zoom, TargetZoom, MouseZoomSpeed * Time.deltaTime);
            }
        }

        private void LateUpdate()
        {
            Ccam.orthographicSize = CalcCameraOrthoSize();

            _targetCamPosition =
                new Vector3(Mathf.Clamp(_targetCamPosition.x, -_zoomMoveRange.x * Zoom,
                                        _zoomMoveRange.x * Zoom),
                            Mathf.Clamp(_targetCamPosition.y, -_zoomMoveRange.y * Zoom,
                                        _zoomMoveRange.y * Zoom),
                            0);
            transform.position = Vector3.Lerp(transform.position, _targetCamPosition,
                                            CamMoveSpeed * Time.deltaTime);

            UpdateZoomHelper();
        }

        private void UpdateZoomHelper()
        {
            if (ShowZoomHelper)
            {
                if (Zoom >= 0.1 && !ZoomHelpPanel.activeInHierarchy)
                {
                    ZoomHelpCam.gameObject.SetActive(true);
                    ZoomHelpPanel.SetActive(true);
                }
                else if (Zoom < 0.1 && ZoomHelpPanel.activeInHierarchy)
                {
                    ZoomHelpCam.gameObject.SetActive(false);
                    ZoomHelpPanel.SetActive(false);
                }

                Vector2 viewportSize = new Vector2(
                    Mathf.Lerp(_maxZoomViewportSize.x, _minZoomViewportSize.x, Zoom),
                    Mathf.Lerp(_maxZoomViewportSize.y, _minZoomViewportSize.y, Zoom));
                ZoomHelpViewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
                                                        viewportSize.x);
                ZoomHelpViewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                                                        viewportSize.y);

                ZoomHelpViewport.anchoredPosition = CalcViewPortSize();

            }
            else if (ZoomHelpPanel.activeInHierarchy)
            {
                ZoomHelpCam.gameObject.SetActive(false);
                ZoomHelpPanel.SetActive(false);
            }
        }

        private float CalcCameraOrthoSize()
        {
            Zoom = Mathf.Clamp01(Zoom);
            return Mathf.Lerp(_maxCamOrthoSize, _minCamOrthoSize, Zoom);
        }

        private Vector2 CalcViewPortSize()
        {
            AnimationCurve curve = new AnimationCurve(
                new Keyframe(-_zoomMoveRange.x, -_zoomViewportMoveRange.x),
                new Keyframe(_zoomMoveRange.x, _zoomViewportMoveRange.x));
            curve.SmoothTangents(0, 0);
            curve.SmoothTangents(1, 0);

            float xPos = curve.Evaluate(transform.position.x);

            curve.MoveKey(0, new Keyframe(-_zoomMoveRange.y, -_zoomViewportMoveRange.y));
            curve.MoveKey(1, new Keyframe(_zoomMoveRange.y, _zoomViewportMoveRange.y));

            float yPos = curve.Evaluate(transform.position.y);

            return new Vector2(xPos, yPos);
        }
    }
}