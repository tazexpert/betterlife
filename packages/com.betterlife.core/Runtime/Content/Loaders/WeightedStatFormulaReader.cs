using System.Linq;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Reads { "target": id, "type": "weighted", "terms": [ { "stat", "weight", "invert"? } ] }.
    /// </summary>
    public sealed class WeightedStatFormulaReader : IStatFormulaReader
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "weighted";

        /// <inheritdoc />
        public string Type => TypeName;

        /// <inheritdoc />
        public IStatFormula Read(JObject json, RuleReadContext context)
        {
            RuleJson.RequireOnly(json, context, "target", "terms");
            var (terms, termsContext) = RuleJson.RequireArrayProperty(json, "terms", context);
            return new WeightedStatFormula(
                new StatId(RuleJson.RequireId(json, "target", context)),
                terms.Select((token, i) => ReadTerm(token, termsContext.Index(i))).ToList());
        }

        private static StatFormulaTerm ReadTerm(JToken token, RuleReadContext context)
        {
            JObject term = RuleJson.RequireObject(token, context);
            RuleJson.RequireOnly(term, context, "stat", "weight", "invert");
            return new StatFormulaTerm(
                new StatId(RuleJson.RequireId(term, "stat", context)),
                RuleJson.RequireFloat(term, "weight", context),
                RuleJson.OptionalBool(term, "invert", context) ?? false);
        }
    }
}
