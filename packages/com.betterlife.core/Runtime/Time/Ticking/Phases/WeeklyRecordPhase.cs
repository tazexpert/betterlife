using System;

namespace BetterLife.Core.Time.Ticking.Phases
{
    /// <summary>
    /// Records the week's core stats for sparklines after every other phase, and trims old change log entries.
    /// </summary>
    public sealed class WeeklyRecordPhase : ITickPhase
    {
        /// <inheritdoc />
        public string Name => "weekly_record";

        /// <inheritdoc />
        public int Order => TickOrder.Record;

        /// <inheritdoc />
        public TickCadence Cadence => TickCadence.Weekly;

        /// <inheritdoc />
        public void Execute(TickContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.Character.RecordWeeklyHistory(context.Week);
        }
    }
}
