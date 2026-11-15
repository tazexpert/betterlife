using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Localization;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time;
using BetterLife.Core.Vices;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Builds the Home dashboard: core stat cards with trend and sparkline, habit cards and the inbox shortcut.
    /// </summary>
    public static class DashboardViewModel
    {
        /// <summary>Inbox shortcut key (placeholder <c>{count}</c>).</summary>
        public const string InboxKey = "dashboard.inbox";
        /// <summary>Habit status key while hooked (placeholder <c>{value}</c>).</summary>
        public const string AddictionKey = "habit.addiction";
        /// <summary>Habit status key while quitting (placeholder <c>{weeks}</c> clean weeks).</summary>
        public const string QuittingKey = "habit.quitting";
        /// <summary>Format of stat values.</summary>
        public const string ValueFormat = "0";
        /// <summary>Format of signed changes.</summary>
        public const string DeltaFormat = "+0.0;-0.0";

        /// <summary>
        /// Builds the dashboard.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="localizer">Localizer.</param>
        /// <returns>The state.</returns>
        public static DashboardState Build(LifeSession session, ILocalizer localizer)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (localizer == null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }
            Character character = session.Character;
            GameDate now = session.TimeState.Clock.Now;
            List<StatCardState> cards = character.History.TrackedStats.Select(id => Card(character, id, localizer)).ToList();
            List<HabitCardState> habits = session.Vices.VisibleHabits().Select(state => Habit(session, state, now, localizer)).ToList();
            int pending = session.Events.Inbox.Count;
            string inbox = localizer.Format(InboxKey, ("count", pending.ToString(CultureInfo.InvariantCulture)));
            return new DashboardState(cards, habits, pending, inbox);
        }

        private static StatCardState Card(Character character, StatId id, ILocalizer localizer)
        {
            StatDefinition definition = character.Catalogs.Stats.GetDefinition(id);
            IReadOnlyList<float> samples = character.History.GetSamples(id);
            float value = character.Stats.GetValue(id);
            float delta = Sparkline.Delta(samples);
            TrendDirection direction = Sparkline.Direction(delta);
            bool improving = direction != TrendDirection.Flat
                && (direction == TrendDirection.Up) == (definition.Polarity == StatPolarity.HigherIsBetter);
            string deltaText = direction == TrendDirection.Flat ? string.Empty : delta.ToString(DeltaFormat, CultureInfo.InvariantCulture);
            return new StatCardState(id, localizer.Get(definition.NameKey), value, value.ToString(ValueFormat, CultureInfo.InvariantCulture),
                delta, deltaText, direction, improving, Sparkline.Points(samples));
        }

        private static HabitCardState Habit(LifeSession session, ViceState state, GameDate now, ILocalizer localizer)
        {
            string name = session.Vices.Vices.TryGet(state.ViceId, out ViceDefinition vice) ? localizer.Get(vice.NameKey) : state.ViceId;
            bool quitting = state.Quitting != null;
            string status = quitting
                ? localizer.Format(QuittingKey, ("weeks", state.CleanWeeks(now).ToString(CultureInfo.InvariantCulture)))
                : localizer.Format(AddictionKey, ("value", state.Addiction.ToString(ValueFormat, CultureInfo.InvariantCulture)));
            return new HabitCardState(state.ViceId, name, state.Addiction, status, quitting);
        }
    }
}
