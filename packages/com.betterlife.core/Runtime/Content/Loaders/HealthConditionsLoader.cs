using System.Collections.Generic;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/health_conditions.json into a health condition catalog (weekly effects, recovery, mortality).
    /// </summary>
    public sealed class HealthConditionsLoader : DefinitionLoader<HealthConditionDefinition>
    {
        /// <inheritdoc />
        public override string TypeName => ContentTypes.HealthConditions;

        private static readonly string[] Dependencies = { ContentTypes.Stats };

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override string ItemsProperty => "conditions";

        /// <inheritdoc />
        protected override HealthConditionDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "descKey", "weeklyStats", "recoveryChancePerWeek", "annualMortality");
            float? recovery = item["recoveryChancePerWeek"] == null || item["recoveryChancePerWeek"].Type == JTokenType.Null
                ? (float?)null
                : RuleJson.RequireFloat(item, "recoveryChancePerWeek", context);
            return new HealthConditionDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                RuleJson.RequireId(item, "descKey", context),
                item["weeklyStats"] == null ? null : ContentReading.ReadStatAmounts(item, "weeklyStats", context, content.Require<StatCatalog>()),
                recovery,
                item["annualMortality"] == null ? 0f : RuleJson.RequireFloat(item, "annualMortality", context));
        }
    }
}
