using BetterLife.Core.Domain.Family;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"familyBackground","background":id}.
    /// </summary>
    public sealed class FamilyBackgroundConditionConverter : ConditionConverter<FamilyBackgroundCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "familyBackground";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override FamilyBackgroundCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "background");
            string id = RuleJson.RequireId(json, "background", context);
            if (!context.RequireCatalogs().FamilyBackgrounds.Contains(id))
            {
                throw context.Property("background").Error($"unknown family background '{id}'.");
            }
            return new FamilyBackgroundCondition(id);
        }

        /// <inheritdoc />
        protected override void WriteTyped(FamilyBackgroundCondition condition, JObject json, RuleRegistry registry)
        {
            json["background"] = condition.BackgroundId;
        }
    }
}
