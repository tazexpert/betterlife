using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Time;

namespace BetterLife.Core.Karma
{
    /// <summary>
    /// Part of a bad deed that comes back later.
    /// </summary>
    public sealed class KarmicDebt
    {
        /// <summary>
        /// Creates a debt.
        /// </summary>
        /// <param name="amount">Remaining amount; positive.</param>
        /// <param name="dueOn">Week the debt comes due.</param>
        /// <param name="sourceKey">Localization key of the deed.</param>
        public KarmicDebt(float amount, GameDate dueOn, string sourceKey)
        {
            if (!(amount > 0f) || float.IsInfinity(amount))
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Debt must be positive.");
            }
            Amount = amount;
            DueOn = dueOn;
            SourceKey = ContentId.RequireLocKey(sourceKey, nameof(sourceKey));
        }

        /// <summary>Remaining amount.</summary>
        public float Amount { get; private set; }

        /// <summary>Week the debt comes due.</summary>
        public GameDate DueOn { get; }

        /// <summary>Localization key of the deed.</summary>
        public string SourceKey { get; }

        internal float Reduce(float offset)
        {
            float used = Math.Min(offset, Amount);
            Amount -= used;
            return used;
        }
    }
}
