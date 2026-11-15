using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Skills;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Localization;
using BetterLife.Core.Randomness;
using BetterLife.Core.Rules;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Budget;
using BetterLife.Core.Time.Ticking.Phases;
using BetterLife.Core.Vices;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Builds the Life tab: the weekly hour plan with what each activity does, problems with the plan, and the
    /// habits the character can try to quit.
    /// </summary>
    public static class LifePlanViewModel
    {
        /// <summary>Summary of the default activity (placeholders <c>{hours}</c>, <c>{activity}</c>).</summary>
        public const string SummaryKey = "life.free_hours";
        /// <summary>Hours text (placeholder <c>{hours}</c>).</summary>
        public const string HoursKey = "life.hours";
        /// <summary>Note of the default activity.</summary>
        public const string DefaultNoteKey = "life.note.default";
        /// <summary>Note of school hours.</summary>
        public const string SchoolNoteKey = "life.note.school";
        /// <summary>Note of work hours.</summary>
        public const string WorkNoteKey = "life.note.work";
        /// <summary>Weekly cost of an activity (placeholder <c>{value}</c>).</summary>
        public const string CostKey = "life.cost";
        /// <summary>Issue key prefix (<c>life.issue.overweek</c>...; placeholders <c>{activity}</c>, <c>{limit}</c>).</summary>
        public const string IssuePrefix = "life.issue.";
        /// <summary>Quit option detail (placeholders <c>{cost}</c>, <c>{weeks}</c>).</summary>
        public const string QuitDetailKey = "habit.quit.detail";
        /// <summary>Quit result title (placeholder <c>{method}</c>).</summary>
        public const string QuitStartedKey = "habit.quit.started";
        /// <summary>Quit could not start.</summary>
        public const string QuitFailedKey = "habit.quit.failed";

        private const ulong PreviewSeed = 0x5EEDUL;
        private const string NumberFormat = "0.#";

        /// <summary>
        /// Builds the Life tab.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="money">Money formatter.</param>
        /// <returns>The state.</returns>
        public static LifePlanState Build(LifeSession session, ILocalizer localizer, MoneyFormatter money)
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
            TimeBudget budget = session.TimeState.Budget;
            // A private generator: checking activity conditions must not draw from the life's saved random streams.
            var rules = new RuleContext(character, now, new Pcg32(PreviewSeed, PreviewSeed), TimeAllocationPhase.SourceKey);
            var formatter = new RuleTextFormatter(localizer, money, session.Economy.State.Nominal);
            string defaultId = budget.DefaultActivityId;

            var rows = new List<ActivityRow>();
            string defaultName = string.Empty;
            foreach (ActivityDefinition activity in budget.Activities.All)
            {
                if (!activity.IsAvailable(rules) && budget.GetHours(activity.Id) == 0)
                {
                    continue;
                }
                int hours = budget.GetHours(activity.Id);
                string note = string.Empty;
                if (activity.Id == defaultId)
                {
                    note = localizer.Get(DefaultNoteKey);
                    defaultName = localizer.Get(activity.NameKey);
                }
                else if (activity.Id == session.Education.SchoolActivityId)
                {
                    note = localizer.Get(SchoolNoteKey);
                }
                else if (activity.Id == session.Career.WorkActivityId)
                {
                    note = localizer.Get(WorkNoteKey);
                }
                bool editable = note.Length == 0 && activity.IsAvailable(rules);
                int max = Math.Min(activity.MaxHours, hours + Math.Max(0, budget.FreeHours));
                rows.Add(new ActivityRow(activity.Id, localizer.Get(activity.NameKey), localizer.Get(activity.DescKey), hours,
                    localizer.Format(HoursKey, ("hours", hours.ToString(CultureInfo.InvariantCulture))), editable, note, max,
                    Effects(session, activity, hours, formatter, localizer, money)));
            }

            List<string> issues = budget.Validate(rules)
                .Select(issue => localizer.Format(SessionText.EnumKey(IssuePrefix, issue.Kind),
                    ("activity", issue.ActivityId == null ? string.Empty : ActivityName(session, issue.ActivityId, localizer)),
                    ("limit", issue.Limit.ToString(CultureInfo.InvariantCulture))))
                .ToList();

            List<OfferGroup> habits = session.Vices.VisibleHabits().Select(state => Habit(session, state, now, localizer, money)).ToList();
            string summary = localizer.Format(SummaryKey, ("hours", Math.Max(0, budget.FreeHours).ToString(CultureInfo.InvariantCulture)), ("activity", defaultName));
            return new LifePlanState(summary, rows, issues, habits);
        }

        /// <summary>
        /// Builds the result of starting to quit a habit.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="methodId">Quit method id.</param>
        /// <param name="quote">Quote returned by the start.</param>
        /// <param name="localizer">Localizer.</param>
        /// <returns>The state.</returns>
        public static ActionResultState BuildQuitResult(LifeSession session, string methodId, QuitQuote quote, ILocalizer localizer)
        {
            if (session == null || quote == null)
            {
                throw new ArgumentNullException(nameof(quote));
            }
            string method = session.Vices.Methods.TryGet(methodId, out QuitMethodDefinition definition) ? localizer.Get(definition.NameKey) : methodId;
            return quote.Available
                ? new ActionResultState(localizer.Format(QuitStartedKey, ("method", method)), true, new string[0])
                : new ActionResultState(localizer.Get(QuitFailedKey), false, quote.ReasonKeys.Select(localizer.Get).ToList());
        }

        private static IReadOnlyList<string> Effects(LifeSession session, ActivityDefinition activity, int hours, RuleTextFormatter formatter,
            ILocalizer localizer, MoneyFormatter money)
        {
            var lines = new List<string>();
            if (hours <= 0)
            {
                return lines;
            }
            StatCatalog stats = session.Character.Catalogs.Stats;
            foreach (ActivityStatEffect effect in activity.Stats)
            {
                float total = effect.TotalFor(hours);
                if (total == 0f || !stats.Contains(effect.StatId) || stats.GetDefinition(effect.StatId).Layer == StatLayer.Hidden)
                {
                    continue;
                }
                lines.Add(formatter.Format(RuleText.Of(RuleTextKeys.EffectStat, (RuleTextKeys.ArgSubjectKey, stats.GetDefinition(effect.StatId).NameKey),
                    (RuleTextKeys.ArgValue, total.ToString(NumberFormat, CultureInfo.InvariantCulture)))));
            }
            foreach (ActivitySkillEffect effect in activity.Skills)
            {
                float total = effect.TotalFor(hours);
                if (total == 0f || !session.Character.Catalogs.Skills.TryGet(effect.SkillId, out SkillDefinition skill))
                {
                    continue;
                }
                lines.Add(formatter.Format(RuleText.Of(RuleTextKeys.EffectSkill, (RuleTextKeys.ArgSubjectKey, skill.NameKey),
                    (RuleTextKeys.ArgValue, total.ToString(NumberFormat, CultureInfo.InvariantCulture)))));
            }
            if (activity.CostPerHour > 0m)
            {
                lines.Add(localizer.Format(CostKey, ("value", SessionText.Money(session, money, activity.CostPerHour * hours))));
            }
            return lines;
        }

        private static OfferGroup Habit(LifeSession session, ViceState state, GameDate now, ILocalizer localizer, MoneyFormatter money)
        {
            string name = session.Vices.Vices.TryGet(state.ViceId, out ViceDefinition vice) ? localizer.Get(vice.NameKey) : state.ViceId;
            string status = state.Quitting != null
                ? localizer.Format(DashboardViewModel.QuittingKey, ("weeks", state.CleanWeeks(now).ToString(CultureInfo.InvariantCulture)))
                : localizer.Format(DashboardViewModel.AddictionKey, ("value", state.Addiction.ToString(DashboardViewModel.ValueFormat, CultureInfo.InvariantCulture)));
            List<AmountChoice> methods = state.Quitting != null
                ? new List<AmountChoice>()
                : session.Vices.Methods.All
                    .Where(m => m.AppliesTo(state.ViceId))
                    .Select(m => Method(session, state.ViceId, m, localizer, money))
                    .ToList();
            return new OfferGroup(state.ViceId, name, status, methods);
        }

        private static AmountChoice Method(LifeSession session, string viceId, QuitMethodDefinition method, ILocalizer localizer, MoneyFormatter money)
        {
            QuitQuote quote = session.Vices.QuoteQuit(viceId, method.Id);
            string detail = quote.Available
                ? localizer.Format(QuitDetailKey, ("cost", SessionText.Money(session, money, quote.Cost)),
                    ("weeks", quote.DurationWeeks.ToString(CultureInfo.InvariantCulture)))
                : string.Join(" · ", quote.ReasonKeys.Select(localizer.Get));
            return new AmountChoice(method.Id, localizer.Get(method.NameKey), 0m, quote.Available, detail);
        }

        private static string ActivityName(LifeSession session, string activityId, ILocalizer localizer)
        {
            return session.TimeState.Budget.Activities.TryGet(activityId, out ActivityDefinition activity) ? localizer.Get(activity.NameKey) : activityId;
        }
    }
}
