using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Published when collateral is seized and sold.
    /// </summary>
    public sealed class ForeclosureEvent : IGameEvent
    {
        /// <summary>
        /// Creates the message.
        /// </summary>
        /// <param name="debt">The loan.</param>
        /// <param name="assetId">Seized asset.</param>
        /// <param name="saleProceeds">Amount raised.</param>
        /// <param name="week">Week of the seizure.</param>
        public ForeclosureEvent(Debt debt, string assetId, decimal saleProceeds, GameDate week)
        {
            Debt = debt;
            AssetId = assetId;
            SaleProceeds = saleProceeds;
            Week = week;
        }

        /// <summary>The loan.</summary>
        public Debt Debt { get; }

        /// <summary>Seized asset.</summary>
        public string AssetId { get; }

        /// <summary>Amount raised.</summary>
        public decimal SaleProceeds { get; }

        /// <summary>Week of the seizure.</summary>
        public GameDate Week { get; }
    }
}
