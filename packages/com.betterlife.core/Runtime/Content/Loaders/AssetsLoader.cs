using BetterLife.Core.Economy;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/assets.json into a catalog of <see cref="AssetDefinition"/>.
    /// </summary>
    public sealed class AssetsLoader : DefinitionLoader<AssetDefinition>
    {
        private const float DefaultBeta = 1f;

        /// <inheritdoc />
        public override string TypeName => ContentTypes.Assets;

        /// <inheritdoc />
        protected override string ItemsProperty => "assets";

        /// <inheritdoc />
        protected override AssetDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "class", "minPurchase", "sellFeeRate",
                "rateSpread", "beta", "idiosyncraticVolatility", "rentalYield", "depreciation");
            return new AssetDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                ContentReading.RequireEnum(item, "class", context, EconomyNames.AssetClasses),
                RuleJson.RequireDecimal(item, "minPurchase", context),
                RuleJson.RequireDecimal(item, "sellFeeRate", context),
                ContentReading.OptionalFloat(item, "rateSpread", context, 0f),
                ContentReading.OptionalFloat(item, "beta", context, DefaultBeta),
                ContentReading.OptionalFloat(item, "idiosyncraticVolatility", context, 0f),
                ContentReading.OptionalFloat(item, "rentalYield", context, 0f),
                ContentReading.OptionalFloat(item, "depreciation", context, 0f));
        }
    }
}
