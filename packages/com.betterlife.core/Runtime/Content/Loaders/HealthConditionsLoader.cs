using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/health_conditions.json into a health condition catalog.
    /// </summary>
    public sealed class HealthConditionsLoader : DefinitionLoader<HealthConditionDefinition>
    {
        /// <inheritdoc />
        public override string TypeName => ContentTypes.HealthConditions;

        /// <inheritdoc />
        protected override string ItemsProperty => "conditions";

        /// <inheritdoc />
        protected override HealthConditionDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "descKey");
            return new HealthConditionDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                RuleJson.RequireId(item, "descKey", context));
        }
    }
}
