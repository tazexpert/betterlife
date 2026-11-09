using BetterLife.Core.Luck;

namespace BetterLife.Core.Scams
{
    /// <summary>
    /// The chance to spot a scam, with the situational adjustments shown separately.
    /// </summary>
    public sealed class ScamDetectionQuote
    {
        /// <summary>
        /// Creates the quote.
        /// </summary>
        /// <param name="chance">Final chance (adjustments included).</param>
        /// <param name="stressAdjustment">Chance points from stress.</param>
        /// <param name="supportAdjustment">Chance points from people who warn the character.</param>
        public ScamDetectionQuote(ChanceBreakdown chance, float stressAdjustment, float supportAdjustment)
        {
            Chance = chance;
            StressAdjustment = stressAdjustment;
            SupportAdjustment = supportAdjustment;
        }

        /// <summary>Final chance.</summary>
        public ChanceBreakdown Chance { get; }

        /// <summary>Chance points from stress.</summary>
        public float StressAdjustment { get; }

        /// <summary>Chance points from supportive people.</summary>
        public float SupportAdjustment { get; }
    }
}
