using System;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// How social time feeds relationships each month.
    /// </summary>
    public sealed class AttentionRules
    {
        /// <summary>
        /// Creates the rules.
        /// </summary>
        /// <param name="gainPerHour">Points per socialize hour, shared by the closest people.</param>
        /// <param name="closestCount">How many people share the attention.</param>
        /// <param name="familyMonthlyGain">Monthly gain with family while living with them.</param>
        public AttentionRules(float gainPerHour, int closestCount, float familyMonthlyGain)
        {
            if (gainPerHour < 0f || closestCount < 1 || familyMonthlyGain < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(gainPerHour), "Values are out of range.");
            }
            GainPerHour = gainPerHour;
            ClosestCount = closestCount;
            FamilyMonthlyGain = familyMonthlyGain;
        }

        /// <summary>Points per socialize hour, shared by the closest people.</summary>
        public float GainPerHour { get; }

        /// <summary>How many people share the attention.</summary>
        public int ClosestCount { get; }

        /// <summary>Monthly gain with family while living with them.</summary>
        public float FamilyMonthlyGain { get; }
    }
}
