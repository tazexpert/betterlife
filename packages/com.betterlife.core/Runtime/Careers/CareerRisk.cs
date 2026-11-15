using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// A monthly danger of a risky or illegal career (injury, scandal, arrest...).
    /// </summary>
    public sealed class CareerRisk
    {
        /// <summary>
        /// Creates the risk.
        /// </summary>
        /// <param name="monthlyChance">Chance per month, 0-1.</param>
        /// <param name="endsJob">True when it ends the job.</param>
        /// <param name="reasonKey">Localization key describing what happened.</param>
        /// <param name="effects">Rule effects applied when it happens.</param>
        public CareerRisk(float monthlyChance, bool endsJob, string reasonKey, EffectSequence effects)
        {
            if (!(monthlyChance >= 0f) || monthlyChance > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(monthlyChance), monthlyChance, "Chance must be within 0-1.");
            }
            MonthlyChance = monthlyChance;
            EndsJob = endsJob;
            ReasonKey = ContentId.RequireLocKey(reasonKey, nameof(reasonKey));
            Effects = effects ?? throw new ArgumentNullException(nameof(effects));
        }

        /// <summary>Chance per month.</summary>
        public float MonthlyChance { get; }

        /// <summary>True when it ends the job.</summary>
        public bool EndsJob { get; }

        /// <summary>Localization key describing what happened.</summary>
        public string ReasonKey { get; }

        /// <summary>Effects applied when it happens.</summary>
        public EffectSequence Effects { get; }
    }
}
