using System.Collections.Generic;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Events;

namespace BetterLife.Core.Time.Ticking.Phases
{
    /// <summary>
    /// Published after the weekly plan has been applied, for the weekly report.
    /// </summary>
    public sealed class TimeAllocationAppliedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="week">The week.</param>
        /// <param name="hours">Effective hours per activity.</param>
        /// <param name="statChanges">Stat changes caused by the plan.</param>
        /// <param name="cashSpent">Cash actually paid for activities.</param>
        public TimeAllocationAppliedEvent(GameDate week, IReadOnlyDictionary<string, int> hours, IReadOnlyList<StatChangeResult> statChanges, decimal cashSpent)
        {
            Week = week;
            Hours = hours;
            StatChanges = statChanges;
            CashSpent = cashSpent;
        }

        /// <summary>The week.</summary>
        public GameDate Week { get; }

        /// <summary>Effective hours per activity.</summary>
        public IReadOnlyDictionary<string, int> Hours { get; }

        /// <summary>Stat changes caused by the plan.</summary>
        public IReadOnlyList<StatChangeResult> StatChanges { get; }

        /// <summary>Cash actually paid for activities.</summary>
        public decimal CashSpent { get; }
    }
}
