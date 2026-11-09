using System;
using BetterLife.Core.Luck;

namespace BetterLife.Core.Scams
{
    /// <summary>
    /// Getting money back by reporting to the police and the bank.
    /// </summary>
    public sealed class ScamRecovery
    {
        /// <summary>
        /// Creates the value.
        /// </summary>
        /// <param name="chance">Chance when reported at once.</param>
        /// <param name="decayPerWeek">Chance points lost per week of delay.</param>
        /// <param name="windowWeeks">Weeks during which a report is accepted.</param>
        /// <param name="share">Share of the loss returned on success.</param>
        public ScamRecovery(ChanceSpec chance, float decayPerWeek, int windowWeeks, decimal share)
        {
            if (decayPerWeek < 0f || windowWeeks < 0 || share < 0m || share > 1m)
            {
                throw new ArgumentOutOfRangeException(nameof(chance), "Values are out of range.");
            }
            Chance = chance ?? throw new ArgumentNullException(nameof(chance));
            DecayPerWeek = decayPerWeek;
            WindowWeeks = windowWeeks;
            Share = share;
        }

        /// <summary>Chance when reported at once.</summary>
        public ChanceSpec Chance { get; }

        /// <summary>Chance points lost per week of delay.</summary>
        public float DecayPerWeek { get; }

        /// <summary>Weeks during which a report is accepted.</summary>
        public int WindowWeeks { get; }

        /// <summary>Share of the loss returned on success.</summary>
        public decimal Share { get; }
    }
}
