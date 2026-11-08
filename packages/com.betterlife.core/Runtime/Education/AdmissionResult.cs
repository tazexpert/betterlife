using BetterLife.Core.Luck;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// The outcome of an application.
    /// </summary>
    public sealed class AdmissionResult
    {
        /// <summary>
        /// Creates the result.
        /// </summary>
        /// <param name="quote">The quote it was based on.</param>
        /// <param name="roll">Exam roll, or null for open admission or when not eligible.</param>
        /// <param name="enrollment">New enrollment, or null when refused.</param>
        public AdmissionResult(AdmissionQuote quote, ChanceRoll roll, Enrollment enrollment)
        {
            Quote = quote;
            Roll = roll;
            Enrollment = enrollment;
        }

        /// <summary>The quote.</summary>
        public AdmissionQuote Quote { get; }

        /// <summary>Exam roll, or null.</summary>
        public ChanceRoll Roll { get; }

        /// <summary>New enrollment, or null.</summary>
        public Enrollment Enrollment { get; }

        /// <summary>True when admitted.</summary>
        public bool Admitted => Enrollment != null;
    }
}
