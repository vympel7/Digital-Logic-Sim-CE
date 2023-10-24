using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Graphics
{
    using Scripts.Chip;
    using Scripts.Core;

    public class Wire : MonoBehaviour
    {
        public Material SimpleMat;

        [HideInInspector]
        public LineRenderer LineRenderer;
        public Color EditCol;
        private Palette _palette;
        public Color PlacedCol;
        public float CurveSize = 0.5f;
        public int Resolution = 10;
        private bool _selected;

        private bool _wireConnected;
        public Pin StartPin;
        public Pin EndPin;

        public bool SimActive = false;
        private EdgeCollider2D _wireCollider;
        public List<Vector2> AnchorPoints { get; private set; }
        private List<Vector2> _drawPoints;
        private const float _thicknessMultiplier = 0.1f;
        private float _length;
        private Material _mat;
        private float _depth;

        private void Awake()
        {
            LineRenderer = GetComponent<LineRenderer>();
            LineRenderer.startWidth =
                ScalingManager.WireSelectedThickness * _thicknessMultiplier;
            LineRenderer.endWidth =
                ScalingManager.WireSelectedThickness * _thicknessMultiplier;
        }

        void Start()
        {
            _palette = UI.UIManager.Instance.Palette;
            LineRenderer.material = SimpleMat;
            _mat = LineRenderer.material;
        }

        public Pin ChipInputPin => (StartPin.PType == Pin.PinType.ChipInput) ? StartPin : EndPin;

        public Pin ChipOutputPin => (StartPin.PType == Pin.PinType.ChipOutput) ? StartPin : EndPin;

        public void tellWireSimIsOff()
        {
            SimActive = false;
            StartPin.tellPinSimIsOff();
            EndPin.tellPinSimIsOff();
        }

        public void tellWireSimIsOn()
        {
            SimActive = true;
            StartPin.tellPinSimIsOn();
            EndPin.tellPinSimIsOn();
        }

        public void SetAnchorPoints(Vector2[] newAnchorPoints)
        {
            AnchorPoints = new List<Vector2>(newAnchorPoints);
            UpdateSmoothedLine();
            UpdateCollider();
        }

        public void SetDepth(int numWires)
        {
            _depth = numWires * 0.01f;
            transform.localPosition = Vector3.forward * _depth;
        }

        private void LateUpdate()
        {
            SetWireCol();
            if (_wireConnected)
            {
                float depthOffset = 5;

                transform.localPosition = Vector3.forward * (_depth + depthOffset);
                UpdateWirePos();
                // transform.position = new Vector3 (transform.position.x,
                // transform.position.y, inputPin.sequentialState * -0.01f);
            }
            LineRenderer.startWidth = ((_selected) ? ScalingManager.WireSelectedThickness
                                                : ScalingManager.WireThickness) *
                                    _thicknessMultiplier;
            LineRenderer.endWidth = ((_selected) ? ScalingManager.WireSelectedThickness
                                                : ScalingManager.WireThickness) *
                                    _thicknessMultiplier;
        }

        private void UpdateWirePos()
        {
            const float maxSqrError = 0.00001f;
            // How far are start and end points from the pins they're connected to (chip
            // has been moved)
            Vector2 startPointError =
                (Vector2)StartPin.transform.position - AnchorPoints[0];
            Vector2 endPointError = (Vector2)EndPin.transform.position -
                                    AnchorPoints[AnchorPoints.Count - 1];

            if (startPointError.sqrMagnitude > maxSqrError ||
                endPointError.sqrMagnitude > maxSqrError)
            {
                // If start and end points are both same offset from where they should be,
                // can move all anchor points (entire wire)
                if ((startPointError - endPointError).sqrMagnitude < maxSqrError &&
                    startPointError.sqrMagnitude > maxSqrError)
                {
                    for (int i = 0; i < AnchorPoints.Count; i++)
                    {
                        AnchorPoints[i] += startPointError;
                    }
                }

                AnchorPoints[0] = StartPin.transform.position;
                AnchorPoints[AnchorPoints.Count - 1] = EndPin.transform.position;
                UpdateSmoothedLine();
                UpdateCollider();
            }
        }

        private void SetWireCol()
        {
            if (_wireConnected)
            {
                Color onCol = _palette.OnCol;
                Color offCol = _palette.OffCol;
                Color selectedCol = _palette.SelectedColor;

                if (_selected)
                {
                    _mat.color = selectedCol;
                }
                else
                {

                    // High Z
                    if (ChipOutputPin.State == -1)
                    {
                        onCol = _palette.HighZCol;
                        offCol = _palette.HighZCol;
                    }
                    if (SimActive)
                    {
                        if (StartPin.WType != Pin.WireType.Simple)
                        {
                            _mat.color = (ChipOutputPin.State == 0) ? offCol : _palette.BusColor;
                        }
                        else
                        {
                            _mat.color = (ChipOutputPin.State == 0) ? offCol : onCol;
                        }
                    }
                    else
                    {
                        _mat.color = offCol;
                    }
                }
            }
            else
            {
                _mat.color = Color.black;
            }
        }

        public void Connect(Pin inputPin, Pin outputPin)
        {
            ConnectToFirstPin(inputPin);
            Place(outputPin);
        }

        public void ConnectToFirstPin(Pin startPin)
        {
            StartPin = startPin;
            LineRenderer = GetComponent<LineRenderer>();
            _mat = SimpleMat;
            _drawPoints = new List<Vector2>();

            transform.localPosition = new Vector3(0, 0, transform.localPosition.z);

            _wireCollider = GetComponent<EdgeCollider2D>();

            AnchorPoints = new List<Vector2>
            {
                startPin.transform.position,
                startPin.transform.position
            };
            UpdateSmoothedLine();
            _mat.color = EditCol;
        }

        public void ConnectToFirstPinViaWire(Pin startPin, Wire parentWire, Vector2 inputPoint)
        {
            LineRenderer = GetComponent<LineRenderer>();
            _mat = SimpleMat;
            _drawPoints = new List<Vector2>();
            StartPin = startPin;
            transform.localPosition = new Vector3(0, 0, transform.localPosition.z);

            _wireCollider = GetComponent<EdgeCollider2D>();

            AnchorPoints = new List<Vector2>();

            // Find point on wire nearest to input point
            Vector2 closestPoint = Vector2.zero;
            float smallestDst = float.MaxValue;
            int closestI = 0;
            for (int i = 0; i < parentWire.AnchorPoints.Count - 1; i++)
            {
                var a = parentWire.AnchorPoints[i];
                var b = parentWire.AnchorPoints[i + 1];
                var pointOnWire = Utility.MathUtility.ClosestPointOnLineSegment(a, b, inputPoint);
                float sqrDst = (pointOnWire - inputPoint).sqrMagnitude;
                if (sqrDst < smallestDst)
                {
                    smallestDst = sqrDst;
                    closestPoint = pointOnWire;
                    closestI = i;
                }
            }

            for (int i = 0; i <= closestI; i++)
            {
                AnchorPoints.Add(parentWire.AnchorPoints[i]);
            }
            AnchorPoints.Add(closestPoint);
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                AnchorPoints.Add(closestPoint);
            }
            AnchorPoints.Add(inputPoint);

            UpdateSmoothedLine();
            _mat.color = EditCol;
        }

        // Connect the input pin to the output pin
        public void Place(Pin endPin)
        {
            EndPin = endPin;
            AnchorPoints[AnchorPoints.Count - 1] = endPin.transform.position;
            UpdateSmoothedLine();

            _wireConnected = true;
            UpdateCollider();

            if (endPin.PType == Pin.PinType.ChipOutput)
                SwapStartEndPoints();

            if (Simulation.Instance.Active)
                tellWireSimIsOn();
        }

        private void SwapStartEndPoints()
        {
            Pin temp = StartPin;
            StartPin = EndPin;
            EndPin = temp;

            AnchorPoints.Reverse();
            _drawPoints.Reverse();

            UpdateSmoothedLine();
            UpdateCollider();
        }

        // Update position of wire end point (for when initially placing the wire)
        public void UpdateWireEndPoint(Vector2 endPointWorldSpace)
        {
            AnchorPoints[AnchorPoints.Count - 1] = ProcessPoint(endPointWorldSpace);
            UpdateSmoothedLine();
        }

        // Add anchor point (for when initially placing the wire)
        public void AddAnchorPoint(Vector2 pointWorldSpace)
        {
            AnchorPoints[AnchorPoints.Count - 1] = ProcessPoint(pointWorldSpace);
            AnchorPoints.Add(ProcessPoint(pointWorldSpace));
        }

        private void UpdateCollider()
        {
            _wireCollider.points = _drawPoints.ToArray();
            _wireCollider.edgeRadius =
                ScalingManager.WireThickness * _thicknessMultiplier;
        }

        private void UpdateSmoothedLine()
        {
            _length = 0;
            GenerateDrawPoints();

            LineRenderer.positionCount = _drawPoints.Count;
            Vector2 lastLocalPos = Vector2.zero;
            for (int i = 0; i < LineRenderer.positionCount; i++)
            {
                Vector2 localPos = transform.parent.InverseTransformPoint(_drawPoints[i]);
                LineRenderer.SetPosition(i, new Vector3(localPos.x, localPos.y, -0.01f));

                if (i > 0)
                    _length += (lastLocalPos - localPos).magnitude;

                lastLocalPos = localPos;
            }
        }

        public void SetSelectionState(bool selected) { this._selected = selected; }

        private Vector2 ProcessPoint(Vector2 endPointWorldSpace)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                Vector2 a = AnchorPoints[AnchorPoints.Count - 2];
                Vector2 b = endPointWorldSpace;
                Vector2 mid = (a + b) / 2;

                bool xAxisLonger = (Mathf.Abs(a.x - b.x) > Mathf.Abs(a.y - b.y));
                if (xAxisLonger)
                {
                    return new Vector2(b.x, a.y);
                }
                else
                {
                    return new Vector2(a.x, b.y);
                }
            }
            return endPointWorldSpace;
        }

        private void GenerateDrawPoints()
        {
            _drawPoints.Clear();
            _drawPoints.Add(AnchorPoints[0]);

            for (int i = 1; i < AnchorPoints.Count - 1; i++)
            {
                Vector2 startPoint = AnchorPoints[i - 1];
                Vector2 targetPoint = AnchorPoints[i];
                Vector2 nextPoint = AnchorPoints[i + 1];

                //calculate Start Curve point
                Vector2 startToTarget = targetPoint - startPoint;
                Vector2 targetDir = startToTarget.normalized;
                float dstToTarget = startToTarget.magnitude;

                float dstToCurveStart = Mathf.Max(dstToTarget - CurveSize, dstToTarget / 2);

                Vector2 curveStartPoint = startPoint + targetDir * dstToCurveStart;


                //calulate end Curve point
                Vector2 targetToNext = nextPoint - targetPoint;
                Vector2 nextTargetDir = targetToNext.normalized;
                float dstToNext = targetToNext.magnitude;

                float dstToCurveEnd = Mathf.Min(CurveSize, dstToNext / 2);

                Vector2 curveEndPoint = targetPoint + nextTargetDir * dstToCurveEnd;

                // Bezier curve
                for (int j = 0; j < Resolution; j++)
                {
                    float t = j / (Resolution - 1f);
                    Vector2 a = Vector2.Lerp(curveStartPoint, targetPoint, t);
                    Vector2 b = Vector2.Lerp(targetPoint, curveEndPoint, t);
                    Vector2 p = Vector2.Lerp(a, b, t);

                    if ((p - _drawPoints[_drawPoints.Count - 1]).sqrMagnitude > 0.001f)
                        _drawPoints.Add(p);
                }
            }
            _drawPoints.Add(AnchorPoints[AnchorPoints.Count - 1]);
        }
    }
}