using BetterLife.Core.Luck;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// The outcome of a job application.
    /// </summary>
    public sealed class HiringResult
    {
        /// <summary>
        /// Creates the result.
        /// </summary>
        /// <param name="quote">The quote it was based on.</param>
        /// <param name="roll">Hiring roll, or null when not eligible.</param>
        /// <param name="job">New job, or null when refused.</param>
        public HiringResult(HiringQuote quote, ChanceRoll roll, Job job)
        {
            Quote = quote;
            Roll = roll;
            Job = job;
        }

        /// <summary>The quote.</summary>
        public HiringQuote Quote { get; }

        /// <summary>Hiring roll, or null.</summary>
        public ChanceRoll Roll { get; }

        /// <summary>New job, or null.</summary>
        public Job Job { get; }

        /// <summary>True when hired.</summary>
        public bool Hired => Job != null;
    }
}
