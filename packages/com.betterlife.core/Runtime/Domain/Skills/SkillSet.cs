using System;
using System.Collections.Generic;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Time;

namespace BetterLife.Core.Domain.Skills
{
    /// <summary>
    /// The skills of one character: practice with diminishing returns and decay when unused.
    /// </summary>
    public sealed class SkillSet
    {
        private readonly DefinitionCatalog<SkillDefinition> _catalog;
        private readonly Dictionary<string, SkillProgress> _progress = new Dictionary<string, SkillProgress>(StringComparer.Ordinal);

        /// <summary>
        /// Creates an empty skill set.
        /// </summary>
        /// <param name="catalog">Skill definitions.</param>
        public SkillSet(DefinitionCatalog<SkillDefinition> catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        /// <summary>Progress of every skill practiced at least once.</summary>
        public IReadOnlyCollection<SkillProgress> All => _progress.Values;

        /// <summary>
        /// Returns the current level, or 0 for a skill never practiced.
        /// </summary>
        /// <param name="skillId">Known skill id.</param>
        /// <returns>Level 0-100.</returns>
        public float GetLevel(string skillId)
        {
            _catalog.Get(skillId);
            return _progress.TryGetValue(skillId, out SkillProgress progress) ? progress.Level : StatBounds.Min;
        }

        /// <summary>
        /// Returns the progress of a skill, or null when never practiced.
        /// </summary>
        /// <param name="skillId">Skill id.</param>
        /// <returns>Progress or null.</returns>
        public SkillProgress Find(string skillId)
        {
            return skillId != null && _progress.TryGetValue(skillId, out SkillProgress progress) ? progress : null;
        }

        /// <summary>
        /// Practices a skill. Gain shrinks near mastery: baseGain × (1 − level/100)^exponent.
        /// </summary>
        /// <param name="skillId">Known skill id.</param>
        /// <param name="baseGain">Non-negative gain before falloff (already including learning-rate multipliers).</param>
        /// <param name="date">Week of practice.</param>
        /// <returns>The gain actually applied.</returns>
        public float Practice(string skillId, float baseGain, GameDate date)
        {
            if (float.IsNaN(baseGain) || float.IsInfinity(baseGain) || baseGain < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(baseGain), baseGain, "Gain must be non-negative and finite.");
            }

            SkillDefinition definition = _catalog.Get(skillId);
            if (!_progress.TryGetValue(skillId, out SkillProgress progress))
            {
                progress = new SkillProgress(skillId, StatBounds.Min, StatBounds.Min, date);
                _progress.Add(skillId, progress);
            }

            float before = progress.Level;
            float remaining = 1f - before / StatBounds.Max;
            float gain = baseGain * (float)Math.Pow(remaining, definition.GainFalloffExponent);
            progress.Practice(gain, date);
            return progress.Level - before;
        }

        /// <summary>
        /// Applies one week of decay to every skill idle longer than its grace period.
        /// Call once per in-game week. Levels never drop below peak × retention.
        /// </summary>
        /// <param name="now">Current week.</param>
        public void ApplyWeeklyDecay(GameDate now)
        {
            foreach (SkillProgress progress in _progress.Values)
            {
                SkillDecayRule rule = _catalog.Get(progress.SkillId).Decay;
                if (progress.LastPracticedOn.WeeksUntil(now) <= rule.GraceWeeks)
                {
                    continue;
                }

                float floor = progress.PeakLevel * rule.RetentionOfPeak;
                if (progress.Level > floor)
                {
                    progress.SetLevel(Math.Max(floor, progress.Level - rule.RatePerWeek));
                }
            }
        }

        /// <summary>
        /// Restores saved progress. Unknown ids are rejected.
        /// </summary>
        /// <param name="progress">Saved progress.</param>
        public void Restore(SkillProgress progress)
        {
            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }
            _catalog.Get(progress.SkillId);
            _progress[progress.SkillId] = progress;
        }
    }
}
