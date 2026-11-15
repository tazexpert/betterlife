using System;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// Betting rules: a house edge with beginner's luck.
    /// </summary>
    public sealed class GamblingRules
    {
        /// <summary>
        /// Creates the value.
        /// </summary>
        /// <param name="stake">Base stake per bet (grows with addiction).</param>
        /// <param name="winChance">Chance to win a bet.</param>
        /// <param name="payout">Gross payout multiple of the stake on a win.</param>
        /// <param name="beginnerBets">Number of first bets with better odds.</param>
        /// <param name="beginnerWinChance">Win chance of the first bets.</param>
        public GamblingRules(DecimalRange stake, float winChance, decimal payout, int beginnerBets, float beginnerWinChance)
        {
            if (!(winChance >= 0f) || winChance > 1f || !(beginnerWinChance >= 0f) || beginnerWinChance > 1f || payout < 0m || beginnerBets < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stake), "Values are out of range.");
            }
            Stake = stake;
            WinChance = winChance;
            Payout = payout;
            BeginnerBets = beginnerBets;
            BeginnerWinChance = beginnerWinChance;
        }

        /// <summary>Base stake per bet (grows with addiction).</summary>
        public DecimalRange Stake { get; }

        /// <summary>Chance to win a bet.</summary>
        public float WinChance { get; }

        /// <summary>Gross payout multiple of the stake on a win.</summary>
        public decimal Payout { get; }

        /// <summary>Number of first bets with better odds.</summary>
        public int BeginnerBets { get; }

        /// <summary>Win chance of the first bets.</summary>
        public float BeginnerWinChance { get; }

        /// <summary>Expected value per baht staked after beginner's luck (negative for a house edge).</summary>
        public decimal ExpectedValuePerBaht => (decimal)WinChance * Payout - 1m;
    }
}
