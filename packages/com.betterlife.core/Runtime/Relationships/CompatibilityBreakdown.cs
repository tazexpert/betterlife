using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// How well the character and an NPC fit, with every contribution shown.
    /// </summary>
    public sealed class CompatibilityBreakdown
    {
        /// <summary>Line key of the base score.</summary>
        public const string BaseKey = "compatibility.base";
        /// <summary>Line key of shared traits.</summary>
        public const string SharedTraitsKey = "compatibility.shared_traits";
        /// <summary>Line key of conflicting traits.</summary>
        public const string ConflictingTraitsKey = "compatibility.conflicting_traits";
        /// <summary>Line key of the age gap.</summary>
        public const string AgeGapKey = "compatibility.age_gap";
        /// <summary>Line key of looks.</summary>
        public const string LooksKey = "compatibility.looks";
        /// <summary>Line key of charisma.</summary>
        public const string CharismaKey = "compatibility.charisma";

        /// <summary>
        /// Creates the breakdown; the score is the clamped sum of the lines.
        /// </summary>
        /// <param name="lines">Contributions (localization key, points).</param>
        public CompatibilityBreakdown(IEnumerable<(string Key, float Points)> lines)
        {
            Lines = (lines ?? throw new ArgumentNullException(nameof(lines))).ToList();
            Score = StatBounds.Clamp(Lines.Sum(l => l.Points));
        }

        /// <summary>Contributions.</summary>
        public IReadOnlyList<(string Key, float Points)> Lines { get; }

        /// <summary>Score 0-100.</summary>
        public float Score { get; }
    }
}
