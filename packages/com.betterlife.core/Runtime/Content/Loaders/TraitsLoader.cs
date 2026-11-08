using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Domain.Traits;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/traits.json into a trait catalog. Modifier stats must exist.
    /// </summary>
    public sealed class TraitsLoader : DefinitionLoader<TraitDefinition>
    {
        private static readonly string[] Dependencies = { ContentTypes.Stats };

        private static readonly Dictionary<string, ModifierDirection> Directions = new Dictionary<string, ModifierDirection>
        {
            { "gain", ModifierDirection.Gain },
            { "loss", ModifierDirection.Loss },
            { "both", ModifierDirection.Both },
        };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.Traits;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override string ItemsProperty => "traits";

        /// <inheritdoc />
        protected override TraitDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "descKey", "category", "exclusiveGroup", "startsHidden", "statModifiers");
            StatCatalog stats = content.Require<StatCatalog>();
            var (modifiers, modifiersContext) = RuleJson.RequireArrayProperty(item, "statModifiers", context);
            return new TraitDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                RuleJson.RequireId(item, "descKey", context),
                RuleJson.RequireId(item, "category", context),
                RuleJson.OptionalId(item, "exclusiveGroup", context),
                ContentReading.RequireBool(item, "startsHidden", context),
                modifiers.Select((token, i) => ReadModifier(token, modifiersContext.Index(i), stats)).ToList());
        }

        private static TraitStatModifier ReadModifier(JToken token, RuleReadContext context, StatCatalog stats)
        {
            JObject modifier = RuleJson.RequireObject(token, context);
            RuleJson.RequireOnly(modifier, context, "stat", "direction", "multiplier");
            var statId = new StatId(RuleJson.RequireId(modifier, "stat", context));
            if (!stats.Contains(statId))
            {
                throw context.Property("stat").Error($"unknown stat '{statId}'.");
            }
            return new TraitStatModifier(
                statId,
                ContentReading.RequireEnum(modifier, "direction", context, Directions),
                RuleJson.RequireFloat(modifier, "multiplier", context));
        }
    }
}
