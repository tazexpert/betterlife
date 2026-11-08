using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;

namespace BetterLife.Core.Time.Ticking
{
    /// <summary>
    /// State shared by the phases of one week tick.
    /// </summary>
    public sealed class TickContext
    {
        private readonly List<InterruptRequest> _interrupts = new List<InterruptRequest>();

        /// <summary>
        /// Creates a context.
        /// </summary>
        /// <param name="week">The week being simulated.</param>
        /// <param name="character">The player character.</param>
        public TickContext(GameDate week, Character character)
        {
            Week = week;
            Character = character ?? throw new ArgumentNullException(nameof(character));
        }

        /// <summary>The week being simulated.</summary>
        public GameDate Week { get; }

        /// <summary>The player character.</summary>
        public Character Character { get; }

        /// <summary>True in the first week of a month.</summary>
        public bool IsMonthStart => Week.IsMonthStart;

        /// <summary>True in the first week of a year.</summary>
        public bool IsYearStart => Week.IsYearStart;

        /// <summary>Interrupt requests made so far this week.</summary>
        public IReadOnlyList<InterruptRequest> Interrupts => _interrupts;

        /// <summary>
        /// Asks to stop time skipping after this week finishes.
        /// </summary>
        /// <param name="severity">Importance.</param>
        /// <param name="reasonKey">Localization key of the cause.</param>
        /// <param name="sourceId">Id of the cause.</param>
        public void RequestInterrupt(Severity severity, string reasonKey, string sourceId)
        {
            _interrupts.Add(new InterruptRequest(severity, reasonKey, sourceId, Week));
        }

        /// <summary>
        /// Returns true when a phase with this cadence runs this week.
        /// </summary>
        /// <param name="cadence">Phase cadence.</param>
        /// <returns>True when it runs.</returns>
        public bool Runs(TickCadence cadence)
        {
            switch (cadence)
            {
                case TickCadence.Weekly:
                    return true;
                case TickCadence.Monthly:
                    return IsMonthStart;
                case TickCadence.Yearly:
                    return IsYearStart;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cadence), cadence, "Unknown cadence.");
            }
        }
    }
}
