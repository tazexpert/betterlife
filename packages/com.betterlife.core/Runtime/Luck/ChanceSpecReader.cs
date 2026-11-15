using System.Linq;
using BetterLife.Core.Astrology;
using BetterLife.Core.Content.Loaders;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Luck
{
    /// <summary>
    /// Reads { "base": 40, "stats": [ { "stat", "weight" } ], "fortune": "study", "karma": true }.
    /// "stats" and "fortune" are optional; "karma" defaults to true.
    /// </summary>
    public static class ChanceSpecReader
    {
        /// <summary>
        /// Reads a chance spec; referenced stats must exist.
        /// </summary>
        /// <param name="token">The chance object.</param>
        /// <param name="context">Read context of the token (with catalogs).</param>
        /// <returns>The spec.</returns>
        public static ChanceSpec Read(JToken token, RuleReadContext context)
        {
            JObject json = RuleJson.RequireObject(token, context);
            RuleJson.RequireOnly(json, context, "base", "stats", "fortune", "karma");
            var terms = new System.Collections.Generic.List<ChanceStatTerm>();
            if (json["stats"] != null)
            {
                var (array, arrayContext) = RuleJson.RequireArrayProperty(json, "stats", context);
                terms.AddRange(array.Select((item, i) => ReadTerm(item, arrayContext.Index(i))));
            }
            FortuneArea? area = json["fortune"] == null ? (FortuneArea?)null : ContentReading.RequireEnum(json, "fortune", context, FortuneAreas.Names);
            return new ChanceSpec(RuleJson.RequireFloat(json, "base", context), terms, area, RuleJson.OptionalBool(json, "karma", context) ?? true);
        }

        /// <summary>
        /// Writes a chance spec.
        /// </summary>
        /// <param name="spec">The spec.</param>
        /// <returns>The JSON object.</returns>
        public static JObject Write(ChanceSpec spec)
        {
            var json = new JObject { ["base"] = RuleJson.Number(spec.BasePercent) };
            if (spec.StatTerms.Count > 0)
            {
                json["stats"] = new JArray(spec.StatTerms.Select(t => new JObject { ["stat"] = t.StatId.Value, ["weight"] = RuleJson.Number(t.Weight) }));
            }
            if (spec.FortuneArea.HasValue)
            {
                json["fortune"] = FortuneAreas.ToName(spec.FortuneArea.Value);
            }
            json["karma"] = spec.UseKarma;
            return json;
        }

        private static ChanceStatTerm ReadTerm(JToken token, RuleReadContext context)
        {
            JObject term = RuleJson.RequireObject(token, context);
            RuleJson.RequireOnly(term, context, "stat", "weight");
            return new ChanceStatTerm(RuleJson.RequireStat(term, "stat", context, false), RuleJson.RequireFloat(term, "weight", context));
        }
    }
}
