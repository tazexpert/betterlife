using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Economy;
using BetterLife.Core.Events;
using BetterLife.Core.Luck;
using BetterLife.Core.Randomness;
using BetterLife.Core.Relationships;
using BetterLife.Core.Rules;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.Time.Ticking.Phases;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// The six vices: weekly temptation (base × stress × addiction × bad company × laziness × life stage × traits), resisting,
    /// uses with short-term relief and long-term costs, gambling with beginner's luck and a house edge, addiction, tolerance,
    /// withdrawal, family discovery, and quitting with relapse or success.
    /// </summary>
    public sealed class ViceService : IDisposable
    {
        /// <summary>Counter flag of failed quit attempts.</summary>
        public const string RelapseFlag = "vice.relapse";
        /// <summary>Counter flag of successful quit attempts.</summary>
        public const string QuitSuccessFlag = "vice.quit_success";
        /// <summary>Counter flag of gambling wins.</summary>
        public const string GamblingWinsFlag = "vice.gambling_wins";
        /// <summary>Source key of withdrawal.</summary>
        public const string WithdrawalKey = "vice.withdrawal";
        /// <summary>Source key of willpower gained by resisting.</summary>
        public const string ResistedKey = "vice.resisted";
        /// <summary>Interrupt reason of relapses.</summary>
        public const string RelapseInterruptKey = "interrupt.vice_relapse";
        /// <summary>Interrupt reason of discovery.</summary>
        public const string DiscoveredInterruptKey = "interrupt.vice_discovered";
        /// <summary>Interrupt reason of a successful quit.</summary>
        public const string QuitInterruptKey = "interrupt.vice_quit";

        private const float Neutral = 50f;
        private const float StakeGrowthPerAddiction = 1f / 50f;

        private readonly Character _character;
        private readonly DefinitionCatalog<ViceDefinition> _vices;
        private readonly DefinitionCatalog<QuitMethodDefinition> _methods;
        private readonly ViceSettings _settings;
        private readonly LuckCalculator _luck;
        private readonly IRandomProvider _random;
        private readonly IEventBus _bus;
        private readonly GameClock _clock;
        private readonly IDisposable _subscription;
        private GameDate? _hoursWeek;
        private IReadOnlyDictionary<string, int> _hours = new Dictionary<string, int>();

        /// <summary>
        /// Creates the service and starts reading the weekly time budget.
        /// </summary>
        /// <param name="character">The player character.</param>
        /// <param name="vices">Vice catalog.</param>
        /// <param name="methods">Quit methods.</param>
        /// <param name="settings">Vice settings.</param>
        /// <param name="luck">Chance calculator.</param>
        /// <param name="random">Random stream for vices.</param>
        /// <param name="bus">Event bus.</param>
        /// <param name="clock">Game clock (current week for player actions).</param>
        public ViceService(Character character, DefinitionCatalog<ViceDefinition> vices, DefinitionCatalog<QuitMethodDefinition> methods,
            ViceSettings settings, LuckCalculator luck, IRandomProvider random, IEventBus bus, GameClock clock)
        {
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _vices = vices ?? throw new ArgumentNullException(nameof(vices));
            _methods = methods ?? throw new ArgumentNullException(nameof(methods));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _luck = luck ?? throw new ArgumentNullException(nameof(luck));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _subscription = _bus.Subscribe<TimeAllocationAppliedEvent>(e =>
            {
                _hoursWeek = e.Week;
                _hours = e.Hours;
            });
        }

        /// <summary>When false (player setting), vices never tempt, cannot be chosen and are never discovered.</summary>
        public bool ContentEnabled { get; set; } = true;

        /// <summary>Vice catalog.</summary>
        public DefinitionCatalog<ViceDefinition> Vices => _vices;

        /// <summary>Quit methods.</summary>
        public DefinitionCatalog<QuitMethodDefinition> Methods => _methods;

        /// <summary>Vice settings (help page key...).</summary>
        public ViceSettings Settings => _settings;

        /// <summary>
        /// Returns true when the habit card should show: addiction from the visible threshold, or while quitting.
        /// </summary>
        /// <param name="state">Vice state.</param>
        /// <returns>True when visible.</returns>
        public bool IsVisible(ViceState state) => state != null && (state.Addiction >= _settings.VisibleFromAddiction || state.Quitting != null);

        /// <summary>
        /// Returns the habits the player should see.
        /// </summary>
        /// <returns>Visible states.</returns>
        public IReadOnlyList<ViceState> VisibleHabits() => _character.Vices.States.Where(IsVisible).ToList();

        /// <summary>
        /// Explains this week's temptation for a vice.
        /// </summary>
        /// <param name="viceId">Vice id.</param>
        /// <returns>The breakdown.</returns>
        public TemptationBreakdown Temptation(string viceId)
        {
            ViceDefinition vice = _vices.Get(viceId);
            ViceState state = _character.Vices.Find(viceId);
            GameDate now = _clock.Now;
            float addiction = state?.Addiction ?? 0f;
            float stress = Math.Max(_settings.StressMinMultiplier,
                1f + (_character.Stats.GetValue(StatIds.Stress) - Neutral) / Neutral * _settings.StressWeight);
            float traits = _character.Traits.Instances
                .Select(t => _settings.TraitMultipliers.TryGetValue(t.TraitId, out float m) ? m : 1f)
                .Aggregate(1f, (a, b) => a * b);
            return new TemptationBreakdown(vice.BaseWeekly, new[]
            {
                (TemptationBreakdown.StressKey, stress),
                (TemptationBreakdown.AddictionKey, 1f + addiction / StatBounds.Max * _settings.AddictionWeight),
                (TemptationBreakdown.BadCompanyKey, BadCompany()),
                (TemptationBreakdown.LazinessKey, Laziness()),
                (TemptationBreakdown.LifeStageKey, vice.StageMultiplier(_character.LifeStage(now).Id)),
                (TemptationBreakdown.TraitsKey, traits),
                (TemptationBreakdown.FirstTryKey, state?.EverUsed == true ? 1f : _settings.FirstTryMultiplier),
            }, _settings.MaxWeeklyTemptation);
        }

        /// <summary>
        /// Explains the chance to resist a temptation of a vice.
        /// </summary>
        /// <param name="viceId">Vice id.</param>
        /// <returns>The quote.</returns>
        public ResistQuote Resist(string viceId)
        {
            _vices.Get(viceId);
            ViceState state = _character.Vices.Find(viceId);
            float addiction = (state?.Addiction ?? 0f) * _settings.Resist.PerAddiction;
            int supporters = _character.Relationships.Living()
                .Count(r => r.Type != RelationshipType.Ex && r.Type != RelationshipType.Colleague && r.Type != RelationshipType.Contact
                    && r.Value >= _settings.SupportMinValue);
            float support = Math.Min(_settings.SupportMax, supporters * _settings.SupportPerTie);
            float quit = state?.Quitting != null && _methods.TryGet(state.Quitting.MethodId, out QuitMethodDefinition method) ? method.ResistBonus : 0f;
            ChanceBreakdown chance = _luck.Explain(_settings.Resist.Chance.WithBonus(addiction + support + quit), _character, _clock.Now);
            return new ResistQuote(chance, addiction, support, quit);
        }

        /// <summary>
        /// Uses a vice by choice.
        /// </summary>
        /// <param name="viceId">Vice id.</param>
        /// <returns>What happened.</returns>
        public ViceUseResult Indulge(string viceId) => Use(_vices.Get(viceId), _clock.Now, true, null);

        /// <summary>
        /// Checks whether the character can start quitting a vice with a method.
        /// </summary>
        /// <param name="viceId">Vice id.</param>
        /// <param name="methodId">Quit method id.</param>
        /// <returns>The quote.</returns>
        public QuitQuote QuoteQuit(string viceId, string methodId)
        {
            _vices.Get(viceId);
            QuitMethodDefinition method = _methods.Get(methodId);
            ViceState state = _character.Vices.Find(viceId);
            var reasons = new List<string>();
            if (state?.Quitting != null)
            {
                reasons.Add(QuitQuote.ReasonAlreadyQuitting);
            }
            if (!method.AppliesTo(viceId))
            {
                reasons.Add(QuitQuote.ReasonMethod);
            }
            if (state == null || !state.EverUsed)
            {
                reasons.Add(QuitQuote.ReasonNotUsing);
            }
            if (_character.Wallet.Cash < method.CostCash)
            {
                reasons.Add(QuitQuote.ReasonCash);
            }
            if (method.Condition != null && !method.Condition.Evaluate(new RuleContext(_character, _clock.Now, _random, method.NameKey)))
            {
                reasons.Add(QuitQuote.ReasonCondition);
            }
            return new QuitQuote(reasons, method.CostCash, method.DurationWeeks);
        }

        /// <summary>
        /// Starts quitting a vice: pays the cost and applies the method's start effects.
        /// </summary>
        /// <param name="viceId">Vice id.</param>
        /// <param name="methodId">Quit method id.</param>
        /// <returns>The quote it was based on.</returns>
        public QuitQuote StartQuit(string viceId, string methodId)
        {
            QuitQuote quote = QuoteQuit(viceId, methodId);
            if (!quote.Available)
            {
                return quote;
            }
            QuitMethodDefinition method = _methods.Get(methodId);
            GameDate now = _clock.Now;
            if (method.CostCash > 0m)
            {
                _character.Wallet.TryWithdraw(method.CostCash);
            }
            _character.Vices.Get(viceId).StartQuit(new QuitAttempt(methodId, now, now.AddWeeks(method.DurationWeeks)));
            method.StartEffects.Apply(new RuleContext(_character, now, _random, method.NameKey)).PublishTo(_bus);
            return quote;
        }

        /// <summary>
        /// Processes one week for every vice: activity uses, temptation, rest and withdrawal, discovery and quit progress.
        /// </summary>
        /// <param name="tick">Tick context.</param>
        public void ProcessWeek(TickContext tick)
        {
            if (tick == null)
            {
                throw new ArgumentNullException(nameof(tick));
            }
            GameDate week = tick.Week;
            int age = _character.AgeAt(week);
            foreach (ViceDefinition vice in _vices.All)
            {
                if (!_character.IsAlive)
                {
                    return;
                }
                bool used = false;
                if (ContentEnabled && age >= vice.MinAge)
                {
                    used = UseActivityHours(vice, week, tick);
                    if (!used)
                    {
                        used = Tempt(vice, week, tick);
                    }
                }
                ViceState state = _character.Vices.Find(vice.Id);
                if (state == null)
                {
                    continue;
                }
                if (!used)
                {
                    Rest(vice, state, week);
                }
                if (ContentEnabled)
                {
                    CheckDiscovery(vice, state, week, tick);
                }
                CheckQuit(vice, state, week, tick);
            }
        }

        /// <summary>
        /// Stops reading the time budget.
        /// </summary>
        public void Dispose() => _subscription.Dispose();

        private bool UseActivityHours(ViceDefinition vice, GameDate week, TickContext tick)
        {
            if (vice.Activity == null || _hoursWeek != week || !_hours.TryGetValue(vice.Activity.ActivityId, out int hours))
            {
                return false;
            }
            int uses = vice.Activity.UsesFor(hours);
            bool any = false;
            for (int i = 0; i < uses; i++)
            {
                any |= Use(vice, week, true, tick).Performed;
            }
            return any;
        }

        private bool Tempt(ViceDefinition vice, GameDate week, TickContext tick)
        {
            if (!_random.Chance(Temptation(vice.Id).WeeklyChance))
            {
                return false;
            }
            ResistQuote resist = Resist(vice.Id);
            if (_luck.Roll(_settings.Resist.Chance.WithBonus(resist.TotalAdjustment), _character, week, _random).Success)
            {
                if (_settings.Resist.WillpowerGain > 0f)
                {
                    _character.ApplyStatChange(new StatChange(StatIds.Willpower, _settings.Resist.WillpowerGain, ResistedKey), week);
                }
                _bus.Publish(new TemptationResistedEvent(vice.Id, week));
                return false;
            }
            return Use(vice, week, false, tick).Performed;
        }

        private ViceUseResult Use(ViceDefinition vice, GameDate week, bool voluntary, TickContext tick)
        {
            if (!ContentEnabled)
            {
                return new ViceUseResult(vice.Id, new[] { ViceUseResult.ReasonDisabled }, voluntary, 0m, 0m, false, null);
            }
            if (_character.AgeAt(week) < vice.MinAge)
            {
                return new ViceUseResult(vice.Id, new[] { ViceUseResult.ReasonTooYoung }, voluntary, 0m, 0m, false, null);
            }
            ViceState state = _character.Vices.Get(vice.Id);
            decimal cost = vice.IsGambling
                ? Money.Round(vice.Gambling.Stake.Roll(_random) * (1m + (decimal)(state.Addiction * StakeGrowthPerAddiction)))
                : vice.CostPerUse;
            if (!Pay(cost, state, week, vice.NameKey))
            {
                return new ViceUseResult(vice.Id, new[] { ViceUseResult.ReasonCash }, voluntary, 0m, 0m, false, null);
            }

            decimal won = vice.IsGambling ? Bet(vice, state, cost, week) : 0m;
            if (vice.BlocksUndo)
            {
                _character.Vices.MarkIrreversible(week);
            }
            float relief = 1f - state.Tolerance / StatBounds.Max;
            Change(StatIds.Stress, vice.ReliefStress * relief, vice.NameKey, week);
            Change(StatIds.Happiness, vice.ReliefHappiness * relief, vice.NameKey, week);
            state.RecordUse(week, vice.GainPerUse * (1f - state.Addiction / StatBounds.Max), vice.TolerancePerUse);

            var context = new RuleContext(_character, week, _random, vice.NameKey);
            vice.UseEffects.Apply(context).PublishTo(_bus);
            var risks = new List<string>();
            foreach (ViceRisk risk in vice.Risks.Where(r => _random.Chance(r.Chance)))
            {
                risk.Effects.Apply(context).PublishTo(_bus);
                risks.Add(risk.ReasonKey);
                tick?.RequestInterrupt(risk.Severity, risk.ReasonKey, vice.Id);
            }
            bool relapsed = state.Quitting != null;
            if (relapsed)
            {
                string methodId = state.Quitting.MethodId;
                state.Relapse();
                IncrementFlag(RelapseFlag, week);
                _bus.Publish(new RelapseEvent(vice.Id, methodId, week));
                tick?.RequestInterrupt(Severity.Normal, RelapseInterruptKey, vice.Id);
            }
            var result = new ViceUseResult(vice.Id, null, voluntary, cost, won, relapsed, risks);
            _bus.Publish(new ViceUsedEvent(result, week));
            return result;
        }

        private bool Pay(decimal cost, ViceState state, GameDate week, string sourceKey)
        {
            if (cost <= 0m || _character.Wallet.TryWithdraw(cost))
            {
                return true;
            }
            if (state.Addiction < _settings.ChaseFromAddiction)
            {
                return false;
            }
            decimal paid = _character.Wallet.Cash > 0m ? _character.Wallet.WithdrawUpTo(cost) : 0m;
            _bus.Publish(new CashShortfallEvent(cost - paid, sourceKey, week));
            return true;
        }

        private decimal Bet(ViceDefinition vice, ViceState state, decimal stake, GameDate week)
        {
            GamblingRules rules = vice.Gambling;
            float chance = state.Bets < rules.BeginnerBets ? rules.BeginnerWinChance : rules.WinChance;
            bool won = _random.Chance(chance);
            decimal payout = won ? Money.Round(stake * rules.Payout) : 0m;
            if (payout > 0m)
            {
                _character.Wallet.Deposit(payout);
                IncrementFlag(GamblingWinsFlag, week);
            }
            state.RecordBet(payout - stake);
            _bus.Publish(new GambleResultEvent(vice.Id, stake, won, payout, week));
            return payout;
        }

        private void Rest(ViceDefinition vice, ViceState state, GameDate week)
        {
            QuitMethodDefinition method = state.Quitting != null && _methods.TryGet(state.Quitting.MethodId, out QuitMethodDefinition m) ? m : null;
            state.Rest(vice.DecayPerCleanWeek * (method?.DecayMultiplier ?? 1f), vice.ToleranceDecay);
            if (!ContentEnabled || !state.EverUsed || state.Addiction < vice.WithdrawalFrom)
            {
                return;
            }
            float strength = state.Addiction / StatBounds.Max * (method?.WithdrawalMultiplier ?? 1f);
            Change(StatIds.Stress, vice.WithdrawalStress * strength, WithdrawalKey, week);
            Change(StatIds.Happiness, vice.WithdrawalHappiness * strength, WithdrawalKey, week);
        }

        private void CheckDiscovery(ViceDefinition vice, ViceState state, GameDate week, TickContext tick)
        {
            ViceDiscovery discovery = vice.Discovery;
            if (discovery == null || state.Discovered || state.Addiction < discovery.FromAddiction)
            {
                return;
            }
            bool family = _character.Relationships.Living().Any(r => r.Type == RelationshipType.Parent || r.Type == RelationshipType.Spouse);
            if (!family || !_random.Chance(discovery.WeeklyChance))
            {
                return;
            }
            state.MarkDiscovered();
            discovery.Effects.Apply(new RuleContext(_character, week, _random, vice.NameKey)).PublishTo(_bus);
            _bus.Publish(new ViceDiscoveredEvent(vice.Id, week));
            tick.RequestInterrupt(Severity.Major, DiscoveredInterruptKey, vice.Id);
        }

        private void CheckQuit(ViceDefinition vice, ViceState state, GameDate week, TickContext tick)
        {
            QuitAttempt attempt = state.Quitting;
            if (attempt == null || week < attempt.EndsOn)
            {
                return;
            }
            state.CompleteQuit();
            IncrementFlag(QuitSuccessFlag, week);
            if (_methods.TryGet(attempt.MethodId, out QuitMethodDefinition method))
            {
                method.SuccessEffects.Apply(new RuleContext(_character, week, _random, method.NameKey)).PublishTo(_bus);
            }
            _bus.Publish(new QuitSucceededEvent(vice.Id, attempt.MethodId, week));
            tick.RequestInterrupt(Severity.Normal, QuitInterruptKey, vice.Id);
        }

        private float BadCompany()
        {
            BadCompanyRules rules = _settings.BadCompany;
            CharacterRelationships people = _character.Relationships;
            int bad = people.Living().Count(r => !RelationshipNames.IsFamily(r.Type) && r.Value >= rules.MinValue
                && people.Npc(r.NpcId).TraitIds.Any(rules.TraitIds.Contains));
            return Math.Min(rules.Max, 1f + bad * rules.PerFriend);
        }

        private float Laziness()
        {
            LazinessRules rules = _settings.Laziness;
            int idle = rules.ActivityIds.Sum(id => _hours.TryGetValue(id, out int hours) ? hours : 0);
            return Math.Min(rules.Max, 1f + Math.Max(0, idle - rules.BaselineHours) * rules.PerHour);
        }

        private void Change(StatId stat, float delta, string sourceKey, GameDate week)
        {
            if (delta != 0f && _character.Catalogs.Stats.Contains(stat))
            {
                _character.ApplyStatChange(new StatChange(stat, delta, sourceKey), week);
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
