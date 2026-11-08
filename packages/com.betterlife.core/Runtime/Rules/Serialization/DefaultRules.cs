using BetterLife.Core.Karma;
using BetterLife.Core.Rules.Serialization.Converters;

namespace BetterLife.Core.Rules.Serialization
{
    /// <summary>
    /// Builds a registry with every built-in condition and effect type that needs no content.
    /// Content-dependent types (fortune, karma.bad, karma.merit) are registered by their loaders.
    /// </summary>
    public static class DefaultRules
    {
        /// <summary>
        /// Creates a registry with the built-in types.
        /// </summary>
        /// <returns>The registry.</returns>
        public static RuleRegistry CreateRegistry()
        {
            return new RuleRegistry()
                .Register(new AllConditionConverter())
                .Register(new AnyConditionConverter())
                .Register(new NotConditionConverter())
                .Register(new StatConditionConverter())
                .Register(new FlagConditionConverter())
                .Register(new TraitConditionConverter())
                .Register(new SkillConditionConverter())
                .Register(new AgeConditionConverter())
                .Register(new YearConditionConverter())
                .Register(new CashConditionConverter())
                .Register(new NetWorthConditionConverter())
                .Register(new TotalDebtConditionConverter())
                .Register(new ReputationConditionConverter())
                .Register(new HealthConditionConditionConverter())
                .Register(new KarmaConditionConverter())
                .Register(new LocationConditionConverter())
                .Register(new FamilyBackgroundConditionConverter())
                .Register(new LifeStageConditionConverter())
                .Register(new StatEffectConverter())
                .Register(new CashEffectConverter())
                .Register(new SetFlagEffectConverter())
                .Register(new ClearFlagEffectConverter())
                .Register(new IncrementFlagEffectConverter())
                .Register(new AddTraitEffectConverter())
                .Register(new RemoveTraitEffectConverter())
                .Register(new RevealTraitEffectConverter())
                .Register(new PracticeSkillEffectConverter())
                .Register(new ReputationEffectConverter())
                .Register(new AddHealthConditionEffectConverter())
                .Register(new RemoveHealthConditionEffectConverter())
                .Register(new IfEffectConverter())
                .Register(new GoodKarmaEffectConverter());
        }
    }
}
