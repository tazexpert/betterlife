using System.Collections.Generic;
using BetterLife.Core.Content.Loaders;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Karma
{
    /// <summary>
    /// Reads {"type":"karma","kind":"bad","op":"&gt;","value":30}.
    /// </summary>
    public sealed class KarmaConditionConverter : ConditionConverter<KarmaCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "karma";

        private static readonly Dictionary<string, KarmaKind> Kinds = new Dictionary<string, KarmaKind>
        {
            { "good", KarmaKind.Good },
            { "bad", KarmaKind.Bad },
        };

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override KarmaCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "kind", "op", "value");
            return new KarmaCondition(ContentReading.RequireEnum(json, "kind", context, Kinds), RuleJson.RequireOperator(json, context), RuleJson.RequireFloat(json, "value", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(KarmaCondition condition, JObject json, RuleRegistry registry)
        {
            json["kind"] = condition.Kind == KarmaKind.Good ? "good" : "bad";
            json["op"] = Comparison.ToSymbol(condition.Operator);
            json["value"] = RuleJson.Number(condition.Value);
        }
    }
}
