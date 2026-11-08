using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Economy;
using BetterLife.Core.Education;
using BetterLife.Core.Events;
using BetterLife.Core.Life;
using BetterLife.Core.Luck;
using BetterLife.Core.Randomness;
using BetterLife.Core.Rules;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Budget;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.Time.Ticking.Phases;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Working life of the player character: applications, weekly performance, boss satisfaction and job stress,
    /// monthly risks, layoffs, firing, promotion reviews and retirement. A job reserves its hours in the time budget.
    /// </summary>
    public sealed class CareerService : IDisposable
    {
        /// <summary>Flag set when fired.</summary>
        public const string FiredFlag = "career.fired_once";

        /// <summary>Source key of the weekly job stress.</summary>
        public const string JobStressKey = "career.job_stress";

        /// <summary>Interrupt reason of promotions.</summary>
        public const string PromotedInterruptKey = "interrupt.career_promoted";

        /// <summary>Interrupt reason of losing a job.</summary>
        public const string JobLostInterruptKey = "interrupt.career_job_lost";

        /// <summary>Interrupt reason of retirement.</summary>
        public const string RetiredInterruptKey = "interrupt.career_retired";

        private const float NeutralPrestige = 50f;
        private const int MonthsPerYear = 12;

        private readonly Character _character;
        private readonly DefinitionCatalog<CareerTrackDefinition> _tracks;
        private readonly CareerSettings _settings;
        private readonly DefinitionCatalog<EducationProgramDefinition> _programs;
        private readonly TimeBudget _budget;
        private readonly TimeSettings _timeSettings;
        private readonly LuckCalculator _luck;
        private readonly IRandomProvider _random;
        private readonly EconomyService _economy;
        private readonly IEventBus _bus;
        private readonly GameClock _clock;
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private GameDate? _hoursWeek;
        private IReadOnlyDictionary<string, int> _hours = new Dictionary<string, int>();

        /// <summary>
        /// Creates the service and starts listening to the time budget.
        /// </summary>
        /// <param name="character">The player character.</param>
        /// <param name="tracks">Career tracks.</param>
        /// <param name="settings">Career settings.</param>
        /// <param name="programs">Education programs (prestige of credentials).</param>
        /// <param name="budget">The character's weekly time budget.</param>
        /// <param name="timeSettings">Time settings (learning speed).</param>
        /// <param name="luck">Chance calculator.</param>
        /// <param name="random">Random stream for careers.</param>
        /// <param name="economy">Economy (layoffs follow the phase).</param>
        /// <param name="bus">Event bus.</param>
        /// <param name="clock">Game clock (current week for player actions).</param>
        public CareerService(Character character, DefinitionCatalog<CareerTrackDefinition> tracks, CareerSettings settings,
            DefinitionCatalog<EducationProgramDefinition> programs, TimeBudget budget, TimeSettings timeSettings, LuckCalculator luck,
            IRandomProvider random, EconomyService economy, IEventBus bus, GameClock clock)
        {
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _tracks = tracks ?? throw new ArgumentNullException(nameof(tracks));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _programs = programs ?? throw new ArgumentNullException(nameof(programs));
            _budget = budget ?? throw new ArgumentNullException(nameof(budget));
            _timeSettings = timeSettings ?? throw new ArgumentNullException(nameof(timeSettings));
            _luck = luck ?? throw new ArgumentNullException(nameof(luck));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _subscriptions.Add(_bus.Subscribe<TimeAllocationAppliedEvent>(e =>
            {
                _hoursWeek = e.Week;
                _hours = e.Hours;
            }));
            _subscriptions.Add(_bus.Subscribe<LifeStageChangedEvent>(e => ReserveHours()));
            _subscriptions.Add(_bus.Subscribe<JobEndedEvent>(e => ReleaseHoursWhenUnemployed()));
        }

        /// <summary>Career tracks.</summary>
        public DefinitionCatalog<CareerTrackDefinition> Tracks => _tracks;

        /// <summary>
        /// Returns the current job's track, or null.
        /// </summary>
        public CareerTrackDefinition CurrentTrack => _character.Career.Current == null ? null : _tracks.Get(_character.Career.Current.TrackId);

        /// <summary>
        /// Returns the current job's level, or null.
        /// </summary>
        public CareerLevelDefinition CurrentLevel => CurrentTrack?.Levels[_character.Career.Current.LevelIndex];

        /// <summary>
        /// Checks whether the character may apply for a track and returns the odds.
        /// </summary>
        /// <param name="trackId">Track id.</param>
        /// <returns>The quote.</returns>
        public HiringQuote Quote(string trackId)
        {
            CareerTrackDefinition track = _tracks.Get(trackId);
            GameDate now = _clock.Now;
            int age = _character.AgeAt(now);
            CharacterCareer career = _character.Career;
            var reasons = new List<string>();
            if (age < _settings.MinWorkAge)
            {
                reasons.Add(HiringQuote.ReasonTooYoung);
            }
            if (age >= _settings.RetirementAge)
            {
                reasons.Add(HiringQuote.ReasonTooOld);
            }
            if (track.Requirement != null && !track.Requirement.Evaluate(new RuleContext(_character, now, _random, track.NameKey)))
            {
                reasons.Add(HiringQuote.ReasonRequirement);
            }
            if (career.Current?.TrackId == trackId)
            {
                reasons.Add(HiringQuote.ReasonSameJob);
            }
            if (career.Attempts.TryGetValue(trackId, out GameDate last) && now.TotalWeeks - last.TotalWeeks < _settings.RetryWeeks)
            {
                reasons.Add(HiringQuote.ReasonRetryWait);
            }

            float adjustment = (Prestige() - NeutralPrestige) * _settings.PrestigePerPoint;
            ChanceBreakdown chance = _luck.Explain(Adjusted(track.Hiring, adjustment), _character, now);
            return new HiringQuote(reasons, adjustment, chance, track.Levels[0].MonthlySalary);
        }

        /// <summary>
        /// Applies for a track; when hired, any current job is quit.
        /// </summary>
        /// <param name="trackId">Track id.</param>
        /// <returns>The result.</returns>
        public HiringResult Apply(string trackId)
        {
            HiringQuote quote = Quote(trackId);
            if (!quote.Eligible)
            {
                return new HiringResult(quote, null, null);
            }

            CareerTrackDefinition track = _tracks.Get(trackId);
            GameDate now = _clock.Now;
            _character.Career.RecordAttempt(trackId, now);
            ChanceRoll roll = _luck.Roll(Adjusted(track.Hiring, quote.PrestigeAdjustment), _character, now, _random);
            if (!roll.Success)
            {
                return new HiringResult(quote, roll, null);
            }

            EndJob(JobEndReason.Quit, now);
            var job = new Job(trackId, 0, now, now, _settings.Performance.Start, _settings.Boss.Start, 0, ReviewAfter(now, track.Levels[0].PromotionAfterMonths));
            _character.Career.Start(job);
            ReserveHours();
            _bus.Publish(new JobStartedEvent(job, now));
            return new HiringResult(quote, roll, job);
        }

        /// <summary>
        /// Quits the current job.
        /// </summary>
        /// <returns>True when there was a job.</returns>
        public bool Quit() => EndJob(JobEndReason.Quit, _clock.Now) != null;

        /// <summary>
        /// Processes one week of work: performance, boss satisfaction, skills and job stress.
        /// </summary>
        /// <param name="tick">Tick context.</param>
        public void ProcessWeek(TickContext tick)
        {
            if (tick == null)
            {
                throw new ArgumentNullException(nameof(tick));
            }
            Job job = _character.Career.Current;
            if (job == null || !_character.IsAlive)
            {
                return;
            }

            CareerTrackDefinition track = _tracks.Get(job.TrackId);
            int hours = HoursOf(_settings.WorkActivityId, tick.Week);
            float skill = track.PerformanceSkillId == null
                ? _character.Stats.GetValue(StatIds.Knowledge)
                : _character.Skills.GetLevel(track.PerformanceSkillId);
            float target = _settings.Performance.Target((float)hours / track.WeeklyHours, skill,
                _character.Stats.GetValue(StatIds.Mental), _character.Stats.GetValue(StatIds.Fatigue));
            job.ChangePerformance((target - job.Performance) * _settings.Performance.WeeklySmoothing);
            job.ChangeBossSatisfaction((job.Performance - job.BossSatisfaction) * _settings.Boss.WeeklySmoothing);

            float learning = _timeSettings.LearningMultiplier(_character.Stats.GetValue(StatIds.Mental));
            foreach (ActivitySkillEffect effect in track.Skills)
            {
                float gain = effect.TotalFor(hours) * learning;
                if (gain > 0f && _character.Catalogs.Skills.Contains(effect.SkillId))
                {
                    _character.Skills.Practice(effect.SkillId, gain, tick.Week);
                }
            }

            float stress = hours * track.StressPerHour * _settings.Boss.StressMultiplier(job.BossSatisfaction);
            if (stress > 0f)
            {
                _character.ApplyStatChange(new StatChange(StatIds.Stress, stress, JobStressKey), tick.Week);
            }
        }

        /// <summary>
        /// Processes one month: retirement, career risk, layoffs, firing and promotion reviews.
        /// </summary>
        /// <param name="tick">Tick context of the month's first week.</param>
        public void ProcessMonth(TickContext tick)
        {
            if (tick == null)
            {
                throw new ArgumentNullException(nameof(tick));
            }
            Job job = _character.Career.Current;
            if (job == null || !_character.IsAlive)
            {
                return;
            }
            GameDate week = tick.Week;
            CareerTrackDefinition track = _tracks.Get(job.TrackId);

            if (_character.AgeAt(week) >= _settings.RetirementAge)
            {
                decimal pension = Money.Round(track.Levels[job.LevelIndex].MonthlySalary * (decimal)track.PensionRate);
                EndJob(JobEndReason.Retired, week);
                _character.Career.Retire(pension);
                tick.RequestInterrupt(Severity.Normal, RetiredInterruptKey, track.Id);
                return;
            }
            if (track.Risk != null && _random.Chance(track.Risk.MonthlyChance))
            {
                var context = new RuleContext(_character, week, _random, track.Risk.ReasonKey);
                EffectResult result = track.Risk.Effects.Apply(context);
                result.PublishTo(_bus);
                _bus.Publish(new CareerRiskEvent(track.Id, track.Risk.ReasonKey, result, week));
                tick.RequestInterrupt(Severity.Major, JobLostInterruptKey, track.Id);
                if (track.Risk.EndsJob)
                {
                    EndJob(JobEndReason.Risk, week);
                }
                if (!_character.Career.IsEmployed || !_character.IsAlive)
                {
                    return;
                }
            }
            if (_random.Chance(_settings.LayoffChance(_economy.State.PhaseId) * track.LayoffSensitivity))
            {
                EndJob(JobEndReason.LaidOff, week);
                tick.RequestInterrupt(Severity.Major, JobLostInterruptKey, track.Id);
                return;
            }

            job.CountBossMonth(job.BossSatisfaction < _settings.Boss.FireBelow);
            if (job.LowBossMonths >= _settings.Boss.FireAfterMonths)
            {
                EndJob(JobEndReason.Fired, week);
                if (_character.Catalogs.Flags.Contains(FiredFlag))
                {
                    _character.Flags.Set(FiredFlag, week);
                }
                tick.RequestInterrupt(Severity.Major, JobLostInterruptKey, track.Id);
                return;
            }
            Review(job, track, week, tick);
        }

        /// <summary>
        /// Reserves work hours in the time budget for the current job (after a stage change resets the plan).
        /// </summary>
        public void ReserveHours()
        {
            CareerTrackDefinition track = CurrentTrack;
            if (track != null)
            {
                _budget.Reserve(_settings.WorkActivityId, track.WeeklyHours);
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

        private void Review(Job job, CareerTrackDefinition track, GameDate week, TickContext tick)
        {
            if (!job.NextReviewOn.HasValue || week < job.NextReviewOn.Value)
            {
                return;
            }
            CareerLevelDefinition level = track.Levels[job.LevelIndex];
            if (job.LevelIndex + 1 >= track.Levels.Count)
            {
                job.ScheduleReview(null);
                return;
            }
            PromotionRules rules = _settings.Promotion;
            bool considered = job.Performance >= level.PromotionPerformance && job.BossSatisfaction >= rules.MinBoss;
            float adjustment = (job.Performance - level.PromotionPerformance) * rules.PerPointAbove;
            if (considered && _luck.Roll(Adjusted(rules.Chance, adjustment), _character, week, _random).Success)
            {
                job.Promote(week, ReviewAfter(week, track.Levels[job.LevelIndex + 1].PromotionAfterMonths));
                _bus.Publish(new PromotedEvent(job, week));
                tick.RequestInterrupt(Severity.Normal, PromotedInterruptKey, track.Id);
                return;
            }
            job.ScheduleReview(ReviewAfter(week, rules.ReviewEveryMonths));
        }

        private PastJob EndJob(JobEndReason reason, GameDate week)
        {
            PastJob ended = _character.Career.End(reason, week);
            if (ended != null)
            {
                _bus.Publish(new JobEndedEvent(ended, week));
            }
            return ended;
        }

        private void ReleaseHoursWhenUnemployed()
        {
            if (!_character.Career.IsEmployed)
            {
                _budget.Set(_settings.WorkActivityId, 0);
            }
        }

        private float Prestige()
        {
            Credential best = _character.Education.Highest(_programs);
            if (best == null || !_programs.Get(best.ProgramId).Institutions.TryGet(best.InstitutionId, out InstitutionDefinition institution))
            {
                return NeutralPrestige;
            }
            return institution.Prestige;
        }

        private int HoursOf(string activityId, GameDate week)
        {
            return _hoursWeek == week && _hours.TryGetValue(activityId, out int hours) ? hours : 0;
        }

        private static GameDate? ReviewAfter(GameDate week, int? months)
        {
            return months.HasValue ? week.AddWeeks(months.Value * GameCalendar.WeeksPerYear / MonthsPerYear) : (GameDate?)null;
        }

        private static ChanceSpec Adjusted(ChanceSpec spec, float points)
        {
            return new ChanceSpec(spec.BasePercent + points, spec.StatTerms, spec.FortuneArea, spec.UseKarma);
        }
    }
}
