using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Family;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Reading helpers specific to content files (on top of <see cref="RuleJson"/>).
    /// </summary>
    public static class ContentReading
    {
        /// <summary>Optional editor hint property allowed at the root of every content file.</summary>
        public const string SchemaProperty = "$schema";

        /// <summary>
        /// Reads a required string property mapped through a fixed vocabulary.
        /// </summary>
        /// <typeparam name="T">Target value type.</typeparam>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <param name="values">Allowed JSON strings and their values.</param>
        /// <returns>The mapped value.</returns>
        public static T RequireEnum<T>(JObject obj, string name, RuleReadContext context, IReadOnlyDictionary<string, T> values)
        {
            string text = RuleJson.RequireString(obj, name, context);
            if (!values.TryGetValue(text, out T value))
            {
                throw context.Property(name).Error($"'{text}' is not one of: {string.Join(", ", values.Keys)}.");
            }
            return value;
        }

        /// <summary>
        /// Reads a property that must be present but may be null (schema style "type": ["integer", "null"]).
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The integer, or null when the value is null.</returns>
        public static int? RequireNullableInt(JObject obj, string name, RuleReadContext context)
        {
            RequirePresent(obj, name, context);
            return obj[name].Type == JTokenType.Null ? (int?)null : RuleJson.RequireInt(obj, name, context);
        }

        /// <summary>
        /// Reads a property that must be present but may be null, holding a content id.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The id, or null when the value is null.</returns>
        public static string RequireNullableId(JObject obj, string name, RuleReadContext context)
        {
            RequirePresent(obj, name, context);
            return obj[name].Type == JTokenType.Null ? null : RuleJson.RequireId(obj, name, context);
        }

        /// <summary>
        /// Reads a required boolean.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The boolean.</returns>
        public static bool RequireBool(JObject obj, string name, RuleReadContext context)
        {
            bool? value = RuleJson.OptionalBool(obj, name, context);
            if (!value.HasValue)
            {
                throw context.Property(name).Error("is required.");
            }
            return value.Value;
        }

        /// <summary>
        /// Reads an array of { "stat", "amount" } targeting stored stats of <paramref name="stats"/>.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <param name="stats">Stat catalog.</param>
        /// <returns>The amounts.</returns>
        public static List<StatAmount> ReadStatAmounts(JObject obj, string name, RuleReadContext context, StatCatalog stats)
        {
            var (array, arrayContext) = RuleJson.RequireArrayProperty(obj, name, context);
            return array.Select((token, i) =>
            {
                RuleReadContext itemContext = arrayContext.Index(i);
                JObject item = RuleJson.RequireObject(token, itemContext);
                RuleJson.RequireOnly(item, itemContext, "stat", "amount");
                return new StatAmount(RequireStoredStat(item, "stat", itemContext, stats), RuleJson.RequireFloat(item, "amount", itemContext));
            }).ToList();
        }

        /// <summary>
        /// Reads a stat id that exists in <paramref name="stats"/> and is stored (not derived).
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <param name="stats">Stat catalog.</param>
        /// <returns>The stat id.</returns>
        public static StatId RequireStoredStat(JObject obj, string name, RuleReadContext context, StatCatalog stats)
        {
            var id = new StatId(RuleJson.RequireId(obj, name, context));
            if (!stats.Contains(id))
            {
                throw context.Property(name).Error($"unknown stat '{id}'.");
            }
            if (stats.IsDerived(id))
            {
                throw context.Property(name).Error($"stat '{id}' is derived and cannot be changed directly; change one of its inputs.");
            }
            return id;
        }

        private static void RequirePresent(JObject obj, string name, RuleReadContext context)
        {
            if (!obj.ContainsKey(name))
            {
                throw context.Property(name).Error("is required (use null for none).");
            }
        }
    }
}
