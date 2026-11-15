using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/reputation_domains.json into a domain catalog.
    /// </summary>
    public sealed class ReputationDomainsLoader : DefinitionLoader<ReputationDomainDefinition>
    {
        /// <inheritdoc />
        public override string TypeName => ContentTypes.ReputationDomains;

        /// <inheritdoc />
        protected override string ItemsProperty => "domains";

        /// <inheritdoc />
        protected override ReputationDomainDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey");
            return new ReputationDomainDefinition(RuleJson.RequireId(item, "id", context), RuleJson.RequireId(item, "nameKey", context));
        }
    }
}
