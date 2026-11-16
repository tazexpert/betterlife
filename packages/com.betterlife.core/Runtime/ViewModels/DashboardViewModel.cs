using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Localization;
using BetterLife.Core.Relationships;
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
            return new DashboardState(cards, habits, pending, inbox, Family(character, now, localizer), AvatarPaths.Of(character, now));
        }

        /// <summary>Status label.</summary>
        public const string StatusKey = "family.status";
        /// <summary>Married (placeholder <c>{name}</c>).</summary>
        public const string MarriedKey = "family.status.married";
        /// <summary>Dating (placeholder <c>{name}</c>).</summary>
        public const string DatingKey = "family.status.dating";
        /// <summary>Widowed.</summary>
        public const string WidowedKey = "family.status.widowed";
        /// <summary>Single after a relationship ended.</summary>
        public const string SingleAgainKey = "family.status.single_again";
        /// <summary>Single.</summary>
        public const string SingleKey = "family.status.single";
        /// <summary>Children label.</summary>
        public const string ChildrenKey = "family.children";
        /// <summary>Children count (placeholder <c>{count}</c>).</summary>
        public const string ChildrenCountKey = "family.children.count";
        /// <summary>Children who passed away (placeholder <c>{count}</c>).</summary>
        public const string ChildrenLostKey = "family.children.lost";
        /// <summary>No children.</summary>
        public const string NoChildrenKey = "family.children.none";

        /// <summary>
        /// Relationship status (married, dating, widowed, single) and the number of children.
        /// </summary>
        /// <param name="character">Character.</param>
        /// <param name="now">Current week.</param>
        /// <param name="localizer">Localizer.</param>
        /// <returns>Two rows: status, children.</returns>
        public static IReadOnlyList<ValueRow> Family(Character character, GameDate now, ILocalizer localizer)
        {
            CharacterRelationships people = character.Relationships;
            Relationship spouse = people.Living(RelationshipType.Spouse).FirstOrDefault();
            Relationship partner = people.Living(RelationshipType.Partner).OrderByDescending(r => r.Value).FirstOrDefault();
            string status;
            if (spouse != null)
            {
                status = localizer.Format(MarriedKey, ("name", people.Npc(spouse.NpcId).Name.GivenName));
            }
            else if (partner != null)
            {
                status = localizer.Format(DatingKey, ("name", people.Npc(partner.NpcId).Name.GivenName));
            }
            else if (people.All.Any(r => r.Type == RelationshipType.Spouse))
            {
                status = localizer.Get(WidowedKey);
            }
            else if (people.All.Any(r => r.Type == RelationshipType.Ex))
            {
                status = localizer.Get(SingleAgainKey);
            }
            else
            {
                status = localizer.Get(SingleKey);
            }

            var children = people.All.Where(r => r.Type == RelationshipType.Child).Select(r => people.Npc(r.NpcId)).Where(n => n != null).ToList();
            int alive = children.Count(n => n.IsAlive);
            int lost = children.Count - alive;
            string kids = children.Count == 0
                ? localizer.Get(NoChildrenKey)
                : localizer.Format(ChildrenCountKey, ("count", alive.ToString(CultureInfo.InvariantCulture)));
            string detail = lost > 0 ? localizer.Format(ChildrenLostKey, ("count", lost.ToString(CultureInfo.InvariantCulture))) : string.Empty;
            return new[]
            {
                new ValueRow(localizer.Get(StatusKey), status),
                new ValueRow(localizer.Get(ChildrenKey), kids, detail),
            };
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
