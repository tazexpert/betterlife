using System.Collections.Generic;

namespace BetterLife.Core.Time.Ticking
{
    /// <summary>
    /// Outcome of a time skip.
    /// </summary>
    public sealed class AdvanceResult
    {
        /// <summary>
        /// Creates a result.
        /// </summary>
        /// <param name="from">Week before the skip.</param>
        /// <param name="to">Week after the skip.</param>
        /// <param name="requestedWeeks">Weeks the player asked for.</param>
        /// <param name="interrupts">Every interrupt request made during the skip, in order.</param>
        /// <param name="stoppedBy">The request that stopped the skip early, or null.</param>
        public AdvanceResult(GameDate from, GameDate to, int requestedWeeks, IReadOnlyList<InterruptRequest> interrupts, InterruptRequest stoppedBy)
        {
            From = from;
            To = to;
            RequestedWeeks = requestedWeeks;
            Interrupts = interrupts ?? new InterruptRequest[0];
            StoppedBy = stoppedBy;
        }

        /// <summary>Week before the skip.</summary>
        public GameDate From { get; }

        /// <summary>Week after the skip.</summary>
        public GameDate To { get; }

        /// <summary>Weeks the player asked for.</summary>
        public int RequestedWeeks { get; }

        /// <summary>Weeks actually simulated.</summary>
        public int WeeksAdvanced => From.WeeksUntil(To);

        /// <summary>Every interrupt request made during the skip.</summary>
        public IReadOnlyList<InterruptRequest> Interrupts { get; }

        /// <summary>The request that stopped the skip early, or null.</summary>
        public InterruptRequest StoppedBy { get; }

        /// <summary>True when the skip stopped before the requested weeks.</summary>
        public bool WasInterrupted => StoppedBy != null;
    }
}
