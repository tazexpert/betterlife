using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// A big purchase the player can make (a condo, a family car): the asset it becomes, its price, the loan that can
    /// finance it and what owning it does at once.
    /// </summary>
    public sealed class PurchaseDefinition : IDefinition
    {
        /// <summary>
        /// Creates the definition.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <param name="nameKey">Name key.</param>
        /// <param name="descKey">Description key.</param>
        /// <param name="category">Group.</param>
        /// <param name="assetId">Asset the purchase becomes in the portfolio.</param>
        /// <param name="price">Price in real money; at least the asset's minimum purchase.</param>
        /// <param name="loanProductId">Secured loan that can finance it.</param>
        /// <param name="effects">Effects applied when bought.</param>
        public PurchaseDefinition(string id, string nameKey, string descKey, PurchaseCategory category, string assetId, decimal price,
            string loanProductId, EffectSequence effects)
        {
            if (price <= 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(price), price, "Price must be positive.");
            }
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            DescKey = ContentId.RequireLocKey(descKey, nameof(descKey));
            Category = category;
            AssetId = ContentId.Require(assetId, nameof(assetId));
            Price = price;
            LoanProductId = ContentId.Require(loanProductId, nameof(loanProductId));
            Effects = effects ?? throw new ArgumentNullException(nameof(effects));
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Name key.</summary>
        public string NameKey { get; }

        /// <summary>Description key.</summary>
        public string DescKey { get; }

        /// <summary>Group.</summary>
        public PurchaseCategory Category { get; }

        /// <summary>Asset the purchase becomes in the portfolio.</summary>
        public string AssetId { get; }

        /// <summary>Price in real money.</summary>
        public decimal Price { get; }

        /// <summary>Secured loan that can finance it.</summary>
        public string LoanProductId { get; }

        /// <summary>Effects applied when bought.</summary>
        public EffectSequence Effects { get; }
    }
}
