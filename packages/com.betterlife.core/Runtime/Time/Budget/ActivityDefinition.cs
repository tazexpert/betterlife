using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Time.Budget
{
    /// <summary>
    /// Something the player can spend weekly hours on, loaded from content/activities.json.
    /// </summary>
    public sealed class ActivityDefinition : IDefinition
    {
        /// <summary>
        /// Creates an activity.
        /// </summary>
        /// <param name="id">Activity id.</param>
        /// <param name="nameKey">Localization key of the name (also the stat change source).</param>
        /// <param name="descKey">Localization key of the description.</param>
        /// <param name="category">Category id.</param>
        /// <param name="minHours">Minimum weekly hours when available.</param>
        /// <param name="maxHours">Maximum weekly hours, at most a full week.</param>
        /// <param name="costPerHour">Cash cost per hour; non-negative.</param>
        /// <param name="condition">Availability requirement, or null for always available.</param>
        /// <param name="stats">Stat effects.</param>
        /// <param name="skills">Skill effects.</param>
        public ActivityDefinition(
            string id,
            string nameKey,
            string descKey,
            string category,
            int minHours,
            int maxHours,
            decimal costPerHour,
            ICondition condition,
            IEnumerable<ActivityStatEffect> stats,
            IEnumerable<ActivitySkillEffect> skills)
        {
            if (minHours < 0 || maxHours < minHours || maxHours > TimeBudget.HoursPerWeek)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHours), maxHours, $"Hours must satisfy 0 <= min <= max <= {TimeBudget.HoursPerWeek}.");
            }
            if (costPerHour < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(costPerHour), costPerHour, "Cost must be non-negative.");
            }

            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            DescKey = ContentId.RequireLocKey(descKey, nameof(descKey));
            Category = ContentId.Require(category, nameof(category));
            MinHours = minHours;
            MaxHours = maxHours;
            CostPerHour = costPerHour;
            Condition = condition;
            Stats = (stats ?? Enumerable.Empty<ActivityStatEffect>()).ToList();
            Skills = (skills ?? Enumerable.Empty<ActivitySkillEffect>()).ToList();
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Localization key of the description.</summary>
        public string DescKey { get; }

        /// <summary>Category id.</summary>
        public string Category { get; }

        /// <summary>Minimum weekly hours when available.</summary>
        public int MinHours { get; }

        /// <summary>Maximum weekly hours.</summary>
        public int MaxHours { get; }

        /// <summary>Cash cost per hour.</summary>
        public decimal CostPerHour { get; }

        /// <summary>Availability requirement, or null.</summary>
        public ICondition Condition { get; }

        /// <summary>Stat effects.</summary>
        public IReadOnlyList<ActivityStatEffect> Stats { get; }

        /// <summary>Skill effects.</summary>
        public IReadOnlyList<ActivitySkillEffect> Skills { get; }

        /// <summary>
        /// Returns true when the character may do this activity now.
        /// </summary>
        /// <param name="context">Rule context.</param>
        /// <returns>True when available.</returns>
        public bool IsAvailable(RuleContext context) => Condition == null || Condition.Evaluate(context);
    }
}
