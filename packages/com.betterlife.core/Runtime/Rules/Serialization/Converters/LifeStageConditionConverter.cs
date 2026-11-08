using System.Linq;
using BetterLife.Core.Domain.Life;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"lifeStage","stages":["teen","young_adult"]}.
    /// </summary>
    public sealed class LifeStageConditionConverter : ConditionConverter<LifeStageCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "lifeStage";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override LifeStageCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "stages");
            var (array, arrayContext) = RuleJson.RequireArrayProperty(json, "stages", context);
            LifeStageCatalog stages = context.RequireCatalogs().LifeStages;
            return new LifeStageCondition(array.Select((token, i) =>
            {
                RuleReadContext itemContext = arrayContext.Index(i);
                if (token.Type != JTokenType.String || !stages.Contains((string)token))
                {
                    throw itemContext.Error($"unknown life stage '{token}'.");
                }
                return (string)token;
            }).ToList());
        }

        /// <inheritdoc />
        protected override void WriteTyped(LifeStageCondition condition, JObject json, RuleRegistry registry)
        {
            json["stages"] = new JArray(condition.StageIds);
        }
    }
}
