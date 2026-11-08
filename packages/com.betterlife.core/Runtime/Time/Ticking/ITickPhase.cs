namespace BetterLife.Core.Time.Ticking
{
    /// <summary>
    /// One step of a week tick. Systems add phases at startup; the time system never changes.
    /// </summary>
    public interface ITickPhase
    {
        /// <summary>Name for logs and diagnostics.</summary>
        string Name { get; }

        /// <summary>Position within a tick; see <see cref="TickOrder"/>. Lower runs first.</summary>
        int Order { get; }

        /// <summary>How often the phase runs.</summary>
        TickCadence Cadence { get; }

        /// <summary>
        /// Runs the phase for one week.
        /// </summary>
        /// <param name="context">Tick context.</param>
        void Execute(TickContext context);
    }
}
