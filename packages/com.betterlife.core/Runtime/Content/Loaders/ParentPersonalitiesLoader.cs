using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Family;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/parent_personalities.json. Traits and stats must exist.
    /// </summary>
    public sealed class ParentPersonalitiesLoader : DefinitionLoader<ParentPersonalityDefinition>
    {
        private static readonly string[] Dependencies = { ContentTypes.CharacterCatalogs };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.ParentPersonalities;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override string ItemsProperty => "personalities";

        /// <inheritdoc />
        protected override ParentPersonalityDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "weight", "statModifiers", "traitChances");
            var (chances, chancesContext) = RuleJson.RequireArrayProperty(item, "traitChances", context);
            return new ParentPersonalityDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                RuleJson.RequireFloat(item, "weight", context),
                ContentReading.ReadStatAmounts(item, "statModifiers", context, content.Require<StatCatalog>()),
                chances.Select((token, i) =>
                {
                    RuleReadContext chanceContext = chancesContext.Index(i);
                    JObject chance = RuleJson.RequireObject(token, chanceContext);
                    RuleJson.RequireOnly(chance, chanceContext, "trait", "chance");
                    return new TraitChance(RuleJson.RequireTrait(chance, "trait", chanceContext), RuleJson.RequireFloat(chance, "chance", chanceContext));
                }).ToList());
        }
    }
}
