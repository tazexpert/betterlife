using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Careers;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Education;
using BetterLife.Core.Localization;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Builds the Work tab (school and job, with where to apply) and the results of applying.
    /// </summary>
    public static class WorkViewModel
    {
        /// <summary>Studying label.</summary>
        public const string StudyingKey = "work.studying";
        /// <summary>Year of study (placeholders <c>{year}</c>, <c>{years}</c>).</summary>
        public const string StudyYearKey = "work.study_year";
        /// <summary>GPA label.</summary>
        public const string GpaKey = "work.gpa";
        /// <summary>Not studying text.</summary>
        public const string NotStudyingKey = "work.not_studying";
        /// <summary>Highest degree label.</summary>
        public const string HighestKey = "work.highest_degree";
        /// <summary>Job label.</summary>
        public const string JobKey = "work.job";
        /// <summary>Salary label.</summary>
        public const string SalaryKey = "work.salary";
        /// <summary>Performance label.</summary>
        public const string PerformanceKey = "work.performance";
        /// <summary>Boss satisfaction label.</summary>
        public const string BossKey = "work.boss";
        /// <summary>Not working text.</summary>
        public const string NotWorkingKey = "work.not_working";
        /// <summary>Retired text (placeholder <c>{value}</c> = monthly pension).</summary>
        public const string RetiredKey = "work.retired";
        /// <summary>School option detail (placeholders <c>{admission}</c>, <c>{tuition}</c>, <c>{payer}</c>).</summary>
        public const string SchoolDetailKey = "work.school.detail";
        /// <summary>Open admission text.</summary>
        public const string OpenAdmissionKey = "work.school.open";
        /// <summary>Exam chance text (placeholder <c>{chance}</c>).</summary>
        public const string ExamChanceKey = "work.school.exam";
        /// <summary>Scholarship possible text.</summary>
        public const string ScholarshipKey = "work.school.scholarship";
        /// <summary>Tuition key prefix by payer (<c>work.payer.family</c>...).</summary>
        public const string PayerPrefix = "work.payer.";
        /// <summary>Job option detail (placeholders <c>{chance}</c>, <c>{salary}</c>, <c>{hours}</c>).</summary>
        public const string JobDetailKey = "work.job.detail";
        /// <summary>Admitted result.</summary>
        public const string AdmittedKey = "work.result.admitted";
        /// <summary>Not admitted result.</summary>
        public const string RejectedKey = "work.result.rejected";
        /// <summary>Scholarship won result.</summary>
        public const string ScholarshipWonKey = "work.result.scholarship";
        /// <summary>Hired result.</summary>
        public const string HiredKey = "work.result.hired";
        /// <summary>Not hired result.</summary>
        public const string NotHiredKey = "work.result.not_hired";
        /// <summary>Left school result.</summary>
        public const string DroppedOutKey = "work.result.dropped_out";
        /// <summary>Quit job result.</summary>
        public const string QuitKey = "work.result.quit";
        /// <summary>Format of the GPA.</summary>
        public const string GpaFormat = "0.00";

        /// <summary>
        /// Builds the Work tab.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="money">Money formatter.</param>
        /// <returns>The state.</returns>
        public static WorkState Build(LifeSession session, ILocalizer localizer, MoneyFormatter money)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (localizer == null || money == null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }
            Character character = session.Character;
            GameDate now = session.TimeState.Clock.Now;

            var education = new List<ValueRow>();
            Enrollment enrollment = character.Education.Current;
            if (enrollment != null && session.Education.Programs.TryGet(enrollment.ProgramId, out EducationProgramDefinition program))
            {
                education.Add(new ValueRow(localizer.Get(StudyingKey), localizer.Get(program.NameKey), InstitutionName(program, enrollment.InstitutionId, localizer)));
                education.Add(new ValueRow(localizer.Format(StudyYearKey, ("year", (enrollment.YearsCompleted + 1).ToString(CultureInfo.InvariantCulture)),
                    ("years", program.Years.ToString(CultureInfo.InvariantCulture))), string.Empty));
                education.Add(new ValueRow(localizer.Get(GpaKey), enrollment.Gpa.ToString(GpaFormat, CultureInfo.InvariantCulture)));
            }
            else
            {
                education.Add(new ValueRow(localizer.Get(NotStudyingKey), string.Empty));
            }
            Credential highest = character.Education.Highest(session.Education.Programs);
            if (highest != null && session.Education.Programs.TryGet(highest.ProgramId, out EducationProgramDefinition degree))
            {
                education.Add(new ValueRow(localizer.Get(HighestKey), localizer.Get(degree.NameKey)));
            }

            bool dependent = !session.Finance.IsIndependent(now);
            List<EducationOption> schools = session.Education.Options()
                .Where(o => o.Quote.Eligible)
                .OrderBy(o => o.Program.Rank)
                .ThenByDescending(o => o.Institution.Prestige)
                .Select(o => School(session, o.Program, o.Institution, o.Quote, dependent, localizer, money))
                .ToList();

            var job = new List<ValueRow>();
            CareerTrackDefinition track = session.Career.CurrentTrack;
            if (track != null)
            {
                Job current = character.Career.Current;
                CareerLevelDefinition level = session.Career.CurrentLevel;
                job.Add(new ValueRow(localizer.Get(JobKey), localizer.Get(level.TitleKey), localizer.Get(track.NameKey)));
                job.Add(new ValueRow(localizer.Get(SalaryKey), localizer.Format(FinanceViewModel.PerMonthKey, ("value", SessionText.Money(session, money, level.MonthlySalary)))));
                job.Add(new ValueRow(localizer.Get(PerformanceKey), current.Performance.ToString(DashboardViewModel.ValueFormat, CultureInfo.InvariantCulture)));
                job.Add(new ValueRow(localizer.Get(BossKey), current.BossSatisfaction.ToString(DashboardViewModel.ValueFormat, CultureInfo.InvariantCulture)));
            }
            else if (character.Career.IsRetired)
            {
                job.Add(new ValueRow(localizer.Format(RetiredKey, ("value", SessionText.Money(session, money, character.Career.PensionMonthly))), string.Empty));
            }
            else
            {
                job.Add(new ValueRow(localizer.Get(NotWorkingKey), string.Empty));
            }

            List<JobOption> jobs = session.Career.Tracks.All
                .Select(t => (Track: t, Quote: session.Career.Quote(t.Id)))
                .Where(p => p.Quote.Eligible)
                .OrderByDescending(p => p.Quote.StartingSalary)
                .Select(p => new JobOption(p.Track.Id, localizer.Get(p.Track.NameKey), localizer.Format(JobDetailKey,
                    ("chance", p.Quote.Chance.Final.ToString(EventDialogViewModel.ChanceFormat, CultureInfo.InvariantCulture)),
                    ("salary", SessionText.Money(session, money, p.Quote.StartingSalary)),
                    ("hours", p.Track.WeeklyHours.ToString(CultureInfo.InvariantCulture)))))
                .ToList();

            return new WorkState(education, enrollment != null, schools, job, track != null, jobs);
        }

        /// <summary>
        /// Builds the result of applying to a school.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="result">Admission result.</param>
        /// <param name="localizer">Localizer.</param>
        /// <returns>The state.</returns>
        public static ActionResultState BuildAdmissionResult(LifeSession session, AdmissionResult result, ILocalizer localizer)
        {
            if (session == null || result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            var lines = new List<string>();
            lines.AddRange(result.Quote.ReasonKeys.Select(localizer.Get));
            if (result.Roll != null)
            {
                lines.Add(Roll(result.Roll.Success, result.Roll.Breakdown.Final, localizer));
            }
            if (result.Admitted && result.Enrollment.Scholarship)
            {
                lines.Add(localizer.Get(ScholarshipWonKey));
            }
            return new ActionResultState(localizer.Get(result.Admitted ? AdmittedKey : RejectedKey), result.Admitted, lines);
        }

        /// <summary>
        /// Builds the result of applying for a job.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="result">Hiring result.</param>
        /// <param name="localizer">Localizer.</param>
        /// <returns>The state.</returns>
        public static ActionResultState BuildHiringResult(LifeSession session, HiringResult result, ILocalizer localizer)
        {
            if (session == null || result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            var lines = new List<string>();
            lines.AddRange(result.Quote.ReasonKeys.Select(localizer.Get));
            if (result.Roll != null)
            {
                lines.Add(Roll(result.Roll.Success, result.Roll.Breakdown.Final, localizer));
            }
            return new ActionResultState(localizer.Get(result.Hired ? HiredKey : NotHiredKey), result.Hired, lines);
        }

        private static EducationOption School(LifeSession session, EducationProgramDefinition program, InstitutionDefinition institution, AdmissionQuote quote,
            bool dependent, ILocalizer localizer, MoneyFormatter money)
        {
            TuitionFunding funding = dependent ? TuitionFunding.Family
                : quote.TuitionPerYear <= 0m || session.Character.Wallet.Cash >= quote.TuitionPerYear ? TuitionFunding.Cash
                : TuitionFunding.StudentLoan;
            string admission = quote.IsOpenAdmission
                ? localizer.Get(OpenAdmissionKey)
                : localizer.Format(ExamChanceKey, ("chance", quote.Chance.Final.ToString(EventDialogViewModel.ChanceFormat, CultureInfo.InvariantCulture)));
            if (quote.ScholarshipPossible)
            {
                admission += " · " + localizer.Get(ScholarshipKey);
            }
            string detail = localizer.Format(SchoolDetailKey, ("admission", admission),
                ("tuition", localizer.Format(FinanceViewModel.PerYearKey, ("value", SessionText.Money(session, money, quote.TuitionPerYear)))),
                ("payer", localizer.Get(SessionText.EnumKey(PayerPrefix, funding))));
            return new EducationOption(program.Id, institution.Id, funding, localizer.Get(program.NameKey) + " · " + localizer.Get(institution.NameKey), detail);
        }

        private static string InstitutionName(EducationProgramDefinition program, string institutionId, ILocalizer localizer)
        {
            return program.Institutions.TryGet(institutionId, out InstitutionDefinition institution) ? localizer.Get(institution.NameKey) : institutionId;
        }

        private static string Roll(bool success, float chance, ILocalizer localizer)
        {
            return localizer.Format(success ? EventDialogViewModel.RollSuccessKey : EventDialogViewModel.RollFailureKey,
                ("value", chance.ToString(EventDialogViewModel.ChanceFormat, CultureInfo.InvariantCulture)));
        }
    }
}
