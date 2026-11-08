using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Time.Budget
{
    /// <summary>
    /// The player's weekly plan: hours per activity. Hours not allocated go to the default activity,
    /// so the effective plan always covers exactly <see cref="HoursPerWeek"/> hours.
    /// </summary>
    public sealed class TimeBudget
    {
        /// <summary>Hours in one week (24 × 7).</summary>
        public const int HoursPerWeek = 168;

        private readonly DefinitionCatalog<ActivityDefinition> _activities;
        private readonly TimeSettings _settings;
        private readonly Dictionary<string, int> _hours = new Dictionary<string, int>(StringComparer.Ordinal);

        /// <summary>
        /// Creates an empty budget (all hours free).
        /// </summary>
        /// <param name="activities">Activity catalog containing the default activity.</param>
        /// <param name="settings">Time settings.</param>
        public TimeBudget(DefinitionCatalog<ActivityDefinition> activities, TimeSettings settings)
        {
            _activities = activities ?? throw new ArgumentNullException(nameof(activities));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _activities.Get(settings.DefaultActivityId);
        }

        /// <summary>Explicitly allocated hours (the default activity is never listed).</summary>
        public IReadOnlyDictionary<string, int> Allocations => _hours;

        /// <summary>Hours not explicitly allocated (spent on the default activity). Negative when over-allocated.</summary>
        public int FreeHours => HoursPerWeek - _hours.Values.Sum();

        /// <summary>The default activity id.</summary>
        public string DefaultActivityId => _settings.DefaultActivityId;

        /// <summary>
        /// Creates a budget filled with the settings' default allocation.
        /// </summary>
        /// <param name="activities">Activity catalog.</param>
        /// <param name="settings">Time settings.</param>
        /// <returns>The budget.</returns>
        public static TimeBudget CreateDefault(DefinitionCatalog<ActivityDefinition> activities, TimeSettings settings)
        {
            var budget = new TimeBudget(activities, settings);
            foreach (KeyValuePair<string, int> pair in settings.DefaultBudget)
            {
                budget.Set(pair.Key, pair.Value);
            }
            return budget;
        }

        /// <summary>
        /// Returns the hours spent on an activity (free hours for the default activity).
        /// </summary>
        /// <param name="activityId">Known activity id.</param>
        /// <returns>Hours.</returns>
        public int GetHours(string activityId)
        {
            _activities.Get(activityId);
            if (activityId == DefaultActivityId)
            {
                return Math.Max(0, FreeHours);
            }
            return _hours.TryGetValue(activityId, out int hours) ? hours : 0;
        }

        /// <summary>
        /// Sets the hours of an activity. Fails (returns false, no change) when the activity's maximum or
        /// the week would be exceeded. The default activity cannot be set; it takes what is left.
        /// </summary>
        /// <param name="activityId">Known activity id other than the default.</param>
        /// <param name="hours">Hours, non-negative.</param>
        /// <returns>True when applied.</returns>
        public bool Set(string activityId, int hours)
        {
            ActivityDefinition activity = _activities.Get(activityId);
            if (activityId == DefaultActivityId)
            {
                throw new InvalidOperationException("The default activity receives the remaining hours and cannot be set.");
            }
            if (hours < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(hours), hours, "Hours must be non-negative.");
            }

            int current = _hours.TryGetValue(activityId, out int existing) ? existing : 0;
            if (hours > activity.MaxHours || FreeHours + current - hours < 0)
            {
                return false;
            }

            if (hours == 0)
            {
                _hours.Remove(activityId);
            }
            else
            {
                _hours[activityId] = hours;
            }
            return true;
        }

        /// <summary>
        /// Restores saved hours without limit checks (validation reports problems later).
        /// </summary>
        /// <param name="activityId">Known activity id other than the default.</param>
        /// <param name="hours">Positive hours.</param>
        public void Restore(string activityId, int hours)
        {
            _activities.Get(activityId);
            if (activityId == DefaultActivityId || hours <= 0)
            {
                return;
            }
            _hours[activityId] = hours;
        }

        /// <summary>
        /// Lists every problem with the plan for the current character state.
        /// </summary>
        /// <param name="context">Rule context used to check availability.</param>
        /// <returns>Problems, empty when valid.</returns>
        public IReadOnlyList<BudgetIssue> Validate(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var issues = new List<BudgetIssue>();
            if (FreeHours < 0)
            {
                issues.Add(new BudgetIssue(BudgetIssueKind.OverWeek, null, HoursPerWeek));
            }
            foreach (ActivityDefinition activity in _activities.All)
            {
                int hours = GetHours(activity.Id);
                bool available = activity.IsAvailable(context);
                if (!available)
                {
                    if (hours > 0 && activity.Id != DefaultActivityId)
                    {
                        issues.Add(new BudgetIssue(BudgetIssueKind.Unavailable, activity.Id, 0));
                    }
                    continue;
                }
                if (hours < activity.MinHours)
                {
                    issues.Add(new BudgetIssue(BudgetIssueKind.BelowMinimum, activity.Id, activity.MinHours));
                }
                if (hours > activity.MaxHours)
                {
                    issues.Add(new BudgetIssue(BudgetIssueKind.AboveMaximum, activity.Id, activity.MaxHours));
                }
            }
            return issues;
        }

        /// <summary>
        /// Returns the hours that actually take effect this week: unavailable activities lose their hours to the
        /// default activity, and over-allocation is trimmed from the end of the plan so the total is exactly one week.
        /// </summary>
        /// <param name="context">Rule context used to check availability.</param>
        /// <returns>Activity id to hours, including the default activity, summing to <see cref="HoursPerWeek"/>.</returns>
        public IReadOnlyDictionary<string, int> EffectiveAllocation(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var result = new Dictionary<string, int>(StringComparer.Ordinal);
            int remaining = HoursPerWeek;
            foreach (ActivityDefinition activity in _activities.All)
            {
                if (activity.Id == DefaultActivityId || !_hours.TryGetValue(activity.Id, out int hours) || !activity.IsAvailable(context))
                {
                    continue;
                }
                int granted = Math.Min(Math.Min(hours, activity.MaxHours), remaining);
                if (granted > 0)
                {
                    result[activity.Id] = granted;
                    remaining -= granted;
                }
            }
            result[DefaultActivityId] = remaining;
            return result;
        }
    }
}
