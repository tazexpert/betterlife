using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Budget;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// Converts <see cref="TimeState"/> to and from <see cref="TimeStateDto"/>.
    /// </summary>
    public static class TimeStateDtoMapper
    {
        /// <summary>
        /// Captures a time state.
        /// </summary>
        /// <param name="state">State to save.</param>
        /// <returns>The DTO.</returns>
        public static TimeStateDto ToDto(TimeState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }
            return new TimeStateDto
            {
                CurrentWeek = state.Clock.Now.TotalWeeks,
                Allocations = state.Budget.Allocations
                    .Select(p => new TimeAllocationDto { ActivityId = p.Key, Hours = p.Value })
                    .OrderBy(a => a.ActivityId, StringComparer.Ordinal)
                    .ToList(),
            };
        }

        /// <summary>
        /// Rebuilds a time state. Allocations of removed or invalid activities are skipped with a warning.
        /// </summary>
        /// <param name="dto">Saved state.</param>
        /// <param name="activities">Current activity catalog.</param>
        /// <param name="settings">Current time settings.</param>
        /// <returns>The state and warnings.</returns>
        public static TimeStateLoadResult FromDto(TimeStateDto dto, DefinitionCatalog<ActivityDefinition> activities, TimeSettings settings)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var warnings = new List<string>();
            var budget = new TimeBudget(activities, settings);
            foreach (TimeAllocationDto allocation in (dto.Allocations ?? new List<TimeAllocationDto>()).Where(a => a != null))
            {
                if (!activities.Contains(allocation.ActivityId) || allocation.ActivityId == settings.DefaultActivityId || allocation.Hours <= 0)
                {
                    warnings.Add($"Skipped allocation '{allocation.ActivityId}' ({allocation.Hours} h).");
                    continue;
                }
                budget.Restore(allocation.ActivityId, allocation.Hours);
            }
            return new TimeStateLoadResult(new TimeState(new GameClock(new GameDate(dto.CurrentWeek)), budget), warnings);
        }
    }
}
