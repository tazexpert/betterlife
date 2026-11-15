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

namespace BetterLife.Core.Scams
{
    /// <summary>
    /// Scammers: each month every scam may target the character (age, loneliness, time online, money and situation raise
    /// the odds); the character may spot it (street smarts, knowledge, calm, people who warn them) or lose money from cash
    /// and savings. Losses can be reported for a chance to get part of the money back.
    /// </summary>
    public sealed class ScamService : IDisposable
    {
        /// <summary>Source key of street smarts gained from experience.</summary>
        public const string LessonKey = "scam.lesson";
        /// <summary>Interrupt reason of being scammed.</summary>
        public const string ScammedInterruptKey = "interrupt.scammed";

        private const float Neutral = 50f;

        private readonly Character _character;
        private readonly DefinitionCatalog<ScamDefinition> _scams;
        private readonly ScamSettings _settings;
        private readonly DefinitionCatalog<AssetDefinition> _assets;
        private readonly LuckCalculator _luck;
        private readonly IRandomProvider _random;
        private readonly IEventBus _bus;
        private readonly GameClock _clock;
        private readonly IDisposable _subscription;
        private IReadOnlyDictionary<string, int> _hours = new Dictionary<string, int>();

        /// <summary>
        /// Creates the service and starts reading the weekly time budget.
        /// </summary>
        /// <param name="character">The player character.</param>
        /// <param name="scams">Scam catalog.</param>
        /// <param name="settings">Scam settings.</param>
        /// <param name="assets">Asset catalog (deposits can be drained).</param>
        /// <param name="luck">Chance calculator.</param>
        /// <param name="random">Random stream for scams.</param>
        /// <param name="bus">Event bus.</param>
        /// <param name="clock">Game clock (current week for player actions).</param>
        public ScamService(Character character, DefinitionCatalog<ScamDefinition> scams, ScamSettings settings, DefinitionCatalog<AssetDefinition> assets,
            LuckCalculator luck, IRandomProvider random, IEventBus bus, GameClock clock)
        {
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _scams = scams ?? throw new ArgumentNullException(nameof(scams));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _luck = luck ?? throw new ArgumentNullException(nameof(luck));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _subscription = _bus.Subscribe<TimeAllocationAppliedEvent>(e => _hours = e.Hours);
        }

        /// <summary>Scam catalog.</summary>
        public DefinitionCatalog<ScamDefinition> Scams => _scams;

        /// <summary>Scam settings (help page key...).</summary>
        public ScamSettings Settings => _settings;

        /// <summary>
        /// Explains how often a scam targets the character now.
        /// </summary>
        /// <param name="scamId">Scam id.</param>
        /// <returns>The breakdown.</returns>
        public ScamTargetingBreakdown Targeting(string scamId)
        {
            ScamDefinition scam = _scams.Get(scamId);
            GameDate now = _clock.Now;
            int online = _settings.OnlineActivityIds.Sum(id => _hours.TryGetValue(id, out int hours) ? hours : 0);
            float onlineMultiplier = Math.Min(_settings.MaxOnline, 1f + Math.Max(0, online - _settings.OnlineBaselineHours) * _settings.PerOnlineHour);
            decimal money = _character.Wallet.Cash + Deposits().Sum(h => h.Value);
            float wealth = 1f + scam.WealthWeight * (float)Math.Max(0.0, Math.Log10((double)(money / _settings.WealthReference)));
            var rules = new RuleContext(_character, now, _random, scam.NameKey);
            float situation = scam.Modifiers.Where(m => m.Condition.Evaluate(rules)).Aggregate(1f, (a, m) => a * m.Multiplier);
            float loneliness = _character.Catalogs.Stats.Contains(StatIds.Loneliness) ? _character.Stats.GetValue(StatIds.Loneliness) : 0f;
            return new ScamTargetingBreakdown(scam.MonthlyChance, new[]
            {
                (ScamTargetingBreakdown.AgeKey, scam.AgeMultiplier(_character.AgeAt(now))),
                (ScamTargetingBreakdown.LonelinessKey, 1f + loneliness / StatBounds.Max * scam.LonelinessWeight),
                (ScamTargetingBreakdown.OnlineKey, onlineMultiplier),
                (ScamTargetingBreakdown.WealthKey, wealth),
                (ScamTargetingBreakdown.SituationKey, situation),
            }, _settings.MaxMonthlyChance);
        }

        /// <summary>
        /// Explains the chance to spot a scam now.
        /// </summary>
        /// <param name="scamId">Scam id.</param>
        /// <returns>The quote.</returns>
        public ScamDetectionQuote Detection(string scamId)
        {
            ScamDefinition scam = _scams.Get(scamId);
            float stress = Math.Max(0f, _character.Stats.GetValue(StatIds.Stress) - Neutral) * _settings.StressPenaltyPerPoint;
            int warners = _character.Relationships.Living()
                .Count(r => r.Type != RelationshipType.Ex && r.Type != RelationshipType.Contact && r.Value >= _settings.SupportMinValue);
            float support = Math.Min(_settings.SupportMax, warners * _settings.SupportPerTie);
            ChanceBreakdown chance = _luck.Explain(scam.Detection.WithBonus(stress + support), _character, _clock.Now);
            return new ScamDetectionQuote(chance, stress, support);
        }

