using System.Collections.Generic;
using BetterLife.Core.Domain;
using BetterLife.Core.Economy;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads purchases.json: big purchases (homes, vehicles). The asset and the loan must exist, the loan must be secured by
    /// that kind of asset, and the price must reach the asset's minimum.
    /// </summary>
    public sealed class PurchasesLoader : DefinitionLoader<PurchaseDefinition>
    {
        private static readonly string[] Dependencies = { ContentTypes.Assets, ContentTypes.DebtProducts, ContentTypes.RuleTypes };

        private static readonly IReadOnlyDictionary<string, PurchaseCategory> Categories = new Dictionary<string, PurchaseCategory>
        {
            ["home"] = PurchaseCategory.Home,
            ["vehicle"] = PurchaseCategory.Vehicle,
        };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.Purchases;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override string ItemsProperty => "purchases";

        /// <inheritdoc />
        protected override PurchaseDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "descKey", "category", "assetId", "price", "loanProduct", "effects");
            string assetId = RuleJson.RequireId(item, "assetId", context);
            string loanId = RuleJson.RequireId(item, "loanProduct", context);
            var assets = content.Require<DefinitionCatalog<AssetDefinition>>();
            var products = content.Require<DefinitionCatalog<DebtProductDefinition>>();
            if (!assets.TryGet(assetId, out AssetDefinition asset))
            {
                throw context.Property("assetId").Error($"unknown asset '{assetId}'.");
            }
            if (!products.TryGet(loanId, out DebtProductDefinition product))
            {
                throw context.Property("loanProduct").Error($"unknown loan product '{loanId}'.");
            }
            if (product.CollateralClass != asset.Class)
            {
                throw context.Property("loanProduct").Error($"'{loanId}' is not secured by this kind of asset.");
            }
            decimal price = RuleJson.RequireDecimal(item, "price", context);
            if (price < asset.MinPurchase)
            {
                throw context.Property("price").Error("price is below the asset's minimum purchase.");
            }
            return new PurchaseDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                RuleJson.RequireId(item, "descKey", context),
                ContentReading.RequireEnum(item, "category", context, Categories),
                assetId, price, loanId,
                content.Rules.ParseEffects(item["effects"], context.Property("effects")));
        }
    }
}
