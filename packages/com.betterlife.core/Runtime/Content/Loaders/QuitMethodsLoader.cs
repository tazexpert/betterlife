using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using BetterLife.Core.Vices;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/quit_methods.json into a catalog of <see cref="QuitMethodDefinition"/>.
    /// </summary>
    public sealed class QuitMethodsLoader : DefinitionLoader<QuitMethodDefinition>
    {
        private static readonly string[] Dependencies = { ContentTypes.RuleTypes, ContentTypes.Vices };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.QuitMethods;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override string ItemsProperty => "methods";

        /// <inheritdoc />
        protected override QuitMethodDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "vices", "costCash", "durationWeeks", "resistBonus", "decayMultiplier",
                "withdrawalMultiplier", "condition", "startEffects", "successEffects");
            List<string> vices = null;
            if (item["vices"] != null && item["vices"].Type != JTokenType.Null)
            {
                var catalog = content.Require<DefinitionCatalog<ViceDefinition>>();
                var (array, arrayContext) = RuleJson.RequireArrayProperty(item, "vices", context);
                vices = array.Select((token, i) =>
                {
                    string id = token.Type == JTokenType.String ? (string)token : null;
                    if (id == null || !catalog.Contains(id))
                    {
                        throw arrayContext.Index(i).Error($"unknown vice '{token}'.");
                    }
                    return id;
                }).ToList();
            }
            ICondition condition = item["condition"] == null || item["condition"].Type == JTokenType.Null
                ? null : content.Rules.ParseCondition(item["condition"], context.Property("condition"));
            return new QuitMethodDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                vices,
                RuleJson.RequireDecimal(item, "costCash", context),
                RuleJson.RequireInt(item, "durationWeeks", context),
                RuleJson.RequireFloat(item, "resistBonus", context),
                RuleJson.RequireFloat(item, "decayMultiplier", context),
                RuleJson.RequireFloat(item, "withdrawalMultiplier", context),
                condition,
                content.Rules.ParseEffects(item["startEffects"], context.Property("startEffects")),
                content.Rules.ParseEffects(item["successEffects"], context.Property("successEffects")));
        }
    }
}
