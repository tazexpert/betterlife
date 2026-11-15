using System.Collections.Generic;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// One activity of the weekly plan.
    /// </summary>
    public sealed class ActivityRow
    {
        /// <summary>
        /// Creates a row.
        /// </summary>
        /// <param name="id">Activity id.</param>
        /// <param name="name">Activity name.</param>
        /// <param name="description">What it does.</param>
        /// <param name="hours">Hours per week.</param>
        /// <param name="hoursText">Hours text.</param>
        /// <param name="isEditable">Whether the player sets it (not the default activity, school or work, and available).</param>
        /// <param name="note">Why it cannot be edited, or empty.</param>
        /// <param name="maxHours">Most hours it can be set to now (its maximum or what the week still allows).</param>
        /// <param name="effects">What these hours do each week.</param>
        public ActivityRow(string id, string name, string description, int hours, string hoursText, bool isEditable, string note, int maxHours,
            IReadOnlyList<string> effects)
        {
            Id = id;
            Name = name;
            Description = description;
            Hours = hours;
            HoursText = hoursText;
            IsEditable = isEditable;
            Note = note ?? string.Empty;
            MaxHours = maxHours;
            Effects = effects;
        }

        /// <summary>Activity id.</summary>
        public string Id { get; }

        /// <summary>Activity name.</summary>
        public string Name { get; }

        /// <summary>What it does.</summary>
        public string Description { get; }

        /// <summary>Hours per week.</summary>
        public int Hours { get; }

        /// <summary>Hours text.</summary>
        public string HoursText { get; }

        /// <summary>Whether the player sets it.</summary>
        public bool IsEditable { get; }

        /// <summary>Why it cannot be edited, or empty.</summary>
        public string Note { get; }

        /// <summary>Most hours it can be set to now.</summary>
        public int MaxHours { get; }

        /// <summary>What these hours do each week.</summary>
        public IReadOnlyList<string> Effects { get; }

        /// <summary>
        /// Hours after a step, kept between 0 and <see cref="MaxHours"/>.
        /// </summary>
        /// <param name="step">Step (negative to remove hours).</param>
        /// <returns>The target hours.</returns>
        public int Stepped(int step) => System.Math.Max(0, System.Math.Min(MaxHours, Hours + step));
    }
}
