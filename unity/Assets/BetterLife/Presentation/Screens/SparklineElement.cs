using System.Collections.Generic;
using BetterLife.Core.ViewModels;
using UnityEngine;
using UnityEngine.UIElements;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Draws a sparkline from unit-box points with <see cref="Painter2D"/>. The line uses the element's text colour,
    /// so USS classes decide it.
    /// </summary>
    public sealed class SparklineElement : VisualElement
    {
        /// <summary>USS class of the element.</summary>
        public const string UssClass = "sparkline";

        private const float LineWidth = 4f;

        private IReadOnlyList<SparkPoint> _points = new SparkPoint[0];

        /// <summary>
        /// Creates an empty sparkline.
        /// </summary>
        public SparklineElement()
        {
            AddToClassList(UssClass);
            generateVisualContent += Draw;
        }

        /// <summary>
        /// Replaces the points and redraws.
        /// </summary>
        /// <param name="points">Points (fewer than two draws nothing).</param>
        public void SetPoints(IReadOnlyList<SparkPoint> points)
        {
            _points = points ?? new SparkPoint[0];
            MarkDirtyRepaint();
        }

        private void Draw(MeshGenerationContext context)
        {
            Rect box = contentRect;
            if (_points.Count < 2 || box.width <= 0f || box.height <= 0f)
            {
                return;
            }
            Painter2D painter = context.painter2D;
            painter.strokeColor = resolvedStyle.color;
            painter.lineWidth = LineWidth;
            painter.lineJoin = LineJoin.Round;
            painter.lineCap = LineCap.Round;
            painter.BeginPath();
            for (int i = 0; i < _points.Count; i++)
            {
                // Panel space grows downwards, the points grow upwards.
                var position = new Vector2(box.xMin + _points[i].X * box.width, box.yMax - _points[i].Y * box.height);
                if (i == 0)
                {
                    painter.MoveTo(position);
                }
                else
                {
                    painter.LineTo(position);
                }
            }
            painter.Stroke();
        }
    }
}
