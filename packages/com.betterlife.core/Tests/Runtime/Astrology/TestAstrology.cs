using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Astrology;
using BetterLife.Core.Karma;
using BetterLife.Core.Luck;
using BetterLife.Core.Time;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Tests.Astrology
{
    /// <summary>
    /// Small, predictable astrology and luck content for tests, as objects and as JSON.
    /// Only aries (+10 career), monday (+5 love), wednesday night (+7 finance) and water (+3 study) have modifiers.
    /// </summary>
    internal static class TestAstrology
    {
        public static readonly (string Id, int Month, int Day)[] Signs =
        {
            ("aries", 4, 13), ("taurus", 5, 14), ("gemini", 6, 14), ("cancer", 7, 15), ("leo", 8, 17), ("virgo", 9, 17),
            ("libra", 10, 17), ("scorpio", 11, 16), ("sagittarius", 12, 16), ("capricorn", 1, 14), ("aquarius", 2, 13), ("pisces", 3, 14),
        };

        public static readonly (string Id, string Element)[] Animals =
        {
            ("rat", "water"), ("ox", "earth"), ("tiger", "wood"), ("rabbit", "wood"), ("dragon", "earth"), ("snake", "fire"),
            ("horse", "fire"), ("goat", "earth"), ("monkey", "metal"), ("rooster", "metal"), ("dog", "earth"), ("pig", "water"),
        };

        private static readonly string[] Elements = { "earth", "water", "fire", "wood", "metal" };

        private static readonly (string Name, ThaiDayOfWeek Day)[] Days =
        {
            ("sunday", ThaiDayOfWeek.Sunday), ("monday", ThaiDayOfWeek.Monday), ("tuesday", ThaiDayOfWeek.Tuesday),
            ("wednesday", ThaiDayOfWeek.Wednesday), ("wednesday_night", ThaiDayOfWeek.WednesdayNight), ("thursday", ThaiDayOfWeek.Thursday),
            ("friday", ThaiDayOfWeek.Friday), ("saturday", ThaiDayOfWeek.Saturday),
        };

        public static AstrologyTables Tables(float amplitude = 0f, float noise = 0f)
        {
            return new AstrologyTables(
                50f,
                new FortuneCycle(12f, amplitude, new Dictionary<FortuneArea, float> { { FortuneArea.Love, 3f } }),
                noise,
                2020,
                Signs.Select(s => new ZodiacSignDefinition(s.Id, $"zodiac.{s.Id}.name", s.Month, s.Day, Mods(SignModifier(s.Id)))),
                Days.Select(d => new BirthDayDefinition(d.Day, $"thaiday.{d.Name}.name", Mods(DayModifier(d.Day)))),
                Elements.Select(e => new ElementDefinition(e, $"element.{e}.name", Mods(e == "water" ? (FortuneArea.Study, 3f) : ((FortuneArea, float)?)null))),
                Animals.Select(a => new YearAnimalDefinition(a.Id, $"animal.{a.Id}.name", a.Element)));
        }

        public static string Json()
        {
            var root = new JObject
            {
                ["baseFortune"] = 50,
                ["cycle"] = new JObject { ["periodYears"] = 12, ["amplitude"] = 0, ["areaPhaseYears"] = new JObject { ["love"] = 3 } },
                ["yearlyNoise"] = 0,
                ["yearAnimalAnchorYear"] = 2020,
                ["zodiacSigns"] = new JArray(Signs.Select(s => new JObject
                {
                    ["id"] = s.Id, ["nameKey"] = $"zodiac.{s.Id}.name", ["startMonth"] = s.Month, ["startDay"] = s.Day,
                    ["modifiers"] = s.Id == "aries" ? new JObject { ["career"] = 10 } : new JObject(),
                })),
                ["birthDays"] = new JArray(Days.Select(d => new JObject { ["day"] = d.Name, ["nameKey"] = $"thaiday.{d.Name}.name", ["modifiers"] = new JObject() })),
                ["elements"] = new JArray(Elements.Select(e => new JObject { ["id"] = e, ["nameKey"] = $"element.{e}.name", ["modifiers"] = new JObject() })),
                ["yearAnimals"] = new JArray(Animals.Select(a => new JObject { ["id"] = a.Id, ["nameKey"] = $"animal.{a.Id}.name", ["element"] = a.Element })),
            };
            return root.ToString();
        }

        public const string LuckJson = @"{ 'chance': { 'min': 5, 'max': 95, 'itemBonusCap': 5 },
            'fortune': { 'neutral': 50, 'pointsPerFortune': 0.2 },
            'karma': { 'goodWeight': 0.1, 'badWeight': 0.1, 'cap': 10, 'debtRatio': 0.5, 'debtDelayWeeks': { 'min': 26, 'max': 260 }, 'meritOffsetRatio': 0.5 } }";

        public static LuckSettings Luck() => new LuckSettings(5f, 95f, 5f, 50f, 0.2f);

        public static KarmaService Karma() => new KarmaService(new KarmaSettings(0.1f, 0.1f, 10f, 0.5f, 26, 260, 0.5f));

        private static (FortuneArea, float)? SignModifier(string id) => id == "aries" ? (FortuneArea.Career, 10f) : ((FortuneArea, float)?)null;

        private static (FortuneArea, float)? DayModifier(ThaiDayOfWeek day)
        {
            switch (day)
            {
                case ThaiDayOfWeek.Monday:
                    return (FortuneArea.Love, 5f);
                case ThaiDayOfWeek.WednesdayNight:
                    return (FortuneArea.Finance, 7f);
                default:
                    return null;
            }
        }

        private static FortuneModifiers Mods((FortuneArea Area, float Points)? modifier)
        {
            var map = new Dictionary<FortuneArea, float>();
            if (modifier.HasValue)
            {
                map.Add(modifier.Value.Area, modifier.Value.Points);
            }
            return new FortuneModifiers(map);
        }
    }
}
