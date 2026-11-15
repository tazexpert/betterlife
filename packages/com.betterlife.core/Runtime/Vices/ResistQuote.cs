using BetterLife.Core.Luck;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// The chance to resist a temptation, with the vice-specific adjustments shown separately.
    /// </summary>
    public sealed class ResistQuote
    {
        /// <summary>
        /// Creates the quote.
        /// </summary>
        /// <param name="chance">Final chance (adjustments included).</param>
        /// <param name="addictionAdjustment">Chance points from addiction.</param>
        /// <param name="supportAdjustment">Chance points from supportive people.</param>
        /// <param name="quitAdjustment">Chance points from the current quit method.</param>
        public ResistQuote(ChanceBreakdown chance, float addictionAdjustment, float supportAdjustment, float quitAdjustment)
        {
            Chance = chance;
            AddictionAdjustment = addictionAdjustment;
            SupportAdjustment = supportAdjustment;
            QuitAdjustment = quitAdjustment;
        }

        /// <summary>Final chance.</summary>
        public ChanceBreakdown Chance { get; }

        /// <summary>Chance points from addiction.</summary>
        public float AddictionAdjustment { get; }

        /// <summary>Chance points from supportive people.</summary>
        public float SupportAdjustment { get; }

        /// <summary>Chance points from the quit method.</summary>
        public float QuitAdjustment { get; }

        /// <summary>All adjustments together.</summary>
        public float TotalAdjustment => AddictionAdjustment + SupportAdjustment + QuitAdjustment;
    }
}