        /// <summary>
        /// Reports a scam to the police and the bank for a chance to get part of the money back (once per incident).
        /// </summary>
        /// <param name="incidentId">Incident id.</param>
        /// <returns>The result.</returns>
        public ScamReportResult Report(int incidentId)
        {
            ScamIncident incident = _character.Scams.Find(incidentId);
            if (incident == null || !_scams.TryGet(incident.ScamId, out ScamDefinition scam))
            {
                return new ScamReportResult(new[] { ScamReportResult.ReasonUnknown }, null, 0m);
            }
            if (incident.Reported)
            {
                return new ScamReportResult(new[] { ScamReportResult.ReasonReported }, null, 0m);
            }
            GameDate now = _clock.Now;
            int weeks = Math.Max(0, now.TotalWeeks - incident.Week.TotalWeeks);
            if (weeks > scam.Recovery.WindowWeeks)
            {
                return new ScamReportResult(new[] { ScamReportResult.ReasonTooLate }, null, 0m);
            }
            ChanceRoll roll = _luck.Roll(scam.Recovery.Chance.WithBonus(-scam.Recovery.DecayPerWeek * weeks), _character, now, _random);
            decimal recovered = roll.Success ? Money.Round(incident.Lost * scam.Recovery.Share) : 0m;
            if (recovered > 0m)
            {
                _character.Wallet.Deposit(recovered);
            }
            _character.Scams.Report(incident, recovered);
            _bus.Publish(new ScamReportedEvent(incident, recovered, now));
            return new ScamReportResult(null, roll, recovered);
        }

        /// <summary>
        /// Processes one month: each scam may target the character, who spots it or loses money.
        /// </summary>
        /// <param name="tick">Tick context of the month's first week.</param>
        public void ProcessMonth(TickContext tick)
        {
            if (tick == null)
            {
                throw new ArgumentNullException(nameof(tick));
            }
            GameDate week = tick.Week;
            int age = _character.AgeAt(week);
            foreach (ScamDefinition scam in _scams.All.Where(s => age >= s.MinAge))
            {
                if (!_character.IsAlive || !_random.Chance(Targeting(scam.Id).MonthlyChance))
                {
                    continue;
                }
                ScamDetectionQuote detection = Detection(scam.Id);
                bool spotted = _luck.Roll(scam.Detection.WithBonus(detection.StressAdjustment + detection.SupportAdjustment), _character, week, _random).Success;
                _character.Scams.RecordAttempt(spotted);
                if (spotted)
                {
                    Learn(_settings.DetectedGain, week);
                    _bus.Publish(new ScamDetectedEvent(scam.Id, week));
                    continue;
                }
                decimal lost = Take(scam.Loss);
                ScamIncident incident = _character.Scams.AddIncident(scam.Id, week, lost, _settings.MaxIncidents);
                scam.Effects.Apply(new RuleContext(_character, week, _random, scam.NameKey)).PublishTo(_bus);
                Learn(_settings.LessonGain, week);
                _bus.Publish(new ScammedEvent(incident, week));
                tick.RequestInterrupt(lost >= _settings.MajorLoss ? Severity.Major : Severity.Normal, ScammedInterruptKey, scam.Id);
            }
        }

        /// <summary>
        /// Stops reading the time budget.
        /// </summary>
        public void Dispose() => _subscription.Dispose();

        private decimal Take(ScamLoss loss)
        {
            decimal cash = _character.Wallet.Cash;
            List<Holding> deposits = Deposits().ToList();
            decimal savings = deposits.Sum(h => h.Value);
            decimal wanted = Math.Min(loss.Max, Math.Max(loss.Min, cash * loss.CashShare + savings * loss.SavingsShare));
            decimal target = Money.Round(Math.Min(wanted, cash + savings));
            decimal fromCash = Math.Min(cash, target);
            if (fromCash > 0m)
            {
                _character.Wallet.WithdrawUpTo(fromCash);
            }
            decimal remaining = target - fromCash;
            foreach (Holding holding in deposits)
            {
                if (remaining <= 0m)
                {
                    break;
                }
                decimal part = Math.Min(remaining, holding.Value);
                remaining -= holding.RemoveShare(part / holding.Value);
                if (holding.Value <= 0m)
                {
                    _character.Finances.Portfolio.Remove(holding.Id);
                }
            }
            return target - Math.Max(0m, remaining);
        }

        private IEnumerable<Holding> Deposits()
        {
            return _character.Finances.Portfolio.Holdings
                .Where(h => h.Value > 0m && _assets.TryGet(h.AssetId, out AssetDefinition asset) && asset.Class == AssetClass.Deposit);
        }

        private void Learn(float amount, GameDate week)
        {
            if (amount > 0f && _character.Catalogs.Stats.Contains(StatIds.StreetSmarts))
            {
                _character.ApplyStatChange(new StatChange(StatIds.StreetSmarts, amount, LessonKey), week);
            }
        }
    }
}
