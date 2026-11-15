using System.Collections.Generic;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Something the player can act on (an asset, a holding, a loan product, a debt, a habit) with its buttons.
    /// </summary>
    public sealed class OfferGroup
    {
        /// <summary>
        /// Creates an offer.
        /// </summary>
        /// <param name="id">Id of the thing acted on (asset id, holding id, product id, debt id, vice id).</param>
        /// <param name="title">Title.</param>
        /// <param name="detail">Terms or status, or empty.</param>
        /// <param name="choices">Buttons.</param>
        public OfferGroup(string id, string title, string detail, IReadOnlyList<AmountChoice> choices)
        {
            Id = id;
            Title = title;
            Detail = detail ?? string.Empty;
            Choices = choices;
        }

        /// <summary>Id of the thing acted on.</summary>
        public string Id { get; }

        /// <summary>Title.</summary>
        public string Title { get; }

        /// <summary>Terms or status, or empty.</summary>
        public string Detail { get; }

        /// <summary>Buttons.</summary>
        public IReadOnlyList<AmountChoice> Choices { get; }
    }
}
