using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Family;

namespace BetterLife.Core.Domain.Characters
{
    /// <summary>
    /// An illness or injury type, loaded from content/health_conditions.json.
    /// </summary>
    public sealed class HealthConditionDefinition : IDefinition
    {
        /// <summary>
        /// Creates a definition.
        /// </summary>
        /// <param name="id">Condition id, for example "flu".</param>
        /// <param name="nameKey">Localization key of the name (also the source of its stat changes).</param>
        /// <param name="descKey">Localization key of the description.</param>
        /// <param name="weeklyStats">Stat changes per week at severity 100 (scaled by severity).</param>
        /// <param name="recoveryChancePerWeek">Weekly chance (0-1) of recovering, or null for a chronic condition.</param>
        /// <param name="annualMortality">Added yearly death probability (0-1) at severity 100.</param>
        public HealthConditionDefinition(string id, string nameKey, string descKey,
            IEnumerable<StatAmount> weeklyStats = null, float? recoveryChancePerWeek = null, float annualMortality = 0f)
        {
            if (recoveryChancePerWeek.HasValue && (!(recoveryChancePerWeek.Value >= 0f) || recoveryChancePerWeek.Value > 1f))
            {
                throw new ArgumentOutOfRangeException(nameof(recoveryChancePerWeek), recoveryChancePerWeek, "Chance must be within 0-1.");
            }
            if (!(annualMortality >= 0f) || annualMortality > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(annualMortality), annualMortality, "Mortality must be within 0-1.");
            }
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            DescKey = ContentId.RequireLocKey(descKey, nameof(descKey));
            WeeklyStats = (weeklyStats ?? Enumerable.Empty<StatAmount>()).ToList();
            RecoveryChancePerWeek = recoveryChancePerWeek;
            AnnualMortality = annualMortality;
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Localization key of the description.</summary>
        public string DescKey { get; }

        /// <summary>Stat changes per week at severity 100.</summary>
        public IReadOnlyList<StatAmount> WeeklyStats { get; }

        /// <summary>Weekly recovery chance, or null when chronic.</summary>
        public float? RecoveryChancePerWeek { get; }

        /// <summary>True when the condition never heals by itself.</summary>
        public bool IsChronic => !RecoveryChancePerWeek.HasValue;

        /// <summary>Added yearly death probability at severity 100.</summary>
        public float AnnualMortality { get; }
    }
}
