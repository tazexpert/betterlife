using System;
using System.Collections.Generic;

namespace BetterLife.Core.Astrology
{
    /// <summary>
    /// Fortune points added per area (missing areas add nothing).
    /// </summary>
    public sealed class FortuneModifiers
    {
        /// <summary>Modifiers that change nothing.</summary>
        public static readonly FortuneModifiers None = new FortuneModifiers(new Dictionary<FortuneArea, float>());

        private readonly Dictionary<FortuneArea, float> _points;

        /// <summary>
        /// Creates modifiers.
        /// </summary>
        /// <param name="points">Finite points per area.</param>
        public FortuneModifiers(IReadOnlyDictionary<FortuneArea, float> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }
            _points = new Dictionary<FortuneArea, float>();
            foreach (KeyValuePair<FortuneArea, float> pair in points)
            {
                if (float.IsNaN(pair.Value) || float.IsInfinity(pair.Value))
                {
                    throw new ArgumentException("Modifier points must be finite.", nameof(points));
                }
                _points.Add(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Returns the points for an area.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <returns>Points, 0 when unspecified.</returns>
        public float For(FortuneArea area) => _points.TryGetValue(area, out float value) ? value : 0f;
    }
}
