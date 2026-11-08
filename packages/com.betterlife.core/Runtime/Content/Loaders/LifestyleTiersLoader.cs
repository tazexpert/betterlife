using System.Collections.Generic;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Economy;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/lifestyle_tiers.json into a catalog of <see cref="LifestyleTierDefinition"/>.
    /// </summary>
    public sealed class LifestyleTiersLoader : DefinitionLoader<LifestyleTierDefinition>
    {
        private static readonly string[] Dependencies = { ContentTypes.Stats };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.LifestyleTiers;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override string ItemsProperty => "tiers";

        /// <inheritdoc />
        protected override LifestyleTierDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "monthlyCost", "monthlyStats");
            return new LifestyleTierDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                RuleJson.RequireDecimal(item, "monthlyCost", context),
                ContentReading.ReadStatAmounts(item, "monthlyStats", context, content.Require<StatCatalog>()));
        }
    }
}
