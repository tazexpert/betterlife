namespace BetterLife.Core.Luck
{
    /// <summary>
    /// A rolled chance: the breakdown, the roll (0-100) and whether it succeeded.
    /// </summary>
    public sealed class ChanceRoll
    {
        /// <summary>
        /// Creates a roll.
        /// </summary>
        /// <param name="breakdown">Chance breakdown.</param>
        /// <param name="roll">Rolled value in [0, 100).</param>
        public ChanceRoll(ChanceBreakdown breakdown, float roll)
        {
            Breakdown = breakdown;
            Roll = roll;
        }

        /// <summary>Chance breakdown.</summary>
        public ChanceBreakdown Breakdown { get; }

        /// <summary>Rolled value in [0, 100).</summary>
        public float Roll { get; }

        /// <summary>True when the roll is below the final chance.</summary>
        public bool Success => Roll < Breakdown.Final;
    }
}
