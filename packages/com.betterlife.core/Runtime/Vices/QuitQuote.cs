using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// Whether the character can start quitting a vice with a method.
    /// </summary>
    public sealed class QuitQuote
    {
        /// <summary>Already quitting this vice.</summary>
        public const string ReasonAlreadyQuitting = "vice.quit.already";
        /// <summary>The method does not work for this vice.</summary>
        public const string ReasonMethod = "vice.quit.method";
        /// <summary>Nothing to quit yet.</summary>
        public const string ReasonNotUsing = "vice.quit.not_using";
        /// <summary>Not enough cash.</summary>
        public const string ReasonCash = "vice.quit.cash";
        /// <summary>The method's condition is not met.</summary>
        public const string ReasonCondition = "vice.quit.condition";

        /// <summary>
        /// Creates the quote.
        /// </summary>
        /// <param name="reasonKeys">Why not; empty when available.</param>
        /// <param name="cost">Cash cost.</param>
        /// <param name="durationWeeks">Weeks without a lapse needed.</param>
        public QuitQuote(IReadOnlyList<string> reasonKeys, decimal cost, int durationWeeks)
        {
            ReasonKeys = (reasonKeys ?? throw new ArgumentNullException(nameof(reasonKeys))).ToList();
            Cost = cost;
            DurationWeeks = durationWeeks;
        }

        /// <summary>True when available.</summary>
        public bool Available => ReasonKeys.Count == 0;

        /// <summary>Why not.</summary>
        public IReadOnlyList<string> ReasonKeys { get; }

        /// <summary>Cash cost.</summary>
        public decimal Cost { get; }

        /// <summary>Weeks without a lapse needed.</summary>
        public int DurationWeeks { get; }
    }
}
