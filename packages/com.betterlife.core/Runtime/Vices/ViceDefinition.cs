using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// A habit-forming vice: temptation, short-term relief, long-term costs, addiction, withdrawal and risks.
    /// </summary>
    public sealed class ViceDefinition : IDefinition
    {
        /// <summary>
        /// Creates the vice.
        /// </summary>
        /// <param name="id">Unique id.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="minAge">Youngest age at which it appears.</param>
        /// <param name="blocksUndo">True when uses may never be undone (gambling).</param>
        /// <param name="activity">Time-budget activity counted as uses, or null.</param>
        /// <param name="baseWeekly">Base weekly temptation chance, 0-1.</param>
        /// <param name="stageMultipliers">Temptation multiplier per life stage id (1 when missing).</param>
        /// <param name="costPerUse">Real cash cost per use (bets use the stake instead).</param>
        /// <param name="reliefStress">Stress change per use before tolerance.</param>
        /// <param name="reliefHappiness">Happiness change per use before tolerance.</param>
        /// <param name="gainPerUse">Addiction gained per use at zero addiction.</param>
        /// <param name="decayPerCleanWeek">Addiction lost per week without use.</param>
        /// <param name="tolerancePerUse">Tolerance gained per use.</param>
        /// <param name="toleranceDecay">Tolerance lost per week without use.</param>
        /// <param name="withdrawalFrom">Addiction from which clean weeks bring withdrawal.</param>
        /// <param name="withdrawalStress">Weekly stress during withdrawal at full addiction.</param>
        /// <param name="withdrawalHappiness">Weekly happiness change during withdrawal at full addiction.</param>
        /// <param name="useEffects">Long-term effects of every use.</param>
        /// <param name="risks">Chances per use that something bad happens.</param>
        /// <param name="discovery">When family find out, or null.</param>
        /// <param name="gambling">Betting rules, or null.</param>
        public ViceDefinition(string id, string nameKey, int minAge, bool blocksUndo, ViceActivityLink activity, float baseWeekly,
            IReadOnlyDictionary<string, float> stageMultipliers, decimal costPerUse, float reliefStress, float reliefHappiness, float gainPerUse,
            float decayPerCleanWeek, float tolerancePerUse, float toleranceDecay, float withdrawalFrom, float withdrawalStress, float withdrawalHappiness,
            EffectSequence useEffects, IEnumerable<ViceRisk> risks, ViceDiscovery discovery, GamblingRules gambling)
        {
            if (minAge < 0 || !(baseWeekly >= 0f) || baseWeekly > 1f || costPerUse < 0m || gainPerUse < 0f || decayPerCleanWeek < 0f
                || tolerancePerUse < 0f || toleranceDecay < 0f || !StatBounds.IsInRange(withdrawalFrom))
            {
                throw new ArgumentOutOfRangeException(nameof(minAge), "Vice values are out of range.");
            }
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            MinAge = minAge;
            BlocksUndo = blocksUndo;
            Activity = activity;
            BaseWeekly = baseWeekly;
            StageMultipliers = stageMultipliers ?? new Dictionary<string, float>();
            CostPerUse = costPerUse;
            ReliefStress = reliefStress;
            ReliefHappiness = reliefHappiness;
            GainPerUse = gainPerUse;
            DecayPerCleanWeek = decayPerCleanWeek;
            TolerancePerUse = tolerancePerUse;
            ToleranceDecay = toleranceDecay;
            WithdrawalFrom = withdrawalFrom;
            WithdrawalStress = withdrawalStress;
            WithdrawalHappiness = withdrawalHappiness;
            UseEffects = useEffects ?? throw new ArgumentNullException(nameof(useEffects));
            Risks = (risks ?? Enumerable.Empty<ViceRisk>()).ToList();
            Discovery = discovery;
            Gambling = gambling;
        }

        /// <summary>Unique id.</summary>
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Youngest age at which it appears.</summary>
        public int MinAge { get; }

        /// <summary>True when uses may never be undone.</summary>
        public bool BlocksUndo { get; }

        /// <summary>Time-budget activity counted as uses, or null.</summary>
        public ViceActivityLink Activity { get; }

        /// <summary>Base weekly temptation chance.</summary>
        public float BaseWeekly { get; }

        /// <summary>Temptation multiplier per life stage id.</summary>
        public IReadOnlyDictionary<string, float> StageMultipliers { get; }

        /// <summary>Cash cost per use.</summary>
        public decimal CostPerUse { get; }

        /// <summary>Stress change per use before tolerance.</summary>
        public float ReliefStress { get; }

        /// <summary>Happiness change per use before tolerance.</summary>
        public float ReliefHappiness { get; }

        /// <summary>Addiction gained per use at zero addiction.</summary>
        public float GainPerUse { get; }

        /// <summary>Addiction lost per clean week.</summary>
        public float DecayPerCleanWeek { get; }

        /// <summary>Tolerance gained per use.</summary>
        public float TolerancePerUse { get; }

        /// <summary>Tolerance lost per clean week.</summary>
        public float ToleranceDecay { get; }

        /// <summary>Addiction from which clean weeks bring withdrawal.</summary>
        public float WithdrawalFrom { get; }

        /// <summary>Weekly withdrawal stress at full addiction.</summary>
        public float WithdrawalStress { get; }

        /// <summary>Weekly withdrawal happiness change at full addiction.</summary>
        public float WithdrawalHappiness { get; }

        /// <summary>Long-term effects of every use.</summary>
        public EffectSequence UseEffects { get; }

        /// <summary>Chances per use that something bad happens.</summary>
        public IReadOnlyList<ViceRisk> Risks { get; }

        /// <summary>When family find out, or null.</summary>
        public ViceDiscovery Discovery { get; }

        /// <summary>Betting rules, or null.</summary>
        public GamblingRules Gambling { get; }

        /// <summary>True for gambling vices.</summary>
        public bool IsGambling => Gambling != null;

        /// <summary>
        /// Returns the temptation multiplier of a life stage.
        /// </summary>
        /// <param name="stageId">Life stage id.</param>
        /// <returns>Multiplier.</returns>
        public float StageMultiplier(string stageId) => stageId != null && StageMultipliers.TryGetValue(stageId, out float value) ? value : 1f;
    }
}
