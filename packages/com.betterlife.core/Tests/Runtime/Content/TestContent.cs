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
                + "{ 'type': 'event_settings', 'path': 'event_settings.json' }"
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
                .Add("event_settings.json", EventSettings(weeklyEventChance));
        }

        public static IContentRepository Load(InMemoryContentSource source)
        {
            return DefaultContent.CreatePipeline(source).Load().RequireRepository();
        }
    }
}
