using BetterLife.Core.Economy;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/debts.json into a catalog of <see cref="DebtProductDefinition"/>.
    /// </summary>
    public sealed class DebtProductsLoader : DefinitionLoader<DebtProductDefinition>
    {
        /// <inheritdoc />
        public override string TypeName => ContentTypes.DebtProducts;

        /// <inheritdoc />
        protected override string ItemsProperty => "products";

        /// <inheritdoc />
        protected override DebtProductDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "kind", "annualRate", "rateSpread", "termMonths", "minPaymentRate",
                "minPayment", "limitIncomeMultiple", "minCreditScore", "maxIncomeMultiple", "collateralClass", "downPaymentRate",
                "dischargeable", "graceMonths", "lateFee");
            string collateral = ContentReading.RequireNullableId(item, "collateralClass", context);
            AssetClass? collateralClass = null;
            if (collateral != null)
            {
                collateralClass = ContentReading.RequireEnum(item, "collateralClass", context, EconomyNames.AssetClasses);
            }
            return new DebtProductDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                ContentReading.RequireEnum(item, "kind", context, EconomyNames.DebtKinds),
                ContentReading.RequireNullableFloat(item, "annualRate", context),
                ContentReading.RequireNullableFloat(item, "rateSpread", context),
                RuleJson.RequireInt(item, "termMonths", context),
                RuleJson.RequireFloat(item, "minPaymentRate", context),
                RuleJson.RequireDecimal(item, "minPayment", context),
                RuleJson.RequireFloat(item, "limitIncomeMultiple", context),
                RuleJson.RequireFloat(item, "minCreditScore", context),
                ContentReading.RequireNullableFloat(item, "maxIncomeMultiple", context),
                collateralClass,
                RuleJson.RequireFloat(item, "downPaymentRate", context),
                ContentReading.RequireBool(item, "dischargeable", context),
                RuleJson.RequireInt(item, "graceMonths", context),
                RuleJson.RequireDecimal(item, "lateFee", context));
        }
    }
}
