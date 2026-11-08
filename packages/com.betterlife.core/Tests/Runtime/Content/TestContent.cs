using BetterLife.Core.Content;
using BetterLife.Core.Tests.Astrology;

namespace BetterLife.Core.Tests.Content
{
    /// <summary>
    /// Minimal valid content for every required type, as in-memory files. Tests add or replace files on top.
    /// </summary>
    internal static class TestContent
    {
        public const string Stats = @"{ 'stats': [
            { 'id': 'health', 'layer': 'core', 'polarity': 'higherIsBetter', 'default': 50, 'nameKey': 'stat.health.name', 'descKey': 'stat.health.desc' },
            { 'id': 'health.fitness', 'layer': 'sub', 'parent': 'health', 'polarity': 'higherIsBetter', 'default': 40,
              'nameKey': 'stat.health.fitness.name', 'descKey': 'stat.health.fitness.desc', 'decay': { 'target': 30, 'ratePerWeek': 0.5 } },
            { 'id': 'mental', 'layer': 'core', 'polarity': 'higherIsBetter', 'default': 50, 'nameKey': 'stat.mental.name', 'descKey': 'stat.mental.desc' },
            { 'id': 'mental.stress', 'layer': 'hidden', 'polarity': 'lowerIsBetter', 'default': 20, 'nameKey': 'stat.stress.name', 'descKey': 'stat.stress.desc' } ],
          'formulas': [ { 'target': 'health', 'type': 'weighted', 'terms': [ { 'stat': 'health.fitness', 'weight': 1 } ] } ] }";

        public const string Traits = @"{ 'traits': [ { 'id': 'anxious', 'nameKey': 'trait.anxious.name', 'descKey': 'trait.anxious.desc',
            'category': 'personality', 'exclusiveGroup': 'temperament', 'startsHidden': true,
            'statModifiers': [ { 'stat': 'mental.stress', 'direction': 'gain', 'multiplier': 1.25 } ] } ] }";

        public const string Skills = @"{ 'skills': [ { 'id': 'cooking', 'nameKey': 'skill.cooking.name', 'category': 'life',
            'gainFalloffExponent': 1.2, 'decay': { 'graceWeeks': 12, 'ratePerWeek': 0.2, 'retentionOfPeak': 0.6 } } ] }";

        public const string Flags = @"{ 'flags': [
            { 'id': 'career.fired_once', 'kind': 'bool', 'defaultExpiryWeeks': null, 'inheritable': false, 'eulogyKey': 'eulogy.fired' },
            { 'id': 'vice.wins', 'kind': 'counter', 'defaultExpiryWeeks': 52, 'inheritable': true, 'eulogyKey': null } ] }";

        public const string Domains = "{ 'domains': [ { 'id': 'business', 'nameKey': 'reputation.business.name' } ] }";

        public const string Conditions = "{ 'conditions': [ { 'id': 'flu', 'nameKey': 'condition.flu.name', 'descKey': 'condition.flu.desc' } ] }";

        public const string Activities = @"{ 'activities': [
            { 'id': 'free_time', 'nameKey': 'activity.free_time.name', 'descKey': 'activity.free_time.desc', 'category': 'rest',
              'minHours': 0, 'maxHours': 168, 'costPerHour': 0, 'stats': [], 'skills': [] },
            { 'id': 'gym', 'nameKey': 'activity.gym.name', 'descKey': 'activity.gym.desc', 'category': 'health',
              'minHours': 0, 'maxHours': 20, 'costPerHour': 15.5, 'condition': { 'type': 'age', 'op': '>=', 'value': 12 },
              'stats': [ { 'stat': 'health.fitness', 'perHour': 0.3, 'maxHours': 10 } ], 'skills': [ { 'skill': 'cooking', 'gainPerHour': 0.1 } ] } ] }";

        public const string TimeSettings = @"{ 'defaultActivity': 'free_time',
            'learning': { 'multiplierAtZeroMental': 0.5, 'multiplierAtFullMental': 1.5 },
            'defaultBudget': [ { 'activity': 'gym', 'hours': 5 } ] }";

        public const string Countries = @"{ 'countries': [
            { 'id': 'thailand', 'nameKey': 'country.thailand.name', 'weight': 9,
              'modifiers': { 'costOfLiving': 1, 'education': 60, 'healthcare': 65, 'taxRate': 0.1 },
              'cities': [ { 'id': 'thailand.bangkok', 'nameKey': 'city.bangkok.name', 'weight': 3, 'modifiers': { 'costOfLiving': 1.35 } },
                          { 'id': 'thailand.rural', 'nameKey': 'city.rural.name', 'weight': 1 } ] },
            { 'id': 'japan', 'nameKey': 'country.japan.name', 'weight': 1,
              'modifiers': { 'costOfLiving': 1.8, 'education': 80, 'healthcare': 88, 'taxRate': 0.2 },
              'cities': [ { 'id': 'japan.tokyo', 'nameKey': 'city.tokyo.name', 'weight': 1 } ] } ] }";

        public const string FamilyBackgrounds = @"{ 'backgrounds': [
            { 'id': 'poor', 'nameKey': 'family.poor.name', 'weight': 3, 'householdIncomeLevel': 1, 'statModifiers': [ { 'stat': 'health.fitness', 'amount': -5 } ] },
            { 'id': 'wealthy', 'nameKey': 'family.wealthy.name', 'weight': 1, 'householdIncomeLevel': 5, 'statModifiers': [ { 'stat': 'health.fitness', 'amount': 5 } ] } ] }";

        public const string Personalities = @"{ 'personalities': [
            { 'id': 'strict', 'nameKey': 'parent.strict.name', 'weight': 1, 'statModifiers': [ { 'stat': 'mental.stress', 'amount': 5 } ],
              'traitChances': [ { 'trait': 'anxious', 'chance': 0.5 } ] },
            { 'id': 'warm', 'nameKey': 'parent.warm.name', 'weight': 1, 'statModifiers': [ { 'stat': 'mental.stress', 'amount': -5 } ], 'traitChances': [] } ] }";

        public const string Genetics = @"{ 'population': { 'mean': 50, 'sd': 15 },
            'stats': [ { 'stat': 'health.fitness', 'heritability': 0.5, 'noise': 5 } ],
            'traits': [ { 'trait': 'anxious', 'prevalence': 0.2, 'heritability': 0.5 } ] }";

        public const string Names = @"{ 'countries': [
            { 'country': 'thailand', 'given': { 'female': [ { 'native': 'มาลี', 'latin': 'Malee' }, { 'native': 'สุดา', 'latin': 'Suda' } ],
                                                'male': [ { 'native': 'สมชาย', 'latin': 'Somchai' }, { 'native': 'วิชัย', 'latin': 'Wichai' } ] },
              'family': [ { 'native': 'ใจดี', 'latin': 'Jaidee' }, { 'native': 'ศรีสุข', 'latin': 'Srisuk' } ] },
            { 'country': 'japan', 'given': { 'female': [ { 'native': 'さくら', 'latin': 'Sakura' } ], 'male': [ { 'native': 'はると', 'latin': 'Haruto' } ] },
              'family': [ { 'native': '佐藤', 'latin': 'Sato' } ] } ] }";

        public const string CreationSettings = "{ 'birthYear': { 'min': 1980, 'max': 2020 }, 'parentAgeAtBirth': { 'min': 20, 'max': 40 } }";

        public const string LifeStages = @"{ 'stages': [
            { 'id': 'baby', 'nameKey': 'stage.baby.name', 'minAge': 0, 'playerControlsTime': false, 'extraAnnualMortality': 0, 'birthdayStats': [], 'conditionRisks': [] },
            { 'id': 'child', 'nameKey': 'stage.child.name', 'minAge': 6, 'playerControlsTime': true, 'extraAnnualMortality': 0, 'birthdayStats': [], 'conditionRisks': [] },
            { 'id': 'adult', 'nameKey': 'stage.adult.name', 'minAge': 18, 'playerControlsTime': true, 'extraAnnualMortality': 0,
              'birthdayStats': [ { 'stat': 'health.fitness', 'amount': -1 } ], 'conditionRisks': [ { 'condition': 'flu', 'annualChance': 0.1 } ] } ] }";

        public const string Mortality = @"{ 'gompertz': { 'referenceAge': 30, 'annualAtReference': 0.0008, 'growthPerYear': 0.085 }, 'maxAge': 110,
            'health': { 'multiplierAtZero': 8, 'multiplierAtFull': 0.6 }, 'healthcare': { 'multiplierAtZero': 1.4, 'multiplierAtFull': 0.85 },
            'maxWeeklyProbability': 0.5 }";

        public const string DeathCauses = @"{ 'causes': [
            { 'id': 'natural', 'nameKey': 'death.natural.name', 'weight': { 'base': 1 } },
            { 'id': 'old_age', 'nameKey': 'death.old_age.name', 'condition': { 'type': 'age', 'op': '>=', 'value': 60 },
              'weight': { 'base': 1, 'modifiers': [ { 'condition': { 'type': 'age', 'op': '>=', 'value': 80 }, 'multiplier': 50 } ] } },
            { 'id': 'accident', 'nameKey': 'death.accident.name', 'weight': { 'base': 0 } } ] }";

        public const string EventSettingsTemplate = "{ 'weeklyEventChance': CHANCE, 'responseWindowWeeks': 4, "
            + "'antiFrustration': { 'streakThreshold': 2, 'windowWeeks': 12, 'negativeWeightMultiplier': 0.4 } }";

        public static string EventSettings(float weeklyChance = 0.15f)
        {
            return EventSettingsTemplate.Replace("CHANCE", weeklyChance.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        public static InMemoryContentSource Source(string extraFiles = null, float weeklyEventChance = 0.15f)
        {
            string files = "{ 'type': 'stats', 'path': 'stats.json' }, { 'type': 'traits', 'path': 'traits.json' }, "
                + "{ 'type': 'skills', 'path': 'skills.json' }, { 'type': 'flags', 'path': 'flags.json' }, "
                + "{ 'type': 'reputation_domains', 'path': 'reputation_domains.json' }, { 'type': 'health_conditions', 'path': 'health_conditions.json' }, "
                + "{ 'type': 'activities', 'path': 'activities.json' }, { 'type': 'time_settings', 'path': 'time_settings.json' }, "
                + "{ 'type': 'astrology', 'path': 'astrology.json' }, { 'type': 'luck_settings', 'path': 'luck.json' }, "
                + "{ 'type': 'event_settings', 'path': 'event_settings.json' }, "
                + "{ 'type': 'countries', 'path': 'countries.json' }, { 'type': 'family_backgrounds', 'path': 'family_backgrounds.json' }, "
                + "{ 'type': 'parent_personalities', 'path': 'parent_personalities.json' }, { 'type': 'genetics', 'path': 'genetics.json' }, "
                + "{ 'type': 'names', 'path': 'names.json' }, { 'type': 'creation_settings', 'path': 'creation_settings.json' }, "
                + "{ 'type': 'life_stages', 'path': 'life_stages.json' }, { 'type': 'mortality', 'path': 'mortality.json' }, "
                + "{ 'type': 'death_causes', 'path': 'death_causes.json' }"
                + (extraFiles == null ? string.Empty : ", " + extraFiles);
            return new InMemoryContentSource()
                .Add(ContentPaths.ManifestFile, "{ '$schema': '../docs/schemas/manifest.schema.json', 'contentVersion': 3, 'files': [ " + files + " ] }")
                .Add("stats.json", Stats)
                .Add("traits.json", Traits)
                .Add("skills.json", Skills)
                .Add("flags.json", Flags)
                .Add("reputation_domains.json", Domains)
                .Add("health_conditions.json", Conditions)
                .Add("activities.json", Activities)
                .Add("time_settings.json", TimeSettings)
                .Add("astrology.json", TestAstrology.Json())
                .Add("luck.json", TestAstrology.LuckJson)
                .Add("event_settings.json", EventSettings(weeklyEventChance))
                .Add("countries.json", Countries)
                .Add("family_backgrounds.json", FamilyBackgrounds)
                .Add("parent_personalities.json", Personalities)
                .Add("genetics.json", Genetics)
                .Add("names.json", Names)
                .Add("creation_settings.json", CreationSettings)
                .Add("life_stages.json", LifeStages)
                .Add("mortality.json", Mortality)
                .Add("death_causes.json", DeathCauses);
        }

        public static IContentRepository Load(InMemoryContentSource source)
        {
            return DefaultContent.CreatePipeline(source).Load().RequireRepository();
        }
    }
}
