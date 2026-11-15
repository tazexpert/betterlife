using System;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Time;

namespace BetterLife.Core.Domain.Skills
{
    /// <summary>
    /// A character's progress in one skill.
    /// </summary>
    public sealed class SkillProgress
    {
        /// <summary>
        /// Creates progress.
        /// </summary>
        /// <param name="skillId">Skill id.</param>
        /// <param name="level">Current level, 0-100.</param>
        /// <param name="peakLevel">Highest level reached, at least <paramref name="level"/>.</param>
        /// <param name="lastPracticedOn">Week of the last practice.</param>
        public SkillProgress(string skillId, float level, float peakLevel, GameDate lastPracticedOn)
        {
            if (!StatBounds.IsInRange(level))
            {
                throw new ArgumentOutOfRangeException(nameof(level), level, "Level must be within 0-100.");
            }
            if (!StatBounds.IsInRange(peakLevel) || peakLevel < level)
            {
                throw new ArgumentOutOfRangeException(nameof(peakLevel), peakLevel, "Peak must be within 0-100 and at least the level.");
            }

            SkillId = skillId;
            Level = level;
            PeakLevel = peakLevel;
            LastPracticedOn = lastPracticedOn;
        }

        /// <summary>Skill id.</summary>
        public string SkillId { get; }

        /// <summary>Current level.</summary>
        public float Level { get; private set; }

        /// <summary>Highest level reached.</summary>
        public float PeakLevel { get; private set; }

        /// <summary>Week of the last practice.</summary>
        public GameDate LastPracticedOn { get; private set; }

        internal void Practice(float gain, GameDate date)
        {
            Level = StatBounds.Clamp(Level + gain);
            PeakLevel = Math.Max(PeakLevel, Level);
            LastPracticedOn = date;
        }

        internal void SetLevel(float level)
        {
            Level = StatBounds.Clamp(level);
        }
    }
}
