using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Economy;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/economy.json into <see cref="EconomySettings"/>.
    /// </summary>
    public sealed class EconomySettingsLoader : SingleDocumentLoader
    {
        private static readonly string[] Dependencies = new string[0];

        /// <inheritdoc />
        public override string TypeName => ContentTypes.Economy;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override void Read(JObject root, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(root, context, "baseYear", "historicalInflation", "maxDebtBalance", "phases", ContentReading.SchemaProperty);
            var (phases, phasesContext) = RuleJson.RequireArrayProperty(root, "phases", context);
            List<EconomyPhaseDefinition> definitions = phases
                .Select((token, i) => ReadPhase(RuleJson.RequireObject(token, phasesContext.Index(i)), phasesContext.Index(i)))
                .ToList();
            content.Add(new EconomySettings(
                RuleJson.RequireInt(root, "baseYear", context),
                RuleJson.RequireFloat(root, "historicalInflation", context),
                RuleJson.RequireDecimal(root, "maxDebtBalance", context),
                definitions));
        }

        private static EconomyPhaseDefinition ReadPhase(JObject phase, RuleReadContext context)
        {
            RuleJson.RequireOnly(phase, context, "id", "nameKey", "next", "durationYears", "inflation", "policyRate", "markets");
            var (durationToken, durationContext) = RuleJson.RequireProperty(phase, "durationYears", context);
            JObject duration = RuleJson.RequireObject(durationToken, durationContext);
            RuleJson.RequireOnly(duration, durationContext, "min", "max");
            var (marketsToken, marketsContext) = RuleJson.RequireProperty(phase, "markets", context);
            JObject markets = RuleJson.RequireObject(marketsToken, marketsContext);
            RuleJson.RequireOnly(markets, marketsContext, "stocks", "property", "crypto");
            return new EconomyPhaseDefinition(
                RuleJson.RequireId(phase, "id", context),
                RuleJson.RequireId(phase, "nameKey", context),
                RuleJson.RequireId(phase, "next", context),
                RuleJson.RequireInt(duration, "min", durationContext),
                RuleJson.RequireInt(duration, "max", durationContext),
                RuleJson.RequireFloatRange(phase, "inflation", context),
                RuleJson.RequireFloatRange(phase, "policyRate", context),
                ReadMarket(markets, "stocks", marketsContext),
                ReadMarket(markets, "property", marketsContext),
                ReadMarket(markets, "crypto", marketsContext));
        }

        private static MarketParameters ReadMarket(JObject markets, string name, RuleReadContext context)
        {
            var (token, marketContext) = RuleJson.RequireProperty(markets, name, context);
            JObject market = RuleJson.RequireObject(token, marketContext);
            RuleJson.RequireOnly(market, marketContext, "mean", "volatility");
            return new MarketParameters(RuleJson.RequireFloat(market, "mean", marketContext), RuleJson.RequireFloat(market, "volatility", marketContext));
        }
    }
}
