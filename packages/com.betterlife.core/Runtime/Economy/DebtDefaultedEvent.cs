using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Published when a loan defaults after too many missed payments.
    /// </summary>
    public sealed class DebtDefaultedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the message.
        /// </summary>
        /// <param name="debt">The loan.</param>
        /// <param name="week">Week of default.</param>
        public DebtDefaultedEvent(Debt debt, GameDate week)
        {
            Debt = debt;
            Week = week;
        }

        /// <summary>The loan.</summary>
        public Debt Debt { get; }

        /// <summary>Week of default.</summary>
        public GameDate Week { get; }
    }
}
