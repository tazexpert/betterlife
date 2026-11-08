using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Family;

namespace BetterLife.Core.Domain.Life
{
    /// <summary>
    /// A stage of life starting at an age, loaded from content/life_stages.json.
    /// </summary>
    public sealed class LifeStageDefinition : IDefinition
    {
        /// <summary>
        /// Creates a stage.
        /// </summary>
        /// <param name="id">Stage id.</param>
        /// <param name="nameKey">Localization key of the name (also the source of birthday stat changes).</param>
        /// <param name="minAge">First age of the stage; non-negative.</param>
        /// <param name="playerControlsTime">False while the player cannot plan the week (babies).</param>
        /// <param name="extraAnnualMortality">Added yearly death probability, 0-1.</param>
        /// <param name="birthdayStats">Stat changes on every birthday in this stage.</param>
        /// <param name="conditionRisks">Yearly chances of developing conditions.</param>
        public LifeStageDefinition(string id, string nameKey, int minAge, bool playerControlsTime, float extraAnnualMortality,
            IEnumerable<StatAmount> birthdayStats, IEnumerable<ConditionRisk> conditionRisks)
        {
            if (minAge < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minAge), minAge, "Age must be non-negative.");
            }
            if (!(extraAnnualMortality >= 0f) || extraAnnualMortality > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(extraAnnualMortality), extraAnnualMortality, "Mortality must be within 0-1.");
            }
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            MinAge = minAge;
            PlayerControlsTime = playerControlsTime;
            ExtraAnnualMortality = extraAnnualMortality;
            BirthdayStats = (birthdayStats ?? Enumerable.Empty<StatAmount>()).ToList();
            ConditionRisks = (conditionRisks ?? Enumerable.Empty<ConditionRisk>()).ToList();
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>First age of the stage.</summary>
        public int MinAge { get; }

        /// <summary>False while the player cannot plan the week.</summary>
        public bool PlayerControlsTime { get; }

        /// <summary>Added yearly death probability.</summary>
        public float ExtraAnnualMortality { get; }

        /// <summary>Stat changes on every birthday.</summary>
        public IReadOnlyList<StatAmount> BirthdayStats { get; }

        /// <summary>Yearly chances of developing conditions.</summary>
        public IReadOnlyList<ConditionRisk> ConditionRisks { get; }
    }
}
