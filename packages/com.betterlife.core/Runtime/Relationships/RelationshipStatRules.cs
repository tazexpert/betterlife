using System;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// How relationships set the Relationships core stat and loneliness.
    /// </summary>
    public sealed class RelationshipStatRules
    {
        /// <summary>
        /// Creates the rules.
        /// </summary>
        /// <param name="closeValue">Value from which someone counts as close.</param>
        /// <param name="topCount">How many of the best relationships set the score.</param>
        /// <param name="emptySlotValue">Value counted for missing top slots.</param>
        /// <param name="monthlySmoothing">Share of the gap to the target closed each month.</param>
        /// <param name="lonelinessBase">Loneliness target with no close ties.</param>
        /// <param name="lonelinessPerCloseTie">Loneliness change per close tie.</param>
        public RelationshipStatRules(float closeValue, int topCount, float emptySlotValue, float monthlySmoothing, float lonelinessBase, float lonelinessPerCloseTie)
        {
            if (topCount < 1 || !(monthlySmoothing > 0f) || monthlySmoothing > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(closeValue), "Values are out of range.");
            }
            CloseValue = closeValue;
            TopCount = topCount;
            EmptySlotValue = emptySlotValue;
            MonthlySmoothing = monthlySmoothing;
            LonelinessBase = lonelinessBase;
            LonelinessPerCloseTie = lonelinessPerCloseTie;
        }

        /// <summary>Value from which someone counts as close.</summary>
        public float CloseValue { get; }

        /// <summary>How many of the best relationships set the score.</summary>
        public int TopCount { get; }

        /// <summary>Value counted for missing top slots.</summary>
        public float EmptySlotValue { get; }

        /// <summary>Share of the gap to the target closed each month.</summary>
        public float MonthlySmoothing { get; }

        /// <summary>Loneliness target with no close ties.</summary>
        public float LonelinessBase { get; }

        /// <summary>Loneliness change per close tie.</summary>
        public float LonelinessPerCloseTie { get; }
    }
}
