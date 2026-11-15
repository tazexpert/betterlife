using System;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// When unhappy partners leave on their own.
    /// </summary>
    public sealed class RomanceRules
    {
        /// <summary>
        /// Creates the rules.
        /// </summary>
        /// <param name="breakupBelow">Value below which a partner counts unhappy months.</param>
        /// <param name="breakupAfterMonths">Unhappy months before a partner leaves.</param>
        /// <param name="divorceBelow">Value below which a spouse counts unhappy months.</param>
        /// <param name="divorceAfterMonths">Unhappy months before a spouse files for divorce.</param>
        public RomanceRules(float breakupBelow, int breakupAfterMonths, float divorceBelow, int divorceAfterMonths)
        {
            if (breakupAfterMonths < 1 || divorceAfterMonths < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(breakupBelow), "Values are out of range.");
            }
            BreakupBelow = breakupBelow;
            BreakupAfterMonths = breakupAfterMonths;
            DivorceBelow = divorceBelow;
            DivorceAfterMonths = divorceAfterMonths;
        }

        /// <summary>Value below which a partner counts unhappy months.</summary>
        public float BreakupBelow { get; }

        /// <summary>Unhappy months before a partner leaves.</summary>
        public int BreakupAfterMonths { get; }

        /// <summary>Value below which a spouse counts unhappy months.</summary>
        public float DivorceBelow { get; }

        /// <summary>Unhappy months before a spouse files for divorce.</summary>
        public int DivorceAfterMonths { get; }
    }
}
