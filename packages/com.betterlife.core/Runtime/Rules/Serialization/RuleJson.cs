using System;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Flags;
using BetterLife.Core.Domain.Stats;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization
{
    /// <summary>
    /// Strict JSON reading and writing helpers shared by rule converters. Every read failure reports its JSON path.
    /// </summary>
    public static class RuleJson
    {
        /// <summary>Discriminator property of every condition and effect.</summary>
        public const string TypeProperty = "type";

        private const string MinProperty = "min";
        private const string MaxProperty = "max";

        /// <summary>
        /// Requires the token to be a JSON object.
        /// </summary>
        /// <param name="token">Token.</param>
        /// <param name="context">Read context.</param>
        /// <returns>The object.</returns>
        public static JObject RequireObject(JToken token, RuleReadContext context)
        {
            if (!(token is JObject obj))
            {
                throw context.Error("expected an object.");
            }
            return obj;
        }

        /// <summary>
        /// Requires the token to be a JSON array.
        /// </summary>
        /// <param name="token">Token.</param>
        /// <param name="context">Read context.</param>
        /// <returns>The array.</returns>
        public static JArray RequireArray(JToken token, RuleReadContext context)
        {
            if (!(token is JArray array))
            {
                throw context.Error("expected an array.");
            }
            return array;
        }

        /// <summary>
        /// Rejects properties other than "type" and the allowed ones (catches typos in content).
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="context">Read context.</param>
        /// <param name="allowed">Allowed property names.</param>
        public static void RequireOnly(JObject obj, RuleReadContext context, params string[] allowed)
        {
            foreach (JProperty property in obj.Properties())
            {
                if (property.Name != TypeProperty && !allowed.Contains(property.Name))
                {
                    throw context.Property(property.Name).Error("unknown property.");
                }
            }
        }

        /// <summary>
        /// Reads a required non-empty string.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The string.</returns>
        public static string RequireString(JObject obj, string name, RuleReadContext context)
        {
            JToken token = RequireToken(obj, name, context);
            if (token.Type != JTokenType.String || string.IsNullOrEmpty((string)token))
            {
                throw context.Property(name).Error("expected a non-empty string.");
            }
            return (string)token;
        }

        /// <summary>
        /// Reads an optional content id.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The id, or null when absent.</returns>
        public static string OptionalId(JObject obj, string name, RuleReadContext context)
        {
            return obj[name] == null ? null : RequireId(obj, name, context);
        }

        /// <summary>
        /// Reads a required, well-formed content id.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The id.</returns>
        public static string RequireId(JObject obj, string name, RuleReadContext context)
        {
            string id = RequireString(obj, name, context);
            if (!ContentId.IsValid(id))
            {
                throw context.Property(name).Error($"'{id}' is not a valid id.");
            }
            return id;
        }

        /// <summary>
        /// Reads a required number as float.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The number.</returns>
        public static float RequireFloat(JObject obj, string name, RuleReadContext context)
        {
            return ToFloat(RequireToken(obj, name, context), context.Property(name));
        }

        /// <summary>
        /// Reads a required number as decimal.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The number.</returns>
        public static decimal RequireDecimal(JObject obj, string name, RuleReadContext context)
        {
            return ToDecimal(RequireToken(obj, name, context), context.Property(name));
        }

        /// <summary>
        /// Reads a required integer.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The integer.</returns>
        public static int RequireInt(JObject obj, string name, RuleReadContext context)
        {
            JToken token = RequireToken(obj, name, context);
            if (token.Type != JTokenType.Integer)
            {
                throw context.Property(name).Error("expected an integer.");
            }
            try
            {
                return (int)token;
            }
            catch (OverflowException ex)
            {
                throw context.Property(name).Error("integer is out of range.", ex);
            }
        }

        /// <summary>
        /// Reads an optional integer.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The integer, or null when absent.</returns>
        public static int? OptionalInt(JObject obj, string name, RuleReadContext context)
        {
            return obj[name] == null ? (int?)null : RequireInt(obj, name, context);
        }

        /// <summary>
        /// Reads an optional boolean.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The boolean, or null when absent.</returns>
        public static bool? OptionalBool(JObject obj, string name, RuleReadContext context)
        {
            JToken token = obj[name];
            if (token == null)
            {
                return null;
            }
            if (token.Type != JTokenType.Boolean)
            {
                throw context.Property(name).Error("expected true or false.");
            }
            return (bool)token;
        }

        /// <summary>
        /// Reads a required comparison operator symbol from "op".
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The operator.</returns>
        public static ComparisonOperator RequireOperator(JObject obj, RuleReadContext context)
        {
            const string name = "op";
            string symbol = RequireString(obj, name, context);
            if (!Comparison.TryParseSymbol(symbol, out ComparisonOperator op))
            {
                throw context.Property(name).Error($"unknown operator '{symbol}' (use == != < <= > >=).");
            }
            return op;
        }

        /// <summary>
        /// Reads a number or a {"min","max"} object.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The range.</returns>
        public static FloatRange RequireFloatRange(JObject obj, string name, RuleReadContext context)
        {
            JToken token = RequireToken(obj, name, context);
            RuleReadContext child = context.Property(name);
            if (!(token is JObject range))
            {
                return FloatRange.Fixed(ToFloat(token, child));
            }

            RequireOnly(range, child, MinProperty, MaxProperty);
            float min = RequireFloat(range, MinProperty, child);
            float max = RequireFloat(range, MaxProperty, child);
            if (max < min)
            {
                throw child.Error("max must be at least min.");
            }
            return new FloatRange(min, max);
        }

        /// <summary>
        /// Reads a money number or a {"min","max"} object.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The range.</returns>
        public static DecimalRange RequireDecimalRange(JObject obj, string name, RuleReadContext context)
        {
            JToken token = RequireToken(obj, name, context);
            RuleReadContext child = context.Property(name);
            if (!(token is JObject range))
            {
                return DecimalRange.Fixed(ToDecimal(token, child));
            }

            RequireOnly(range, child, MinProperty, MaxProperty);
            decimal min = RequireDecimal(range, MinProperty, child);
            decimal max = RequireDecimal(range, MaxProperty, child);
            if (max < min)
            {
                throw child.Error("max must be at least min.");
            }
            return new DecimalRange(min, max);
        }

        /// <summary>
        /// Reads a stat id that exists in the catalog; optionally it must be stored (not derived).
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <param name="requireStored">True when the stat will be changed.</param>
        /// <returns>The stat id.</returns>
        public static StatId RequireStat(JObject obj, string name, RuleReadContext context, bool requireStored)
        {
            var id = new StatId(RequireId(obj, name, context));
            if (!context.RequireCatalogs().Stats.Contains(id))
            {
                throw context.Property(name).Error($"unknown stat '{id}'.");
            }
            if (requireStored && context.RequireCatalogs().Stats.IsDerived(id))
            {
                throw context.Property(name).Error($"stat '{id}' is derived and cannot be changed directly; change one of its inputs.");
            }
            return id;
        }

        /// <summary>
        /// Reads a declared flag id, optionally of a specific kind.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <param name="kind">Required kind, or null for any.</param>
        /// <returns>The flag id.</returns>
        public static string RequireFlag(JObject obj, string name, RuleReadContext context, FlagKind? kind)
        {
            string id = RequireId(obj, name, context);
            if (!context.RequireCatalogs().Flags.TryGet(id, out FlagDefinition definition))
            {
                throw context.Property(name).Error($"undeclared flag '{id}' (declare it in flags.json).");
            }
            if (kind.HasValue && definition.Kind != kind.Value)
            {
                throw context.Property(name).Error($"flag '{id}' is {definition.Kind}, expected {kind.Value}.");
            }
            return id;
        }

        /// <summary>
        /// Reads a trait id that exists in the catalog.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The trait id.</returns>
        public static string RequireTrait(JObject obj, string name, RuleReadContext context)
        {
            string id = RequireId(obj, name, context);
            if (!context.RequireCatalogs().Traits.Contains(id))
            {
                throw context.Property(name).Error($"unknown trait '{id}'.");
            }
            return id;
        }

        /// <summary>
        /// Reads a skill id that exists in the catalog.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The skill id.</returns>
        public static string RequireSkill(JObject obj, string name, RuleReadContext context)
        {
            string id = RequireId(obj, name, context);
            if (!context.RequireCatalogs().Skills.Contains(id))
            {
                throw context.Property(name).Error($"unknown skill '{id}'.");
            }
            return id;
        }

        /// <summary>
        /// Reads a declared reputation domain id.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The domain id.</returns>
        public static string RequireReputationDomain(JObject obj, string name, RuleReadContext context)
        {
            string id = RequireId(obj, name, context);
            if (!context.RequireCatalogs().ReputationDomains.Contains(id))
            {
                throw context.Property(name).Error($"unknown reputation domain '{id}'.");
            }
            return id;
        }

        /// <summary>
        /// Reads a declared health condition id.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The condition id.</returns>
        public static string RequireHealthCondition(JObject obj, string name, RuleReadContext context)
        {
            string id = RequireId(obj, name, context);
            if (!context.RequireCatalogs().HealthConditions.Contains(id))
            {
                throw context.Property(name).Error($"unknown health condition '{id}'.");
            }
            return id;
        }

        /// <summary>
        /// Writes a float without binary noise (0.35 rather than 0.3499999940395355).
        /// </summary>
        /// <param name="value">Value.</param>
        /// <returns>The JSON token.</returns>
        public static JToken Number(float value) => Number((decimal)value);

        /// <summary>
        /// Writes a decimal, using an integer token for whole values (70 rather than 70.0).
        /// </summary>
        /// <param name="value">Value.</param>
        /// <returns>The JSON token.</returns>
        public static JToken Number(decimal value)
        {
            bool whole = value == decimal.Truncate(value) && value >= long.MinValue && value <= long.MaxValue;
            return whole ? new JValue((long)value) : new JValue(value);
        }

        /// <summary>
        /// Writes a fixed value as a number and a range as {"min","max"}.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <returns>The JSON token.</returns>
        public static JToken Range(FloatRange range)
        {
            return range.IsFixed ? Number(range.Min) : new JObject { [MinProperty] = Number(range.Min), [MaxProperty] = Number(range.Max) };
        }

        /// <summary>
        /// Writes a fixed amount as a number and a range as {"min","max"}.
        /// </summary>
        /// <param name="range">Range.</param>
        /// <returns>The JSON token.</returns>
        public static JToken Range(DecimalRange range)
        {
            return range.IsFixed ? Number(range.Min) : new JObject { [MinProperty] = Number(range.Min), [MaxProperty] = Number(range.Max) };
        }

        private static JToken RequireToken(JObject obj, string name, RuleReadContext context)
        {
            JToken token = obj[name];
            if (token == null || token.Type == JTokenType.Null)
            {
                throw context.Property(name).Error("is required.");
            }
            return token;
        }

        private static float ToFloat(JToken token, RuleReadContext context)
        {
            if (token.Type != JTokenType.Integer && token.Type != JTokenType.Float)
            {
                throw context.Error("expected a number.");
            }
            float value = (float)token;
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw context.Error("number is out of range.");
            }
            return value;
        }

        private static decimal ToDecimal(JToken token, RuleReadContext context)
        {
            if (token.Type != JTokenType.Integer && token.Type != JTokenType.Float)
            {
                throw context.Error("expected a number.");
            }
            try
            {
                return (decimal)token;
            }
            catch (OverflowException ex)
            {
                throw context.Error("number is out of range.", ex);
            }
        }

        /// <summary>
        /// Returns the elements of an array property with their read contexts.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The array and its context.</returns>
        public static (JArray Array, RuleReadContext Context) RequireArrayProperty(JObject obj, string name, RuleReadContext context)
        {
            RuleReadContext child = context.Property(name);
            return (RequireArray(RequireToken(obj, name, context), child), child);
        }

        /// <summary>
        /// Returns an object-valued property with its read context.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The token and its context.</returns>
        public static (JToken Token, RuleReadContext Context) RequireProperty(JObject obj, string name, RuleReadContext context)
        {
            return (RequireToken(obj, name, context), context.Property(name));
        }
    }
}
