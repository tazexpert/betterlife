using System;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// A time-budget activity whose hours count as uses of a vice once they exceed a normal amount.
    /// </summary>
    public sealed class ViceActivityLink
    {
        /// <summary>
        /// Creates the link.
        /// </summary>
        /// <param name="activityId">Activity id.</param>
        /// <param name="freeHours">Weekly hours that are normal and do not count.</param>
        /// <param name="hoursPerUse">Hours above the free amount that make one use.</param>
        public ViceActivityLink(string activityId, int freeHours, int hoursPerUse)
        {
            if (freeHours < 0 || hoursPerUse < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(hoursPerUse), "Free hours must be non-negative and hours per use positive.");
            }
            ActivityId = activityId ?? throw new ArgumentNullException(nameof(activityId));
            FreeHours = freeHours;
            HoursPerUse = hoursPerUse;
        }

        /// <summary>Activity id.</summary>
        public string ActivityId { get; }

        /// <summary>Weekly hours that do not count.</summary>
        public int FreeHours { get; }

        /// <summary>Hours above the free amount that make one use.</summary>
        public int HoursPerUse { get; }

        /// <summary>
        /// Returns the uses that a week's hours make.
        /// </summary>
        /// <param name="hours">Hours spent this week.</param>
        /// <returns>Uses.</returns>
        public int UsesFor(int hours) => Math.Max(0, hours - FreeHours) / HoursPerUse;
    }
}
