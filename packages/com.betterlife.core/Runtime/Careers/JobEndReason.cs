namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Why a job ended.
    /// </summary>
    public enum JobEndReason
    {
        /// <summary>The character quit (including for a new job).</summary>
        Quit = 0,

        /// <summary>Fired by an unhappy boss or an event.</summary>
        Fired = 1,

        /// <summary>Laid off in a downturn.</summary>
        LaidOff = 2,

        /// <summary>A career risk ended it (arrest, injury...).</summary>
        Risk = 3,

        /// <summary>Retired.</summary>
        Retired = 4,
    }
}
