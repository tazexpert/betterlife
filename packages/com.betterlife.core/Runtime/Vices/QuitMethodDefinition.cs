using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// A way to quit a vice (cold turkey, rehab, ordination...).
    /// </summary>
    public sealed class QuitMethodDefinition : IDefinition
    {
        /// <summary>
        /// Creates the method.
        /// </summary>
        /// <param name="id">Unique id.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="viceIds">Vices it works for, or null for all.</param>
        /// <param name="costCash">Cash cost when starting.</param>
        /// <param name="durationWeeks">Weeks without a lapse needed to succeed.</param>
        /// <param name="resistBonus">Resist chance points while it lasts.</param>
        /// <param name="decayMultiplier">Multiplier of addiction loss while it lasts.</param>
        /// <param name="withdrawalMultiplier">Multiplier of withdrawal while it lasts.</param>
        /// <param name="condition">Condition to start, or null.</param>
        /// <param name="startEffects">Effects on starting.</param>
        /// <param name="successEffects">Effects on success.</param>
        public QuitMethodDefinition(string id, string nameKey, IEnumerable<string> viceIds, decimal costCash, int durationWeeks, float resistBonus,
            float decayMultiplier, float withdrawalMultiplier, ICondition condition, EffectSequence startEffects, EffectSequence successEffects)
        {
            if (costCash < 0m || durationWeeks < 1 || decayMultiplier < 0f || withdrawalMultiplier < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(costCash), "Cost, duration or multipliers out of range.");
            }
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            ViceIds = viceIds == null ? null : new HashSet<string>(viceIds, StringComparer.Ordinal);
            CostCash = costCash;
            DurationWeeks = durationWeeks;
            ResistBonus = resistBonus;
            DecayMultiplier = decayMultiplier;
            WithdrawalMultiplier = withdrawalMultiplier;
            Condition = condition;
            StartEffects = startEffects ?? throw new ArgumentNullException(nameof(startEffects));
            SuccessEffects = successEffects ?? throw new ArgumentNullException(nameof(successEffects));
        }

        /// <summary>Unique id.</summary>
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Vices it works for, or null for all.</summary>
        public IReadOnlyCollection<string> ViceIds { get; }

        /// <summary>Cash cost when starting.</summary>
        public decimal CostCash { get; }

        /// <summary>Weeks without a lapse needed to succeed.</summary>
        public int DurationWeeks { get; }

        /// <summary>Resist chance points while it lasts.</summary>
        public float ResistBonus { get; }

        /// <summary>Multiplier of addiction loss.</summary>
        public float DecayMultiplier { get; }

        /// <summary>Multiplier of withdrawal.</summary>
        public float WithdrawalMultiplier { get; }

        /// <summary>Condition to start, or null.</summary>
        public ICondition Condition { get; }

        /// <summary>Effects on starting.</summary>
        public EffectSequence StartEffects { get; }

        /// <summary>Effects on success.</summary>
        public EffectSequence SuccessEffects { get; }

        /// <summary>
        /// Returns true when the method works for a vice.
        /// </summary>
        /// <param name="viceId">Vice id.</param>
        /// <returns>True when it applies.</returns>
        public bool AppliesTo(string viceId) => ViceIds == null || ViceIds.Contains(viceId);
    }
}
