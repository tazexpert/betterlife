using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Published when the character is declared bankrupt.
    /// </summary>
    public sealed class BankruptcyEvent : IGameEvent
    {
        /// <summary>
        /// Creates the message.
        /// </summary>
        /// <param name="dischargedDebt">Debt erased.</param>
        /// <param name="until">End of the bankruptcy period.</param>
        public BankruptcyEvent(decimal dischargedDebt, GameDate until)
        {
            DischargedDebt = dischargedDebt;
            Until = until;
        }

        /// <summary>Debt erased.</summary>
        public decimal DischargedDebt { get; }

        /// <summary>End of the bankruptcy period.</summary>
        public GameDate Until { get; }
    }
}
