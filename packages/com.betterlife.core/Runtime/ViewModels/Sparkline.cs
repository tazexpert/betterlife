using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Sparkline and trend math for stat history samples (oldest first).
    /// </summary>
    public static class Sparkline
    {
        /// <summary>Smallest vertical span, so small wobbles are not blown up into big swings.</summary>
        public const float MinSpan = 10f;
        /// <summary>Weeks compared by the trend.</summary>
        public const int TrendWeeks = 4;
        /// <summary>Changes smaller than this count as flat.</summary>
        public const float FlatThreshold = 0.5f;

        /// <summary>
        /// Maps samples into a unit box. The vertical range fits the data but is at least <paramref name="minSpan"/> wide.
        /// </summary>
        /// <param name="samples">Samples, oldest first.</param>
        /// <param name="minSpan">Smallest vertical span.</param>
        /// <returns>Points; none for fewer than two samples.</returns>
        public static IReadOnlyList<SparkPoint> Points(IReadOnlyList<float> samples, float minSpan = MinSpan)
        {
            if (samples == null || samples.Count < 2)
            {
                return new SparkPoint[0];
            }
            float min = samples.Min();
            float max = samples.Max();
            float span = Math.Max(max - min, minSpan);
            float bottom = (min + max - span) / 2f;
            var points = new SparkPoint[samples.Count];
            for (int i = 0; i < samples.Count; i++)
            {
                points[i] = new SparkPoint(i / (float)(samples.Count - 1), (samples[i] - bottom) / span);
            }
            return points;
        }

        /// <summary>
        /// Change between the newest sample and the one <paramref name="weeks"/> earlier (or the oldest available).
        /// </summary>
        /// <param name="samples">Samples, oldest first.</param>
        /// <param name="weeks">Window in weeks.</param>
        /// <returns>The change; 0 without two samples.</returns>
        public static float Delta(IReadOnlyList<float> samples, int weeks = TrendWeeks)
        {
            if (samples == null || samples.Count < 2)
            {
                return 0f;
            }
            int from = Math.Max(0, samples.Count - 1 - weeks);
            return samples[samples.Count - 1] - samples[from];
        }

        /// <summary>
        /// Direction of a change.
        /// </summary>
        /// <param name="delta">Change.</param>
        /// <returns>The direction.</returns>
        public static TrendDirection Direction(float delta)
        {
            if (Math.Abs(delta) < FlatThreshold)
            {
                return TrendDirection.Flat;
            }
            return delta > 0f ? TrendDirection.Up : TrendDirection.Down;
        }
    }
}
