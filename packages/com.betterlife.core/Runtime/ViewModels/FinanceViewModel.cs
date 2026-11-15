using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Economy;
using BetterLife.Core.Localization;
using BetterLife.Core.Scams;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Builds the Money tab: overview, economy, holdings, debts, last month's report, lifestyle and scam incidents.
    /// All money is shown at today's prices.
    /// </summary>
    public static class FinanceViewModel
    {
        /// <summary>Summary label keys.</summary>
        public const string CashKey = "finance.cash";
        /// <summary>Total investments label.</summary>
        public const string InvestmentsKey = "finance.investments";
        /// <summary>Total debt label.</summary>
        public const string DebtsKey = "finance.debts";
        /// <summary>Monthly income label.</summary>
        public const string IncomeKey = "finance.monthly_income";
        /// <summary>Economy phase label.</summary>
        public const string PhaseKey = "economy.phase";
        /// <summary>Inflation label.</summary>
        public const string InflationLabelKey = "economy.inflation";
        /// <summary>Policy rate label.</summary>
        public const string RateKey = "economy.policy_rate";
        /// <summary>Holding detail (placeholder <c>{gain}</c>).</summary>
        public const string GainKey = "finance.holding.gain";
        /// <summary>Debt detail (placeholders <c>{payment}</c>, <c>{rate}</c>).</summary>
        public const string DebtDetailKey = "finance.debt.detail";
        /// <summary>Detail of a defaulted debt.</summary>
        public const string DefaultedKey = "finance.debt.defaulted";
        /// <summary>Report category key prefix.</summary>
        public const string CategoryPrefix = "finance.category.";
        /// <summary>Net change label of the report.</summary>
        public const string NetKey = "finance.report.net";
        /// <summary>Note when there is no report yet.</summary>
        public const string NoReportKey = "finance.report.none";
        /// <summary>Monthly cost text (placeholder <c>{value}</c>).</summary>
        public const string PerMonthKey = "finance.per_month";
        /// <summary>Yearly cost text (placeholder <c>{value}</c>).</summary>
        public const string PerYearKey = "finance.per_year";
        /// <summary>Scam detail (placeholders <c>{date}</c>, <c>{lost}</c>).</summary>
        public const string ScamDetailKey = "scam.incident.detail";
        /// <summary>Recovered part (placeholder <c>{value}</c>).</summary>
        public const string ScamRecoveredKey = "scam.incident.recovered";
        /// <summary>Note of an incident reported without luck.</summary>
        public const string ScamReportedKey = "scam.incident.reported";
        /// <summary>Report button (placeholder <c>{chance}</c>).</summary>
        public const string ScamReportKey = "scam.report.action";

        private const float Hundred = 100f;

        /// <summary>
        /// Builds the Money tab.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="money">Money formatter.</param>
        /// <returns>The state.</returns>
        public static FinanceState Build(LifeSession session, ILocalizer localizer, MoneyFormatter money)
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
            CharacterFinances finances = character.Finances;
            GameDate now = session.TimeState.Clock.Now;
            string Money(decimal real) => SessionText.Money(session, money, real);

            var summary = new List<ValueRow>
            {
                new ValueRow(localizer.Get(CashKey), Money(character.Wallet.Cash)),
                new ValueRow(localizer.Get(InvestmentsKey), Money(finances.Portfolio.TotalValue)),
                new ValueRow(localizer.Get(DebtsKey), Money(finances.Debts.TotalBalance), string.Empty,
                    finances.Debts.TotalBalance > 0m ? RowTone.Bad : RowTone.Neutral),
                new ValueRow(localizer.Get(FinanceService.NetWorthKey), Money(character.NetWorth), string.Empty, ValueRow.ToneOf(character.NetWorth)),
                new ValueRow(localizer.Get(IncomeKey), Money(finances.LastMonthlyIncome)),
            };

            EconomyState state = session.Economy.State;
            var economy = new List<ValueRow>
            {
                new ValueRow(localizer.Get(PhaseKey), localizer.Get(session.Economy.Phase.NameKey)),
                new ValueRow(localizer.Get(InflationLabelKey), SessionText.Percent(state.AnnualInflation * Hundred, localizer)),
                new ValueRow(localizer.Get(RateKey), SessionText.Percent(state.PolicyRate * Hundred, localizer)),
            };

            List<ValueRow> holdings = finances.Portfolio.Holdings
                .OrderByDescending(h => h.Value)
                .Select(h => new ValueRow(
                    session.Finance.Assets.TryGet(h.AssetId, out AssetDefinition asset) ? localizer.Get(asset.NameKey) : h.AssetId,
                    Money(h.Value),
                    localizer.Format(GainKey, ("gain", Signed(session, money, h.Gain))),
                    ValueRow.ToneOf(h.Gain)))
                .ToList();

            List<ValueRow> debts = finances.Debts.Debts
                .OrderByDescending(d => d.Balance)
                .Select(d => new ValueRow(
                    session.Finance.Products.TryGet(d.ProductId, out DebtProductDefinition product) ? localizer.Get(product.NameKey) : d.ProductId,
                    Money(d.Balance),
                    d.IsDefaulted
                        ? localizer.Get(DefaultedKey)
                        : localizer.Format(DebtDetailKey, ("payment", Money(d.MonthlyPayment)), ("rate", SessionText.Percent(d.AnnualRate * Hundred, localizer))),
                    RowTone.Bad))
                .ToList();

            FinanceMonthReport last = session.Finance.LastReport;
            var report = new List<ValueRow>();
            string note = localizer.Get(NoReportKey);
            if (last != null)
            {
                note = string.Empty;
                foreach (IGrouping<FinanceLineCategory, FinanceLine> group in last.Lines.GroupBy(l => l.Category).OrderBy(g => g.Key))
                {
                    decimal total = group.Sum(l => l.Amount);
                    string detail = string.Join(", ", group.GroupBy(l => l.SourceKey).Select(g => localizer.Get(g.Key)));
                    report.Add(new ValueRow(localizer.Get(SessionText.EnumKey(CategoryPrefix, group.Key)), Signed(session, money, total), detail, ValueRow.ToneOf(total)));
                }
                decimal net = last.Lines.Sum(l => l.Amount);
                report.Add(new ValueRow(localizer.Get(NetKey), Signed(session, money, net), string.Empty, ValueRow.ToneOf(net)));
            }

            List<LifestyleOption> lifestyles = session.Finance.LifestyleTiers.All
                .OrderBy(t => t.MonthlyCost)
                .Select(t => new LifestyleOption(t.Id, localizer.Get(t.NameKey),
                    localizer.Format(PerMonthKey, ("value", Money(session.Finance.LifestyleCost(t.Id)))), t.Id == finances.LifestyleTierId))
                .ToList();

            List<ScamIncidentRow> scams = character.Scams.Incidents
                .OrderByDescending(i => i.Week)
                .Select(i => Incident(session, i, now, localizer, money))
                .ToList();

            return new FinanceState(summary, economy, holdings, debts, report, note, lifestyles, session.Finance.CanChooseLifestyle(now), scams);
        }

        /// <summary>
        /// Estimated chance to recover money by reporting an incident now (the same formula as <see cref="ScamService.Report"/>,
        /// before luck and karma), or null when it can no longer be reported.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="incident">Incident.</param>
        /// <param name="now">Current week.</param>
        /// <returns>Chance in percent, or null.</returns>
        public static float? ReportChance(LifeSession session, ScamIncident incident, GameDate now)
        {
            if (incident.Reported || !session.Scams.Scams.TryGet(incident.ScamId, out ScamDefinition scam))
            {
                return null;
            }
            int weeks = Math.Max(0, now.TotalWeeks - incident.Week.TotalWeeks);
            if (weeks > scam.Recovery.WindowWeeks)
            {
                return null;
            }
            float chance = scam.Recovery.Chance.BasePercent - scam.Recovery.DecayPerWeek * weeks;
            return Math.Max(0f, Math.Min(Hundred, chance));
        }

        /// <summary>Report result title.</summary>
        public const string ReportTitleKey = "scam.report.title";
        /// <summary>Report result line when money came back (placeholder <c>{value}</c>).</summary>
        public const string ReportRecoveredKey = "scam.report.recovered";
        /// <summary>Report result line when nothing came back.</summary>
        public const string ReportNothingKey = "scam.report.nothing";

        /// <summary>
        /// Builds the result of a scam report.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="result">Report result.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="money">Money formatter.</param>
        /// <returns>The state.</returns>
        public static ActionResultState BuildReportResult(LifeSession session, ScamReportResult result, ILocalizer localizer, MoneyFormatter money)
        {
            if (session == null || result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            var lines = new List<string>();
            if (!result.Reported)
            {
                lines.AddRange(result.ReasonKeys.Select(localizer.Get));
                return new ActionResultState(localizer.Get(ReportTitleKey), false, lines);
            }
            if (result.Roll != null)
            {
                lines.Add(localizer.Format(result.Roll.Success ? EventDialogViewModel.RollSuccessKey : EventDialogViewModel.RollFailureKey,
                    ("value", result.Roll.Breakdown.Final.ToString(EventDialogViewModel.ChanceFormat, CultureInfo.InvariantCulture))));
            }
            lines.Add(result.Recovered > 0m
                ? localizer.Format(ReportRecoveredKey, ("value", SessionText.Money(session, money, result.Recovered)))
                : localizer.Get(ReportNothingKey));
            return new ActionResultState(localizer.Get(ReportTitleKey), result.Recovered > 0m, lines);
        }

        private static ScamIncidentRow Incident(LifeSession session, ScamIncident incident, GameDate now, ILocalizer localizer, MoneyFormatter money)
        {
            string name = session.Scams.Scams.TryGet(incident.ScamId, out ScamDefinition scam) ? localizer.Get(scam.NameKey) : incident.ScamId;
            string detail = localizer.Format(ScamDetailKey, ("date", GameDateText.MonthYear(incident.Week, localizer)),
                ("lost", SessionText.Money(session, money, incident.Lost)));
            if (incident.Recovered > 0m)
            {
                detail += " · " + localizer.Format(ScamRecoveredKey, ("value", SessionText.Money(session, money, incident.Recovered)));
            }
            else if (incident.Reported)
            {
                detail += " · " + localizer.Get(ScamReportedKey);
            }
            float? chance = ReportChance(session, incident, now);
            string report = chance.HasValue
                ? localizer.Format(ScamReportKey, ("chance", chance.Value.ToString(SessionText.PercentFormat, CultureInfo.InvariantCulture)))
                : null;
            return new ScamIncidentRow(incident.Id, name, detail, chance.HasValue, report);
        }

        private static string Signed(LifeSession session, MoneyFormatter money, decimal real)
        {
            string text = SessionText.Money(session, money, real);
            return real > 0m ? "+" + text : text;
        }
    }
}
