using System.Collections.Generic;
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

        private static void RequirePresent(JObject obj, string name, RuleReadContext context)
        {
            if (!obj.ContainsKey(name))
            {
                throw context.Property(name).Error("is required (use null for none).");
            }
        }
    }
}
