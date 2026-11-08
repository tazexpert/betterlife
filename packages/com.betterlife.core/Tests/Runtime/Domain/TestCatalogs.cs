using System.Collections.Generic;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Flags;
using BetterLife.Core.Domain.Skills;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Domain.Traits;
using BetterLife.Core.Time;

namespace BetterLife.Core.Tests.Domain
{
    /// <summary>
    /// Builds catalogs in code that mirror the shape of content/*.json, for domain tests.
    /// </summary>
    internal static class TestCatalogs
    {
        public const string Country = "thailand";

        public static StatCatalog Stats()
        {
            var definitions = new List<StatDefinition>
            {
                Core(StatIds.Health), Core(StatIds.Mental), Core(StatIds.Intelligence), Core(StatIds.Wealth, 30f), Core(StatIds.Relationships),
                Sub(StatIds.Fitness, StatIds.Health, 50f), Sub(StatIds.Nutrition, StatIds.Health, 60f),
                Sub(StatIds.Fatigue, StatIds.Health, 20f, StatPolarity.LowerIsBetter), Sub(StatIds.DiseaseBurden, StatIds.Health, 0f, StatPolarity.LowerIsBetter),
                Sub(StatIds.Happiness, StatIds.Mental, 60f), Sub(StatIds.Stress, StatIds.Mental, 20f, StatPolarity.LowerIsBetter),
                Sub(StatIds.Confidence, StatIds.Mental, 50f), Sub(StatIds.Loneliness, StatIds.Mental, 20f, StatPolarity.LowerIsBetter),
                Sub(StatIds.Iq, StatIds.Intelligence, 50f), Sub(StatIds.Knowledge, StatIds.Intelligence, 0f),
                Sub(StatIds.CreditScore, StatIds.Wealth, 50f),
                Hidden(StatIds.Charisma), Hidden(StatIds.Looks), Hidden(StatIds.Willpower),
            };
            var formulas = new List<IStatFormula>
            {
                new WeightedStatFormula(StatIds.Health, new[]
                {
                    new StatFormulaTerm(StatIds.Fitness, 0.35f), new StatFormulaTerm(StatIds.Nutrition, 0.25f),
                    new StatFormulaTerm(StatIds.Fatigue, 0.15f, true), new StatFormulaTerm(StatIds.DiseaseBurden, 0.25f, true),
                }),
                new WeightedStatFormula(StatIds.Mental, new[]
                {
                    new StatFormulaTerm(StatIds.Happiness, 0.35f), new StatFormulaTerm(StatIds.Stress, 0.30f, true),
                    new StatFormulaTerm(StatIds.Confidence, 0.15f), new StatFormulaTerm(StatIds.Loneliness, 0.20f, true),
                }),
                new WeightedStatFormula(StatIds.Intelligence, new[]
                {
                    new StatFormulaTerm(StatIds.Iq, 0.5f), new StatFormulaTerm(StatIds.Knowledge, 0.5f),
                }),
            };
            return new StatCatalog(definitions, formulas);
        }

        public static DefinitionCatalog<TraitDefinition> Traits()
        {
            return new DefinitionCatalog<TraitDefinition>(new[]
            {
                Trait("anxious", "temperament", true, new TraitStatModifier(StatIds.Stress, ModifierDirection.Gain, 1.25f)),
                Trait("calm", "temperament", true, new TraitStatModifier(StatIds.Stress, ModifierDirection.Gain, 0.8f)),
                Trait("athletic", null, false,
                    new TraitStatModifier(StatIds.Fitness, ModifierDirection.Gain, 1.2f),
                    new TraitStatModifier(StatIds.Fitness, ModifierDirection.Loss, 0.8f)),
                Trait("sensitive", null, true, new TraitStatModifier(StatIds.Stress, ModifierDirection.Both, 2f)),
            });
        }

        public static DefinitionCatalog<SkillDefinition> Skills()
        {
            return new DefinitionCatalog<SkillDefinition>(new[]
            {
                new SkillDefinition("programming", "skill.programming.name", "technical", 1f, new SkillDecayRule(8, 5f, 0.5f)),
                new SkillDefinition("cooking", "skill.cooking.name", "life", 0f, new SkillDecayRule(0, 1f, 0f)),
            });
        }

        public static DefinitionCatalog<FlagDefinition> Flags()
        {
            return new DefinitionCatalog<FlagDefinition>(new[]
            {
                new FlagDefinition("career.fired_once", FlagKind.Bool, null, false, "eulogy.career.fired_once"),
                new FlagDefinition("relationship.recent_breakup", FlagKind.Bool, 26, false, null),
                new FlagDefinition("vice.gambling_wins", FlagKind.Counter, null, false, null),
            });
        }

        public static DefinitionCatalog<ReputationDomainDefinition> ReputationDomains()
        {
            return new DefinitionCatalog<ReputationDomainDefinition>(new[]
            {
                new ReputationDomainDefinition("business", "reputation.business.name"),
                new ReputationDomainDefinition("underworld", "reputation.underworld.name"),
            });
        }

        public static DefinitionCatalog<HealthConditionDefinition> HealthConditions()
        {
            return new DefinitionCatalog<HealthConditionDefinition>(new[]
            {
                new HealthConditionDefinition("flu", "condition.flu.name", "condition.flu.desc"),
                new HealthConditionDefinition("cold", "condition.cold.name", "condition.cold.desc"),
            });
        }

        public static CharacterCatalogs All()
        {
            return new CharacterCatalogs(Stats(), Traits(), Skills(), Flags(), ReputationDomains(), HealthConditions());
        }

        public static Character NewCharacter(CharacterCatalogs catalogs = null)
        {
            return new Character(
                new CharacterId("chr_00000000000000ab"),
                new PersonName("Somchai", "Jaidee"),
                GenderProfile.DefaultFor(BiologicalSex.Male),
                new BirthInfo(new CalendarDate(2000, 6, 15), 9, Country),
                catalogs ?? All());
        }

        private static StatDefinition Core(StatId id, float value = 50f)
        {
            return new StatDefinition(id, StatLayer.Core, StatPolarity.HigherIsBetter, value, Key(id, "name"), Key(id, "desc"));
        }

        private static StatDefinition Sub(StatId id, StatId parent, float value, StatPolarity polarity = StatPolarity.HigherIsBetter)
        {
            return new StatDefinition(id, StatLayer.Sub, polarity, value, Key(id, "name"), Key(id, "desc"), parent);
        }

        private static StatDefinition Hidden(StatId id)
        {
            return new StatDefinition(id, StatLayer.Hidden, StatPolarity.HigherIsBetter, 50f, Key(id, "name"), Key(id, "desc"));
        }

        private static string Key(StatId id, string suffix) => $"stat.{id.Value}.{suffix}";

        private static TraitDefinition Trait(string id, string group, bool hidden, params TraitStatModifier[] modifiers)
        {
            return new TraitDefinition(id, $"trait.{id}.name", $"trait.{id}.desc", "personality", group, hidden, modifiers);
        }
    }
}
