using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LSDE.Demo
{
    /// <summary>
    /// Draws an organic, cloud-like speech bubble procedurally — jelly wobble style.
    /// Ported from the PixiJS implementation in bubble-text.ts.
    /// Uses bezier-interpolated control points along an ellipse with random bulge
    /// variations for the organic "manga/BD" look. Animated wobble in Update.
    /// Resolution-independent — never pixelates.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class ProceduralSpeechBubble : MaskableGraphic
    {
        [Header("Bubble Shape")]
        [SerializeField]
        [Tooltip("Number of control points around the ellipse. More = rounder shape.")]
        private int _controlPointCount = 14;

        [SerializeField]
        [Tooltip("Random bulge variation amplitude for the organic cloud look.")]
        private float _bulgeVariation = 8f;

        [Header("Outline")]
        [SerializeField]
        [Tooltip("Thickness of the black outline in pixels.")]
        private float _outlineThickness = 4f;

        [SerializeField]
        [Tooltip("Color of the outline.")]
        private Color _outlineColor = new Color(0.13f, 0.13f, 0.13f, 1f);

        [Header("Tail")]
        [SerializeField]
        [Tooltip("Height of the tail extending below the bubble body.")]
        private float _tailHeight = 50.6f;

        [SerializeField]
        [Tooltip("Horizontal offset of the tail from center.")]
        private float _tailHorizontalOffset = 114.8f;

        [Header("Animation")]
        [SerializeField]
        [Tooltip("Amplitude of the jelly wobble animation.")]
        private float _wobbleAmplitude = 6f;

        [SerializeField]
        [Tooltip("Speed of the wobble animation.")]
        private float _wobbleSpeed = 1.2f;

        [SerializeField]
        [Tooltip("Enable wobble animation.")]
        private bool _enableWobble = true;

        private float _elapsedTime;
        private List<ControlPointData> _controlPoints;

        /// <summary>
        /// Use Unity's default UI material so the graphic renders properly.
        /// </summary>
        public override Material materialForRendering =>
            defaultMaterial != null ? defaultMaterial : Canvas.GetDefaultCanvasMaterial();

        protected override void OnEnable()
        {
            base.OnEnable();
            _controlPoints = null;
        }

        private void Update()
        {
            if (!_enableWobble)
            {
                return;
            }

            _elapsedTime += Time.deltaTime;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            var rectTransform = GetComponent<RectTransform>();
            var rect = rectTransform.rect;

            // Body occupies the rect minus the tail area at the bottom
            float bodyBottom = rect.yMin + _tailHeight;
            float bodyCenterX = (rect.xMin + rect.xMax) * 0.5f;
            float bodyCenterY = (bodyBottom + rect.yMax) * 0.5f;
            float bodyRadiusX = (rect.xMax - rect.xMin) * 0.5f + 6f;
            float bodyRadiusY = (rect.yMax - bodyBottom) * 0.5f + 6f;

            // Generate control points once (with deterministic bulge)
            if (_controlPoints == null || _controlPoints.Count != _controlPointCount)
            {
                _controlPoints = GenerateControlPoints(
                    bodyCenterX,
                    bodyCenterY,
                    bodyRadiusX,
                    bodyRadiusY
                );
            }
            else
            {
                UpdateControlPointPositions(bodyCenterX, bodyCenterY, bodyRadiusX, bodyRadiusY);
            }

            // Compute animated positions with wobble
            var animatedPositions = ComputeAnimatedPositions(_elapsedTime);

            // Generate smooth body curve points
            var bodyCurvePoints = GenerateCurvePoints(animatedPositions);

            // Build the FULL contour: body ellipse with tail inserted seamlessly
            float tailCenterX = bodyCenterX + _tailHorizontalOffset;
            float tailTipX = tailCenterX + 22f;
            float tailTipY = rect.yMin;

            var fullContour = BuildContourWithTail(
                bodyCurvePoints,
                tailCenterX,
                bodyBottom,
                tailTipX,
                tailTipY
            );

            // --- Draw filled shape as fan triangles from center ---
            int centerVertexIndex = AddVertex(
                vertexHelper,
                new Vector2(bodyCenterX, bodyCenterY),
                color
            );

            int firstContourVertexIndex = vertexHelper.currentVertCount;
            foreach (var contourPoint in fullContour)
            {
                AddVertex(vertexHelper, contourPoint, color);
            }

            int contourVertexCount = fullContour.Count;
            for (int pointIndex = 0; pointIndex < contourVertexCount; pointIndex++)
            {
                int currentVertex = firstContourVertexIndex + pointIndex;
                int nextVertex = firstContourVertexIndex + (pointIndex + 1) % contourVertexCount;
                vertexHelper.AddTriangle(centerVertexIndex, currentVertex, nextVertex);
            }

            // --- Draw outline along the unified contour ---
            DrawOutlineAlongPoints(vertexHelper, fullContour);
        }

        /// <summary>
        /// Build the full contour by replacing the bottom section of the ellipse
        /// with the tail bezier curves. Uses DETERMINISTIC indices based on the
        /// control point angles (not animated positions), so the tail never flickers.
        ///
        /// With 14 control points starting at angle -π/2 (bottom), and 4 curve segments
        /// per control point = 56 total curve points:
        /// - Index 0 = bottom center (control point 0, angle -90°)
        /// - The bottom section spans roughly indices 53-56 and 0-3
        /// The tail replaces this section.
        /// </summary>
        private List<Vector2> BuildContourWithTail(
            List<Vector2> bodyCurvePoints,
            float tailCenterX,
            float bodyBottom,
            float tailTipX,
            float tailTipY,
            int tailSegmentCount = 8
        )
        {
            int totalCurvePoints = bodyCurvePoints.Count;
            int segmentsPerControlPoint = 4;

            // The tail replaces a NARROW section at the bottom of the ellipse.
            // Control point 0 is at angle -π/2 (bottom). Each control point
            // generates segmentsPerControlPoint (4) curve points.
            // We cut only 1 control point's worth of arc on each side of the bottom.
            int tailRightBaseIndex = totalCurvePoints - segmentsPerControlPoint; // = 52
            int tailLeftBaseIndex = segmentsPerControlPoint; // = 4

            var result = new List<Vector2>();

            // Body arc: from tailLeftBaseIndex to tailRightBaseIndex
            // (the entire top arc, from bottom-left up and around to bottom-right)
            for (int pointIndex = tailLeftBaseIndex; pointIndex <= tailRightBaseIndex; pointIndex++)
            {
                result.Add(bodyCurvePoints[pointIndex % totalCurvePoints]);
            }

            // Tail right base = where the body arc ends (bottom-right)
            Vector2 tailBaseRight = bodyCurvePoints[tailRightBaseIndex % totalCurvePoints];
            Vector2 tailTip = new Vector2(tailTipX, tailTipY);

            // Tail right side: bezier from tailBaseRight down to tip
            // Control point stays in vertical axis — no horizontal offset to avoid crossing
            float tailMidY = (bodyBottom + tailTipY) * 0.5f;
            Vector2 tailRightControl = new Vector2(tailTip.x, tailMidY);

            for (int segmentIndex = 1; segmentIndex <= tailSegmentCount; segmentIndex++)
            {
                float interpolation = (float)segmentIndex / tailSegmentCount;
                result.Add(
                    EvaluateQuadraticBezier(tailBaseRight, tailRightControl, tailTip, interpolation)
                );
            }

            // Tail left side: bezier from tip back up to tailBaseLeft
            Vector2 tailBaseLeft = bodyCurvePoints[tailLeftBaseIndex];
            Vector2 tailLeftControl = new Vector2(tailBaseLeft.x, tailMidY);

            // Go up to but NOT including the last point (tailBaseLeft = result[0])
            // to avoid a duplicate vertex at the seam. But we must get CLOSE enough
            // that the outline closes cleanly.
            for (int segmentIndex = 1; segmentIndex <= tailSegmentCount; segmentIndex++)
            {
                float interpolation = (float)segmentIndex / tailSegmentCount;
                // Stop just before 1.0 to avoid exact duplicate with result[0]
                if (segmentIndex == tailSegmentCount)
                {
                    interpolation = 0.98f;
                }
                result.Add(
                    EvaluateQuadraticBezier(tailTip, tailLeftControl, tailBaseLeft, interpolation)
                );
            }

            return result;
        }

        /// <summary>
        /// Generate control points along an ellipse with deterministic bulge variations.
        /// Ported from bubble-text.ts generateControlPoints().
        /// </summary>
        private List<ControlPointData> GenerateControlPoints(
            float centerX,
            float centerY,
            float radiusX,
            float radiusY
        )
        {
            var controlPoints = new List<ControlPointData>();

            for (int pointIndex = 0; pointIndex < _controlPointCount; pointIndex++)
            {
                float angle =
                    ((float)pointIndex / _controlPointCount) * Mathf.PI * 2f - Mathf.PI / 2f;

                // Deterministic bulge for organic look (same math as TS)
                float bulgeOffset =
                    Mathf.Sin(pointIndex * 2.3f + 0.7f) * _bulgeVariation
                    + Mathf.Cos(pointIndex * 3.1f) * _bulgeVariation * 0.5f;

                float baseX = centerX + Mathf.Cos(angle) * (radiusX + bulgeOffset);
                float baseY = centerY + Mathf.Sin(angle) * (radiusY + bulgeOffset * 0.7f);

                // Deterministic phase and amplitude for wobble animation
                float phase = pointIndex * 1.7f + pointIndex * pointIndex * 0.3f;
                float amplitudeMultiplier = 0.6f + ((pointIndex * 7 + 3) % 5) / 5f;

                controlPoints.Add(
                    new ControlPointData
                    {
                        BaseX = baseX,
                        BaseY = baseY,
                        Phase = phase,
                        AmplitudeMultiplier = amplitudeMultiplier,
                    }
                );
            }

            return controlPoints;
        }

        private void UpdateControlPointPositions(
            float centerX,
            float centerY,
            float radiusX,
            float radiusY
        )
        {
            for (int pointIndex = 0; pointIndex < _controlPointCount; pointIndex++)
            {
                float angle =
                    ((float)pointIndex / _controlPointCount) * Mathf.PI * 2f - Mathf.PI / 2f;

                float bulgeOffset =
                    Mathf.Sin(pointIndex * 2.3f + 0.7f) * _bulgeVariation
                    + Mathf.Cos(pointIndex * 3.1f) * _bulgeVariation * 0.5f;

                var point = _controlPoints[pointIndex];
                point.BaseX = centerX + Mathf.Cos(angle) * (radiusX + bulgeOffset);
                point.BaseY = centerY + Mathf.Sin(angle) * (radiusY + bulgeOffset * 0.7f);
                _controlPoints[pointIndex] = point;
            }
        }

        /// <summary>
        /// Compute animated positions with wobble. Ported from bubble-text.ts computeAnimatedPositions().
        /// </summary>
        private List<Vector2> ComputeAnimatedPositions(float time)
        {
            var positions = new List<Vector2>();

            foreach (var controlPoint in _controlPoints)
            {
                float wobbleX = _enableWobble
                    ? Mathf.Sin(time * _wobbleSpeed + controlPoint.Phase)
                        * _wobbleAmplitude
                        * controlPoint.AmplitudeMultiplier
                    : 0f;

                float wobbleY = _enableWobble
                    ? Mathf.Cos(time * _wobbleSpeed * 0.7f + controlPoint.Phase + 1.3f)
                        * _wobbleAmplitude
                        * controlPoint.AmplitudeMultiplier
                    : 0f;

                positions.Add(
                    new Vector2(controlPoint.BaseX + wobbleX, controlPoint.BaseY + wobbleY)
                );
            }

            return positions;
        }

        /// <summary>
        /// Generate smooth curve between control points using quadratic bezier midpoints.
        /// Ported from bubble-text.ts drawCloudBody() — uses midpoints between consecutive
        /// control points as curve vertices, with the control points as bezier handles.
        /// </summary>
        private List<Vector2> GenerateCurvePoints(List<Vector2> controlPositions)
        {
            var curvePoints = new List<Vector2>();
            int pointCount = controlPositions.Count;
            int segmentsPerCurve = 4;

            for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
            {
                var currentPoint = controlPositions[pointIndex];
                var nextPoint = controlPositions[(pointIndex + 1) % pointCount];
                var afterNextPoint = controlPositions[(pointIndex + 2) % pointCount];

                // Quadratic bezier: from midpoint(current, next) through next to midpoint(next, afterNext)
                Vector2 startMidpoint = (currentPoint + nextPoint) * 0.5f;
                Vector2 endMidpoint = (nextPoint + afterNextPoint) * 0.5f;

                for (int segmentIndex = 0; segmentIndex < segmentsPerCurve; segmentIndex++)
                {
                    float interpolation = (float)segmentIndex / segmentsPerCurve;
                    Vector2 curvePosition = EvaluateQuadraticBezier(
                        startMidpoint,
                        nextPoint,
                        endMidpoint,
                        interpolation
                    );
                    curvePoints.Add(curvePosition);
                }
            }

            return curvePoints;
        }

        /// <summary>
        /// Draw an outline as a triangle strip along a closed contour.
        /// </summary>
        private void DrawOutlineAlongPoints(VertexHelper vertexHelper, List<Vector2> contourPoints)
        {
            int pointCount = contourPoints.Count;
            int outlineStartVertex = vertexHelper.currentVertCount;

            for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
            {
                var currentPoint = contourPoints[pointIndex];
                var nextPoint = contourPoints[(pointIndex + 1) % pointCount];
                var previousPoint = contourPoints[(pointIndex - 1 + pointCount) % pointCount];

                Vector2 toNext = (nextPoint - currentPoint).normalized;
                Vector2 fromPrevious = (currentPoint - previousPoint).normalized;
                Vector2 averageDirection = ((toNext + fromPrevious) * 0.5f).normalized;
                Vector2 outwardNormal = new Vector2(averageDirection.y, -averageDirection.x);

                AddVertex(vertexHelper, currentPoint, _outlineColor);
                AddVertex(
                    vertexHelper,
                    currentPoint + outwardNormal * _outlineThickness,
                    _outlineColor
                );
            }

            int outlineVertexCount = vertexHelper.currentVertCount - outlineStartVertex;
            for (int pairIndex = 0; pairIndex < outlineVertexCount - 2; pairIndex += 2)
            {
                int innerCurrent = outlineStartVertex + pairIndex;
                int outerCurrent = outlineStartVertex + pairIndex + 1;
                int innerNext = outlineStartVertex + pairIndex + 2;
                int outerNext = outlineStartVertex + pairIndex + 3;

                if (outerNext < vertexHelper.currentVertCount)
                {
                    vertexHelper.AddTriangle(innerCurrent, outerCurrent, innerNext);
                    vertexHelper.AddTriangle(outerCurrent, outerNext, innerNext);
                }
            }

            // Close the outline loop
            if (outlineVertexCount >= 4)
            {
                int lastInner = outlineStartVertex + outlineVertexCount - 2;
                int lastOuter = outlineStartVertex + outlineVertexCount - 1;
                int firstInner = outlineStartVertex;
                int firstOuter = outlineStartVertex + 1;

                vertexHelper.AddTriangle(lastInner, lastOuter, firstInner);
                vertexHelper.AddTriangle(lastOuter, firstOuter, firstInner);
            }
        }

        /// <summary>
        /// Evaluate a quadratic bezier curve at parameter t.
        /// B(t) = (1-t)² * P0 + 2(1-t)t * P1 + t² * P2
        /// </summary>
        private static Vector2 EvaluateQuadraticBezier(
            Vector2 startPoint,
            Vector2 controlPoint,
            Vector2 endPoint,
            float interpolation
        )
        {
            float oneMinusT = 1f - interpolation;
            return oneMinusT * oneMinusT * startPoint
                + 2f * oneMinusT * interpolation * controlPoint
                + interpolation * interpolation * endPoint;
        }

        private static int AddVertex(VertexHelper vertexHelper, Vector2 position, Color vertexColor)
        {
            int vertexIndex = vertexHelper.currentVertCount;
            vertexHelper.AddVert(
                new Vector3(position.x, position.y, 0f),
                vertexColor,
                Vector4.zero
            );
            return vertexIndex;
        }

        private struct ControlPointData
        {
            public float BaseX;
            public float BaseY;
            public float Phase;
            public float AmplitudeMultiplier;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _controlPoints = null;

            SetVerticesDirty();
        }
#endif
    }
}
