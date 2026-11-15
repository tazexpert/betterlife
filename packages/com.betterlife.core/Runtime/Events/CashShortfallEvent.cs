using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Time;

namespace BetterLife.Core.Events
{
    /// <summary>
    /// Published when a payment could not be covered by cash. The Economy system turns it into debt.
    /// </summary>
    public sealed class CashShortfallEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="amount">Unpaid amount; positive.</param>
        /// <param name="sourceKey">Localization key of what was being paid for.</param>
        /// <param name="week">Week of the payment.</param>
        public CashShortfallEvent(decimal amount, string sourceKey, GameDate week)
        {
            if (amount <= 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Shortfall must be positive.");
            }
            Amount = amount;
            SourceKey = ContentId.RequireLocKey(sourceKey, nameof(sourceKey));
            Week = week;
        }

        /// <summary>Unpaid amount.</summary>
        public decimal Amount { get; }

        /// <summary>What was being paid for.</summary>
        public string SourceKey { get; }

        /// <summary>Week of the payment.</summary>
        public GameDate Week { get; }
    }
}
