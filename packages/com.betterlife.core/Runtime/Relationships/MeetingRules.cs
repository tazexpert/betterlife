using System;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Monthly chances to meet new people.
    /// </summary>
    public sealed class MeetingRules
    {
        /// <summary>
        /// Creates the rules.
        /// </summary>
        /// <param name="monthlyBase">Base chance to make a friend.</param>
        /// <param name="perSocialHour">Extra chance per socialize hour in the month.</param>
        /// <param name="school">Chance to meet a classmate while enrolled.</param>
        /// <param name="work">Chance to meet a colleague while employed.</param>
        /// <param name="startValue">Value of a new relationship.</param>
        /// <param name="ageGapYears">Largest age gap of new people.</param>
        public MeetingRules(float monthlyBase, float perSocialHour, float school, float work, FloatRange startValue, int ageGapYears)
        {
            if (monthlyBase < 0f || perSocialHour < 0f || school < 0f || work < 0f || ageGapYears < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(monthlyBase), "Values are out of range.");
            }
            MonthlyBase = monthlyBase;
            PerSocialHour = perSocialHour;
            School = school;
            Work = work;
            StartValue = startValue;
            AgeGapYears = ageGapYears;
        }

        /// <summary>Base chance to make a friend.</summary>
        public float MonthlyBase { get; }

        /// <summary>Extra chance per socialize hour in the month.</summary>
        public float PerSocialHour { get; }

        /// <summary>Chance to meet a classmate while enrolled.</summary>
        public float School { get; }

        /// <summary>Chance to meet a colleague while employed.</summary>
        public float Work { get; }

        /// <summary>Value of a new relationship.</summary>
        public FloatRange StartValue { get; }

        /// <summary>Largest age gap of new people.</summary>
        public int AgeGapYears { get; }
    }
}
