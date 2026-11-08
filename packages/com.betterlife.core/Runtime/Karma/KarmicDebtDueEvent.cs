using System;
using BetterLife.Core.Events;

namespace BetterLife.Core.Karma
{
    /// <summary>
    /// Published when a karmic debt comes due. The event system picks a misfortune scaled by the amount.
    /// </summary>
    public sealed class KarmicDebtDueEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="debt">The due debt.</param>
        public KarmicDebtDueEvent(KarmicDebt debt)
        {
            Debt = debt ?? throw new ArgumentNullException(nameof(debt));
        }

        /// <summary>The due debt.</summary>
        public KarmicDebt Debt { get; }
    }
}
