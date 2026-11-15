using System.Collections.Generic;
using BetterLife.Core.Domain.Family;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/family_backgrounds.json. Stat modifiers must target stored stats.
    /// </summary>
    public sealed class FamilyBackgroundsLoader : DefinitionLoader<FamilyBackgroundDefinition>
    {
        private static readonly string[] Dependencies = { ContentTypes.Stats };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.FamilyBackgrounds;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override string ItemsProperty => "backgrounds";

        /// <inheritdoc />
        protected override FamilyBackgroundDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "weight", "householdIncomeLevel", "statModifiers");
            return new FamilyBackgroundDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                RuleJson.RequireFloat(item, "weight", context),
                RuleJson.RequireInt(item, "householdIncomeLevel", context),
                ContentReading.ReadStatAmounts(item, "statModifiers", context, content.Require<StatCatalog>()));
        }
    }
}
