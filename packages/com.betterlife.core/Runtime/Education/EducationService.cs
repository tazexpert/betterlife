using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Economy;
using BetterLife.Core.Events;
using BetterLife.Core.Life;
using BetterLife.Core.Luck;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Budget;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.Time.Ticking.Phases;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// Schooling of the player character: automatic enrollment of dependents, entrance exams, scholarships,
    /// tuition, weekly performance from the time budget, yearly promotion, graduation, dropout and expulsion.
    /// School reserves its hours in the time budget while enrolled.
    /// </summary>
    public sealed class EducationService : IDisposable
    {
        /// <summary>Counter flag of repeated school years.</summary>
        public const string RepeatedYearFlag = "education.repeated_year";

        /// <summary>Flag set on dropping out.</summary>
        public const string DropoutFlag = "education.dropout";

        /// <summary>Flag set on expulsion.</summary>
        public const string ExpelledFlag = "education.expelled";

        /// <summary>Source key of tuition payments.</summary>
        public const string TuitionKey = "education.tuition";

        /// <summary>Interrupt reason of graduation.</summary>
        public const string GraduatedInterruptKey = "interrupt.education_graduated";

        /// <summary>Interrupt reason of a repeated year or expulsion.</summary>
        public const string TroubleInterruptKey = "interrupt.education_trouble";

        private const int WeeksPerSchoolYear = GameCalendar.WeeksPerYear;

        private readonly Character _character;
        private readonly DefinitionCatalog<EducationProgramDefinition> _programs;
        private readonly EducationSettings _settings;
        private readonly TimeSettings _timeSettings;
        private readonly TimeBudget _budget;
        private readonly LuckCalculator _luck;
        private readonly IRandomProvider _random;
        private readonly IDependencyStatus _dependency;
        private readonly ILoanProvider _loans;
        private readonly IEventBus _bus;
        private readonly GameClock _clock;
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private GameDate? _hoursWeek;
        private IReadOnlyDictionary<string, int> _hours = new Dictionary<string, int>();

        /// <summary>
        /// Creates the service and starts listening to the time budget.
        /// </summary>
        /// <param name="character">The player character.</param>
        /// <param name="programs">Program catalog.</param>
        /// <param name="settings">Education settings.</param>
        /// <param name="timeSettings">Time settings (learning speed).</param>
        /// <param name="budget">The character's weekly time budget.</param>
        /// <param name="luck">Chance calculator.</param>
        /// <param name="random">Random stream for exams and scholarships.</param>
        /// <param name="dependency">Whether the family still pays.</param>
        /// <param name="loans">Lender for student loans.</param>
        /// <param name="bus">Event bus.</param>
        /// <param name="clock">Game clock (current week for player actions).</param>
        public EducationService(Character character, DefinitionCatalog<EducationProgramDefinition> programs, EducationSettings settings,
            TimeSettings timeSettings, TimeBudget budget, LuckCalculator luck, IRandomProvider random, IDependencyStatus dependency,
            ILoanProvider loans, IEventBus bus, GameClock clock)
        {
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _programs = programs ?? throw new ArgumentNullException(nameof(programs));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _timeSettings = timeSettings ?? throw new ArgumentNullException(nameof(timeSettings));
            _budget = budget ?? throw new ArgumentNullException(nameof(budget));
            _luck = luck ?? throw new ArgumentNullException(nameof(luck));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
            _loans = loans ?? throw new ArgumentNullException(nameof(loans));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _subscriptions.Add(_bus.Subscribe<TimeAllocationAppliedEvent>(e =>
            {
                _hoursWeek = e.Week;
                _hours = e.Hours;
            }));
            _subscriptions.Add(_bus.Subscribe<LifeStageChangedEvent>(e => ReserveHours()));
        }

        /// <summary>Program catalog.</summary>
        public DefinitionCatalog<EducationProgramDefinition> Programs => _programs;

        /// <summary>
        /// Lists every institution the character could apply to now, with quotes.
        /// </summary>
        /// <returns>(program, institution, quote) for each institution.</returns>
        public IReadOnlyList<(EducationProgramDefinition Program, InstitutionDefinition Institution, AdmissionQuote Quote)> Options()
        {
            return _programs.All
                .SelectMany(p => p.Institutions.All.Select(i => (p, i, Quote(p.Id, i.Id))))
                .ToList();
        }

        /// <summary>
        /// Checks whether the character may apply and returns the odds.
        /// </summary>
        /// <param name="programId">Program id.</param>
        /// <param name="institutionId">Institution id of that program.</param>
        /// <returns>The quote.</returns>
        public AdmissionQuote Quote(string programId, string institutionId)
        {
            EducationProgramDefinition program = _programs.Get(programId);
            InstitutionDefinition institution = program.Institutions.Get(institutionId);
            GameDate now = _clock.Now;
            CharacterEducation education = _character.Education;
            int age = _character.AgeAt(now);

            var reasons = new List<string>();
            if (age < program.MinEntryAge)
            {
                reasons.Add(AdmissionQuote.ReasonTooYoung);
            }
            if (program.MaxEntryAge.HasValue && age > program.MaxEntryAge.Value)
            {
                reasons.Add(AdmissionQuote.ReasonTooOld);
            }
            int highest = education.HighestRank(_programs);
            if (highest < program.RequiresRank)
            {
                reasons.Add(AdmissionQuote.ReasonRequirement);
            }
            if (highest >= program.Rank)
            {
                reasons.Add(AdmissionQuote.ReasonCompleted);
            }
            if (education.IsEnrolled)
            {
                reasons.Add(AdmissionQuote.ReasonEnrolled);
            }
            if (institution.HasExam && education.Attempts.TryGetValue(programId, out GameDate last)
                && now.TotalWeeks - last.TotalWeeks < _settings.Admission.RetryWeeks)
            {
                reasons.Add(AdmissionQuote.ReasonRetryWait);
            }

            float score = ExamScore();
            float adjustment = institution.HasExam ? (score - institution.Difficulty.Value) * _settings.Admission.PerPointAboveDifficulty : 0f;
            ChanceBreakdown chance = institution.HasExam ? _luck.Explain(Adjusted(_settings.Admission.Chance, adjustment), _character, now) : null;
            bool scholarship = institution.TuitionPerYear > 0m && score >= _settings.Scholarship.MinScore;
            return new AdmissionQuote(reasons, score, adjustment, chance, institution.TuitionPerYear, scholarship);
        }

        /// <summary>
        /// Applies to a program: open programs admit every eligible applicant, others roll the entrance exam.
        /// Dependents always have their tuition paid by the family.
        /// </summary>
        /// <param name="programId">Program id.</param>
        /// <param name="institutionId">Institution id.</param>
        /// <param name="funding">Who pays when independent (Cash or StudentLoan).</param>
        /// <returns>The result.</returns>
        public AdmissionResult Apply(string programId, string institutionId, TuitionFunding funding = TuitionFunding.Cash)
        {
            AdmissionQuote quote = Quote(programId, institutionId);
            if (!quote.Eligible)
            {
                return new AdmissionResult(quote, null, null);
            }

            GameDate now = _clock.Now;
            InstitutionDefinition institution = _programs.Get(programId).Institutions.Get(institutionId);
            ChanceRoll roll = null;
            if (institution.HasExam)
            {
                _character.Education.RecordAttempt(programId, now);
                roll = _luck.Roll(Adjusted(_settings.Admission.Chance, quote.ExamAdjustment), _character, now, _random);
                if (!roll.Success)
                {
                    return new AdmissionResult(quote, roll, null);
                }
            }
            bool scholarship = quote.ScholarshipPossible && _luck.Roll(_settings.Scholarship.Chance, _character, now, _random).Success;
            Enrollment enrollment = Enroll(programId, institutionId, now, scholarship, funding);
            return new AdmissionResult(quote, roll, enrollment);
        }

        /// <summary>
        /// Leaves the current program. Dependents cannot leave compulsory schooling.
        /// </summary>
        /// <returns>True when the character left.</returns>
        public bool DropOut()
        {
            Enrollment current = _character.Education.Current;
            if (current == null || (_programs.Get(current.ProgramId).Compulsory && !_dependency.IsIndependent(_clock.Now)))
            {
                return false;
            }
            End(EducationEndReason.DroppedOut, _clock.Now);
            SetFlag(DropoutFlag, _clock.Now);
            return true;
        }

        /// <summary>
        /// Processes one week: performance, skills and year end for students; automatic enrollment of dependents at year start.
        /// </summary>
        /// <param name="tick">Tick context.</param>
        public void ProcessWeek(TickContext tick)
        {
            if (tick == null)
            {
                throw new ArgumentNullException(nameof(tick));
            }
            if (!_character.IsAlive)
            {
                return;
            }

            GameDate week = tick.Week;
            Enrollment current = _character.Education.Current;
            if (current != null)
            {
                Study(current, week);
                if (week.TotalWeeks - current.YearStartedOn.TotalWeeks >= WeeksPerSchoolYear)
                {
                    EndYear(current, week, tick);
                }
            }
            if (week.IsYearStart && !_character.Education.IsEnrolled && !_dependency.IsIndependent(week))
            {
                AutoEnroll(week);
            }
        }

        /// <summary>
        /// Reserves school hours in the time budget for the current enrollment (after a stage change resets the plan).
        /// </summary>
        public void ReserveHours()
        {
            Enrollment current = _character.Education.Current;
            if (current != null)
            {
                _budget.Reserve(_settings.SchoolActivityId, _programs.Get(current.ProgramId).WeeklyHours);
            }
        }

        /// <summary>
        /// Stops listening to events.
        /// </summary>
        public void Dispose()
        {
            foreach (IDisposable subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();
        }

        private float ExamScore()
        {
            CharacterEducation education = _character.Education;
            float academic = education.Current?.Performance ?? education.Highest(_programs)?.FinalPerformance ?? _settings.StartPerformance;
            return _settings.Admission.Score(academic, _character.Stats.GetValue(StatIds.Knowledge));
        }

        private static ChanceSpec Adjusted(ChanceSpec spec, float points)
        {
            return new ChanceSpec(spec.BasePercent + points, spec.StatTerms, spec.FortuneArea, spec.UseKarma);
        }

        private Enrollment Enroll(string programId, string institutionId, GameDate week, bool scholarship, TuitionFunding funding)
        {
            bool dependent = !_dependency.IsIndependent(week);
            var enrollment = new Enrollment(programId, institutionId, week, week, 0, 0, _settings.StartPerformance, scholarship,
                dependent ? TuitionFunding.Family : (funding == TuitionFunding.Family ? TuitionFunding.Cash : funding));
            _character.Education.Enroll(enrollment);
            ReserveHours();
            PayTuition(enrollment, week);
            _bus.Publish(new EnrolledEvent(enrollment, week));
            return enrollment;
        }

        private void AutoEnroll(GameDate week)
        {
            int age = _character.AgeAt(week);
            int highest = _character.Education.HighestRank(_programs);
            EducationProgramDefinition program = _programs.All.FirstOrDefault(p =>
                p.AutoEnroll && p.AcceptsAge(age) && highest >= p.RequiresRank && highest < p.Rank);
            if (program == null)
            {
                return;
            }
            Enroll(program.Id, AutoInstitution(program).Id, week, false, TuitionFunding.Family);
        }

        private InstitutionDefinition AutoInstitution(EducationProgramDefinition program)
        {
            int income = _character.Family == null ? 0 : _character.Catalogs.FamilyBackgrounds.Get(_character.Family.BackgroundId).HouseholdIncomeLevel;
            return program.Institutions.All.LastOrDefault(i => i.FamilyMinIncome.HasValue && i.FamilyMinIncome.Value <= income)
                ?? program.Institutions.All[0];
        }

        private void Study(Enrollment current, GameDate week)
        {
            EducationProgramDefinition program = _programs.Get(current.ProgramId);
            InstitutionDefinition institution = program.Institutions.Get(current.InstitutionId);
            int schoolHours = HoursOf(_settings.SchoolActivityId, week);
            int studyHours = HoursOf(_settings.StudyActivityId, week);
            float target = _settings.Performance.Target((float)schoolHours / program.WeeklyHours, _character.Stats.GetValue(StatIds.Iq),
                _character.Stats.GetValue(StatIds.Knowledge), _character.Stats.GetValue(StatIds.Mental), studyHours, institution.Quality);
            current.ChangePerformance((target - current.Performance) * _settings.Performance.WeeklySmoothing);

            float learning = _timeSettings.LearningMultiplier(_character.Stats.GetValue(StatIds.Mental));
            foreach (ActivitySkillEffect skill in program.Skills)
            {
                float gain = skill.TotalFor(schoolHours) * learning;
                if (gain > 0f && _character.Catalogs.Skills.Contains(skill.SkillId))
                {
                    _character.Skills.Practice(skill.SkillId, gain, week);
                }
            }
        }

        private void EndYear(Enrollment current, GameDate week, TickContext tick)
        {
            EducationProgramDefinition program = _programs.Get(current.ProgramId);
            bool passed = current.Performance >= _settings.PassPerformance;
            _bus.Publish(new SchoolYearEndedEvent(current, passed, current.Performance, week));
            if (passed)
            {
                current.PassYear(week);
                if (current.YearsCompleted >= program.Years)
                {
                    End(EducationEndReason.Graduated, week);
                    tick.RequestInterrupt(Severity.Normal, GraduatedInterruptKey, program.Id);
                    return;
                }
            }
            else
            {
                current.RepeatYear(week);
                IncrementFlag(RepeatedYearFlag, week);
                tick.RequestInterrupt(Severity.Normal, TroubleInterruptKey, program.Id);
                if (current.Repeats > _settings.MaxRepeats)
                {
                    End(EducationEndReason.Expelled, week);
                    SetFlag(ExpelledFlag, week);
                    tick.RequestInterrupt(Severity.Major, TroubleInterruptKey, program.Id);
                    return;
                }
            }

            if (current.Scholarship && current.Performance < _settings.Scholarship.KeepPerformance)
            {
                current.SetScholarship(false);
            }
            if (current.Funding == TuitionFunding.Family && _dependency.IsIndependent(week))
            {
                current.SetFunding(TuitionFunding.Cash);
            }
            PayTuition(current, week);
        }

        private void End(EducationEndReason reason, GameDate week)
        {
            Enrollment ended = _character.Education.End(reason, week);
            if (ended == null)
            {
                return;
            }
            _budget.Set(_settings.SchoolActivityId, 0);
            _bus.Publish(new EducationEndedEvent(ended, reason, week));
        }

        private void PayTuition(Enrollment enrollment, GameDate week)
        {
            decimal tuition = _programs.Get(enrollment.ProgramId).Institutions.Get(enrollment.InstitutionId).TuitionPerYear;
            if (tuition <= 0m || enrollment.Scholarship || enrollment.Funding == TuitionFunding.Family)
            {
                return;
            }
            if (enrollment.Funding == TuitionFunding.StudentLoan && _loans.Quote(_settings.StudentLoanProductId, tuition).Approved)
            {
                _loans.TakeLoan(_settings.StudentLoanProductId, tuition);
            }
            decimal paid = _character.Wallet.Cash > 0m ? _character.Wallet.WithdrawUpTo(tuition) : 0m;
            if (paid < tuition)
            {
                _bus.Publish(new CashShortfallEvent(tuition - paid, TuitionKey, week));
            }
        }

        private int HoursOf(string activityId, GameDate week)
        {
            return _hoursWeek == week && _hours.TryGetValue(activityId, out int hours) ? hours : 0;
        }

        private void SetFlag(string flagId, GameDate week)
        {
            if (_character.Catalogs.Flags.Contains(flagId))
            {
                _character.Flags.Set(flagId, week);
            }
        }

        private void IncrementFlag(string flagId, GameDate week)
        {
            if (_character.Catalogs.Flags.Contains(flagId))
            {
                _character.Flags.Increment(flagId, week);
            }
        }
    }
}
