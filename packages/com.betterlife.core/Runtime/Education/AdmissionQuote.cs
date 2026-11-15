using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Luck;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// Whether the character may apply to a program at an institution, and the odds.
    /// </summary>
    public sealed class AdmissionQuote
    {
        /// <summary>Too young for the program.</summary>
        public const string ReasonTooYoung = "education.admission.too_young";
        /// <summary>Too old for the program.</summary>
        public const string ReasonTooOld = "education.admission.too_old";
        /// <summary>Missing the required credential.</summary>
        public const string ReasonRequirement = "education.admission.requirement";
        /// <summary>Already studying.</summary>
        public const string ReasonEnrolled = "education.admission.already_enrolled";
        /// <summary>Already holds this level or higher.</summary>
        public const string ReasonCompleted = "education.admission.completed";
        /// <summary>Tried this program too recently.</summary>
        public const string ReasonRetryWait = "education.admission.retry_wait";

        /// <summary>
        /// Creates the quote.
        /// </summary>
        /// <param name="reasonKeys">Why the character may not apply; empty when eligible.</param>
        /// <param name="examScore">Exam score 0-100.</param>
        /// <param name="examAdjustment">Chance points from the score versus the difficulty (included in the chance).</param>
        /// <param name="chance">Admission chance, or null for open admission.</param>
        /// <param name="tuitionPerYear">Real yearly tuition.</param>
        /// <param name="scholarshipPossible">True when the score qualifies for a scholarship roll.</param>
        public AdmissionQuote(IReadOnlyList<string> reasonKeys, float examScore, float examAdjustment, ChanceBreakdown chance, decimal tuitionPerYear, bool scholarshipPossible)
        {
            ReasonKeys = (reasonKeys ?? throw new ArgumentNullException(nameof(reasonKeys))).ToList();
            ExamScore = examScore;
            ExamAdjustment = examAdjustment;
            Chance = chance;
            TuitionPerYear = tuitionPerYear;
            ScholarshipPossible = scholarshipPossible;
        }

        /// <summary>True when the character may apply.</summary>
        public bool Eligible => ReasonKeys.Count == 0;

        /// <summary>Why not, as localization keys.</summary>
        public IReadOnlyList<string> ReasonKeys { get; }

        /// <summary>Exam score 0-100.</summary>
        public float ExamScore { get; }

        /// <summary>Chance points from score versus difficulty.</summary>
        public float ExamAdjustment { get; }

        /// <summary>Admission chance, or null for open admission.</summary>
        public ChanceBreakdown Chance { get; }

        /// <summary>True when there is no exam.</summary>
        public bool IsOpenAdmission => Chance == null;

        /// <summary>Real yearly tuition.</summary>
        public decimal TuitionPerYear { get; }

        /// <summary>True when the score qualifies for a scholarship roll.</summary>
        public bool ScholarshipPossible { get; }
    }
}
