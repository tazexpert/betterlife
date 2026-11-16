using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Rules;
using BetterLife.Core.Time;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// The chance that a life milestone happens once within an age range (for example 25% between 13 and 18). Each week
    /// in the range rolls the weekly share of that chance, so a character who lives through the whole range meets the
    /// milestone with the given overall chance.
    /// </summary>
    public sealed class MilestoneChance
    {
        /// <summary>Highest chance in percent.</summary>
        public const float MaxPercent = 100f;

        /// <summary>
        /// Creates the chance.
        /// </summary>
        /// <param name="minAge">First age of the range.</param>
        /// <param name="maxAge">Last age of the range (inclusive).</param>
        /// <param name="percent">Chance over the whole range, 0..100.</param>
        /// <param name="modifiers">Conditional changes in percentage points.</param>
        public MilestoneChance(int minAge, int maxAge, float percent, IEnumerable<MilestoneModifier> modifiers)
        {
            if (minAge < 0 || maxAge < minAge)
            {
                throw new ArgumentOutOfRangeException(nameof(maxAge), maxAge, "The age range is invalid.");
            }
            if (!(percent >= 0f) || percent > MaxPercent)
            {
                throw new ArgumentOutOfRangeException(nameof(percent), percent, "Chance must be within 0-100.");
            }
            MinAge = minAge;
            MaxAge = maxAge;
            Percent = percent;
            Modifiers = (modifiers ?? Enumerable.Empty<MilestoneModifier>()).ToList();
        }

        /// <summary>First age of the range.</summary>
        public int MinAge { get; }

        /// <summary>Last age of the range (inclusive).</summary>
        public int MaxAge { get; }

        /// <summary>Chance over the whole range, 0..100.</summary>
        public float Percent { get; }

        /// <summary>Conditional changes in percentage points.</summary>
        public IReadOnlyList<MilestoneModifier> Modifiers { get; }

        /// <summary>Weeks in the age range.</summary>
        public int RangeWeeks => (MaxAge - MinAge + 1) * GameCalendar.WeeksPerYear;

        /// <summary>
        /// Whether an age is inside the range.
        /// </summary>
        /// <param name="age">Age in years.</param>
        /// <returns>True inside the range.</returns>
        public bool Covers(int age) => age >= MinAge && age <= MaxAge;

        /// <summary>
        /// The overall chance now: base plus the modifiers that hold, kept within 0..100.
        /// </summary>
        /// <param name="context">Rule context of the character.</param>
        /// <returns>Chance in percent.</returns>
        public float PercentNow(RuleContext context)
        {
            float total = Percent + Modifiers.Where(m => m.Condition.Evaluate(context)).Sum(m => m.Add);
            return Math.Max(0f, Math.Min(MaxPercent, total));
        }

        /// <summary>
        /// The chance of one week so that the whole range adds up to <paramref name="percent"/>:
        /// 1 − (1 − P)^(1 / weeks). A certain milestone (100%) happens in the first week it can.
        /// </summary>
        /// <param name="percent">Overall chance in percent.</param>
        /// <param name="rangeWeeks">Weeks in the range.</param>
        /// <returns>Weekly chance 0..1.</returns>
        public static float WeeklyChance(float percent, int rangeWeeks)
        {
            float p = percent / MaxPercent;
            if (p >= 1f)
            {
                return 1f;
            }
            if (p <= 0f || rangeWeeks <= 0)
            {
                return 0f;
            }
            return (float)(1.0 - Math.Pow(1.0 - p, 1.0 / rangeWeeks));
        }
    }
}
