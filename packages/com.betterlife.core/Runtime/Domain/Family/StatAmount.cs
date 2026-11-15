using System;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Domain.Family
{
    /// <summary>
    /// A fixed stat change applied at birth.
    /// </summary>
    public sealed class StatAmount
    {
        /// <summary>
        /// Creates an amount.
        /// </summary>
        /// <param name="statId">Stored stat.</param>
        /// <param name="amount">Finite change.</param>
        public StatAmount(StatId statId, float amount)
        {
            if (float.IsNaN(amount) || float.IsInfinity(amount))
            {
                throw new ArgumentException("Amount must be finite.", nameof(amount));
            }
            StatId = statId;
            Amount = amount;
        }

        /// <summary>Stored stat.</summary>
        public StatId StatId { get; }

        /// <summary>Change.</summary>
        public float Amount { get; }
    }
}
