using System;
using System.Collections.Generic;

namespace BetterLife.Core.Astrology
{
    /// <summary>
    /// The yearly rise and fall of fortune: a sine wave per area with its own phase.
    /// </summary>
    public sealed class FortuneCycle
    {
        /// <summary>
        /// Creates a cycle.
        /// </summary>
        /// <param name="periodYears">Length of one full cycle in years; positive.</param>
        /// <param name="amplitude">Peak points above and below the base; non-negative.</param>
        /// <param name="areaPhaseYears">Phase shift per area in years.</param>
        public FortuneCycle(float periodYears, float amplitude, IReadOnlyDictionary<FortuneArea, float> areaPhaseYears)
        {
            if (!(periodYears > 0f) || float.IsInfinity(periodYears))
            {
                throw new ArgumentOutOfRangeException(nameof(periodYears), periodYears, "Period must be positive.");
            }
            if (!(amplitude >= 0f) || float.IsInfinity(amplitude))
            {
                throw new ArgumentOutOfRangeException(nameof(amplitude), amplitude, "Amplitude must be non-negative.");
            }
            PeriodYears = periodYears;
            Amplitude = amplitude;
            AreaPhaseYears = areaPhaseYears ?? new Dictionary<FortuneArea, float>();
        }

        /// <summary>Cycle length in years.</summary>
        public float PeriodYears { get; }

        /// <summary>Peak points above and below the base.</summary>
        public float Amplitude { get; }

        /// <summary>Phase shift per area in years.</summary>
        public IReadOnlyDictionary<FortuneArea, float> AreaPhaseYears { get; }

        /// <summary>
        /// Returns the cycle points for a year.
        /// </summary>
        /// <param name="year">Calendar year.</param>
        /// <param name="area">Area.</param>
        /// <param name="personalPhaseYears">Phase shift of the person (from the zodiac sign).</param>
        /// <returns>Points between -Amplitude and +Amplitude.</returns>
        public float PointsFor(int year, FortuneArea area, float personalPhaseYears)
        {
            float areaPhase = AreaPhaseYears.TryGetValue(area, out float phase) ? phase : 0f;
            double angle = 2.0 * Math.PI * (year + areaPhase + personalPhaseYears) / PeriodYears;
            return (float)(Amplitude * Math.Sin(angle));
        }
    }
}
