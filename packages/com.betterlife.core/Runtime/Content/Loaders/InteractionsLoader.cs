using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Luck;
using BetterLife.Core.Relationships;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/interactions.json into a catalog of <see cref="InteractionDefinition"/>.
    /// </summary>
    public sealed class InteractionsLoader : DefinitionLoader<InteractionDefinition>
    {
        private static readonly string[] Dependencies = { ContentTypes.RuleTypes };

        private static readonly Dictionary<string, ChildMode> ChildModes = new Dictionary<string, ChildMode>
        {
            ["birth"] = ChildMode.Birth,
            ["adoption"] = ChildMode.Adoption,
        };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.Interactions;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override string ItemsProperty => "interactions";

        /// <inheritdoc />
        protected override InteractionDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "types", "minValue", "maxValue", "condition", "costCash", "cooldownWeeks", "chance",
                "compatibilityPerPoint", "success", "failure");
            var (types, typesContext) = RuleJson.RequireArrayProperty(item, "types", context);
            List<RelationshipType> relationTypes = types.Select((token, i) =>
            {
                RuleReadContext c = typesContext.Index(i);
                string name = token.Type == JTokenType.String ? (string)token : null;
                if (name == null || !RelationshipNames.Types.TryGetValue(name, out RelationshipType type))
                {
                    throw c.Error($"'{token}' is not one of: {string.Join(", ", RelationshipNames.Types.Keys)}.");
                }
                return type;
            }).ToList();
            ICondition condition = IsNull(item["condition"]) ? null : content.Rules.ParseCondition(item["condition"], context.Property("condition"));
            ChanceSpec chance = IsNull(item["chance"]) ? null : ChanceSpecReader.Read(item["chance"], context.Property("chance"));
            return new InteractionDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                relationTypes,
                ContentReading.RequireNullableFloat(item, "minValue", context),
                ContentReading.RequireNullableFloat(item, "maxValue", context),
                condition,
                RuleJson.RequireDecimal(item, "costCash", context),
                RuleJson.RequireInt(item, "cooldownWeeks", context),
                chance,
                RuleJson.RequireFloat(item, "compatibilityPerPoint", context),
                ReadOutcome(item, "success", context, content),
                ReadOutcome(item, "failure", context, content));
        }

        private static InteractionOutcome ReadOutcome(JObject item, string name, RuleReadContext context, ContentLoadContext content)
        {
            var (token, outcomeContext) = RuleJson.RequireProperty(item, name, context);
            JObject outcome = RuleJson.RequireObject(token, outcomeContext);
            RuleJson.RequireOnly(outcome, outcomeContext, "value", "becomes", "child", "memoryKey", "effects");
            string becomes = ContentReading.RequireNullableId(outcome, "becomes", outcomeContext);
            RelationshipType? type = becomes == null ? (RelationshipType?)null : ContentReading.RequireEnum(outcome, "becomes", outcomeContext, RelationshipNames.Types);
            string child = ContentReading.RequireNullableId(outcome, "child", outcomeContext);
            ChildMode mode = child == null ? ChildMode.None : ContentReading.RequireEnum(outcome, "child", outcomeContext, ChildModes);
            return new InteractionOutcome(
                RuleJson.RequireFloatRange(outcome, "value", outcomeContext),
                type,
                mode,
                ContentReading.RequireNullableId(outcome, "memoryKey", outcomeContext),
                content.Rules.ParseEffects(outcome["effects"], outcomeContext.Property("effects")));
        }

        private static bool IsNull(JToken token) => token == null || token.Type == JTokenType.Null;
    }
}
