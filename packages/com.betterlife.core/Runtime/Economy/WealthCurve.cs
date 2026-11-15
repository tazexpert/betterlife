using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Maps net worth, measured in months of reference living costs, to the Wealth score by linear interpolation.
    /// </summary>
    public sealed class WealthCurve
    {
        private readonly List<(float Months, float Score)> _points;

        /// <summary>
        /// Creates a curve.
        /// </summary>
        /// <param name="points">At least two points with increasing months and scores within 0-100.</param>
        public WealthCurve(IEnumerable<(float Months, float Score)> points)
        {
            _points = (points ?? throw new ArgumentNullException(nameof(points))).OrderBy(p => p.Months).ToList();
            if (_points.Count < 2 || _points.Select(p => p.Months).Distinct().Count() != _points.Count || _points.Any(p => !StatBounds.IsInRange(p.Score)))
            {
                throw new ArgumentException("A curve needs at least two points with distinct months and scores within 0-100.", nameof(points));
            }
        }

        /// <summary>Points ordered by months.</summary>
        public IReadOnlyList<(float Months, float Score)> Points => _points;

        /// <summary>
        /// Returns the Wealth score for net worth covering a number of months of living costs (clamped at the ends).
        /// </summary>
        /// <param name="months">Months covered; negative when in debt.</param>
        /// <returns>Score 0-100.</returns>
        public float ScoreFor(float months)
        {
            if (months <= _points[0].Months)
            {
                return _points[0].Score;
            }
            for (int i = 1; i < _points.Count; i++)
            {
                if (months <= _points[i].Months)
                {
                    (float m0, float s0) = _points[i - 1];
                    (float m1, float s1) = _points[i];
                    return s0 + (s1 - s0) * (months - m0) / (m1 - m0);
                }
            }
            return _points[_points.Count - 1].Score;
        }
    }
}
