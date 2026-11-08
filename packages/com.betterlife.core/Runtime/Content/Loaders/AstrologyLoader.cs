using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Astrology;
using BetterLife.Core.Rules.Serialization;
using BetterLife.Core.Time;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/astrology.json into <see cref="AstrologyTables"/> and <see cref="FortuneCalculator"/>,
    /// and registers the "fortune" condition.
    /// </summary>
    public sealed class AstrologyLoader : SingleDocumentLoader
    {
        private static readonly string[] NoDependencies = new string[0];

        private static readonly Dictionary<string, ThaiDayOfWeek> Days = new Dictionary<string, ThaiDayOfWeek>
        {
            { "sunday", ThaiDayOfWeek.Sunday },
            { "monday", ThaiDayOfWeek.Monday },
            { "tuesday", ThaiDayOfWeek.Tuesday },
            { "wednesday", ThaiDayOfWeek.Wednesday },
            { "wednesday_night", ThaiDayOfWeek.WednesdayNight },
            { "thursday", ThaiDayOfWeek.Thursday },
            { "friday", ThaiDayOfWeek.Friday },
            { "saturday", ThaiDayOfWeek.Saturday },
        };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.Astrology;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => NoDependencies;

        /// <inheritdoc />
        protected override void Read(JObject root, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(root, context, "baseFortune", "cycle", "yearlyNoise", "yearAnimalAnchorYear", "zodiacSigns", "birthDays", "elements", "yearAnimals", ContentReading.SchemaProperty);
            var tables = new AstrologyTables(
                RuleJson.RequireFloat(root, "baseFortune", context),
                ReadCycle(root, context),
                RuleJson.RequireFloat(root, "yearlyNoise", context),
                RuleJson.RequireInt(root, "yearAnimalAnchorYear", context),
                ReadArray(root, "zodiacSigns", context, (item, c) =>
                {
                    RuleJson.RequireOnly(item, c, "id", "nameKey", "startMonth", "startDay", "modifiers");
                    return new ZodiacSignDefinition(RuleJson.RequireId(item, "id", c), RuleJson.RequireId(item, "nameKey", c),
                        RuleJson.RequireInt(item, "startMonth", c), RuleJson.RequireInt(item, "startDay", c), ReadModifiers(item, c));
                }),
                ReadArray(root, "birthDays", context, (item, c) =>
                {
                    RuleJson.RequireOnly(item, c, "day", "nameKey", "modifiers");
                    return new BirthDayDefinition(ContentReading.RequireEnum(item, "day", c, Days), RuleJson.RequireId(item, "nameKey", c), ReadModifiers(item, c));
                }),
                ReadArray(root, "elements", context, (item, c) =>
                {
                    RuleJson.RequireOnly(item, c, "id", "nameKey", "modifiers");
                    return new ElementDefinition(RuleJson.RequireId(item, "id", c), RuleJson.RequireId(item, "nameKey", c), ReadModifiers(item, c));
                }),
                ReadArray(root, "yearAnimals", context, (item, c) =>
                {
                    RuleJson.RequireOnly(item, c, "id", "nameKey", "element");
                    return new YearAnimalDefinition(RuleJson.RequireId(item, "id", c), RuleJson.RequireId(item, "nameKey", c), RuleJson.RequireId(item, "element", c));
                }));

            var calculator = new FortuneCalculator(tables);
            content.Add(tables);
            content.Add(calculator);
            content.Rules.Register(new FortuneConditionConverter(calculator));
        }

        private static FortuneCycle ReadCycle(JObject root, RuleReadContext context)
        {
            var (token, cycleContext) = RuleJson.RequireProperty(root, "cycle", context);
            JObject cycle = RuleJson.RequireObject(token, cycleContext);
            RuleJson.RequireOnly(cycle, cycleContext, "periodYears", "amplitude", "areaPhaseYears");
            return new FortuneCycle(
                RuleJson.RequireFloat(cycle, "periodYears", cycleContext),
                RuleJson.RequireFloat(cycle, "amplitude", cycleContext),
                ReadAreaNumbers(cycle, "areaPhaseYears", cycleContext));
        }

        private static FortuneModifiers ReadModifiers(JObject item, RuleReadContext context)
        {
            return new FortuneModifiers(ReadAreaNumbers(item, "modifiers", context));
        }

        private static Dictionary<FortuneArea, float> ReadAreaNumbers(JObject obj, string property, RuleReadContext context)
        {
            var (token, mapContext) = RuleJson.RequireProperty(obj, property, context);
            JObject map = RuleJson.RequireObject(token, mapContext);
            RuleJson.RequireOnly(map, mapContext, FortuneAreas.Names.Keys.ToArray());
            var result = new Dictionary<FortuneArea, float>();
            foreach (KeyValuePair<string, FortuneArea> area in FortuneAreas.Names.Where(a => map[a.Key] != null))
            {
                result.Add(area.Value, RuleJson.RequireFloat(map, area.Key, mapContext));
            }
            return result;
        }

        private static List<T> ReadArray<T>(JObject root, string property, RuleReadContext context, System.Func<JObject, RuleReadContext, T> read)
        {
            var (array, arrayContext) = RuleJson.RequireArrayProperty(root, property, context);
            return array.Select((token, i) =>
            {
                RuleReadContext itemContext = arrayContext.Index(i);
                return read(RuleJson.RequireObject(token, itemContext), itemContext);
            }).ToList();
        }
    }
}
