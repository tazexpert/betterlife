using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Localization;
using BetterLife.Core.Relationships;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Builds the People tab (grouped list) and the person screen (closeness, compatibility, memories, interactions).
    /// </summary>
    public static class PeopleViewModel
    {
        /// <summary>Relationship type key prefix (<c>relationship.type.spouse</c>...).</summary>
        public const string TypePrefix = "relationship.type.";
        /// <summary>Group title keys.</summary>
        public const string FamilyKey = "people.family";
        /// <summary>Love group title.</summary>
        public const string LoveKey = "people.love";
        /// <summary>Friends group title.</summary>
        public const string FriendsKey = "people.friends";
        /// <summary>Others group title.</summary>
        public const string OthersKey = "people.others";
        /// <summary>Empty list text.</summary>
        public const string EmptyKey = "people.empty";
        /// <summary>Row detail (placeholders <c>{type}</c>, <c>{age}</c>).</summary>
        public const string RowDetailKey = "people.row";
        /// <summary>Detail of someone who passed away (placeholder <c>{type}</c>).</summary>
        public const string DepartedKey = "people.departed";
        /// <summary>Person detail line (placeholders <c>{type}</c>, <c>{age}</c>, <c>{since}</c>).</summary>
        public const string PersonDetailKey = "person.detail";
        /// <summary>Compatibility total label.</summary>
        public const string CompatibilityKey = "person.compatibility";
        /// <summary>Interaction detail (placeholders <c>{chance}</c>, <c>{cost}</c>).</summary>
        public const string ChanceCostKey = "interaction.chance_cost";
        /// <summary>Interaction detail without a roll (placeholder <c>{cost}</c>).</summary>
        public const string CostKey = "interaction.cost";
        /// <summary>Interaction detail when free and sure.</summary>
        public const string FreeKey = "interaction.free";
        /// <summary>Interaction detail with a roll and no cost (placeholder <c>{chance}</c>).</summary>
        public const string ChanceKey = "interaction.chance";
        /// <summary>Format of closeness and points.</summary>
        public const string NumberFormat = "+0;-0;0";

        private static readonly RelationshipType[] Family = { RelationshipType.Parent, RelationshipType.Sibling, RelationshipType.Child };
        private static readonly RelationshipType[] Love = { RelationshipType.Spouse, RelationshipType.Partner };
        private static readonly RelationshipType[] Friends = { RelationshipType.Friend, RelationshipType.Colleague };
        private static readonly RelationshipType[] Others = { RelationshipType.Ex, RelationshipType.Contact };

        /// <summary>
        /// Builds the grouped list; empty groups are left out.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="localizer">Localizer.</param>
        /// <returns>Groups in display order.</returns>
        public static IReadOnlyList<PeopleGroup> Build(LifeSession session, ILocalizer localizer)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (localizer == null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }
            GameDate now = session.TimeState.Clock.Now;
            var groups = new List<PeopleGroup>();
            foreach ((string key, RelationshipType[] types) in new[] { (FamilyKey, Family), (LoveKey, Love), (FriendsKey, Friends), (OthersKey, Others) })
            {
                List<PersonRow> rows = session.Character.Relationships.All
                    .Where(r => types.Contains(r.Type))
                    .Select(r => (Relationship: r, Npc: session.Character.Relationships.Npc(r.NpcId)))
                    .Where(p => p.Npc != null)
                    .OrderByDescending(p => p.Npc.IsAlive)
                    .ThenByDescending(p => p.Relationship.Value)
                    .Select(p => Row(p.Relationship, p.Npc, now, localizer))
                    .ToList();
                if (rows.Count > 0)
                {
                    groups.Add(new PeopleGroup(localizer.Get(key), rows));
                }
            }
            return groups;
        }

        /// <summary>
        /// Builds one person's screen.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="npcId">NPC id.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="money">Money formatter.</param>
        /// <returns>The state, or null when the person is unknown.</returns>
        public static PersonState BuildPerson(LifeSession session, int npcId, ILocalizer localizer, MoneyFormatter money)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (localizer == null || money == null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }
            Relationship relationship = session.Character.Relationships.With(npcId);
            Npc npc = session.Character.Relationships.Npc(npcId);
            if (relationship == null || npc == null)
            {
                return null;
            }
            GameDate now = session.TimeState.Clock.Now;
            string type = TypeName(relationship.Type, localizer);
            string detail = localizer.Format(PersonDetailKey, ("type", type), ("age", AgeOf(npc, now)),
                ("since", GameDateText.MonthYear(relationship.Since, localizer)));

            var compatibility = new List<ValueRow>();
            var interactions = new List<InteractionOption>();
            if (npc.IsAlive)
            {
                CompatibilityBreakdown fit = session.Relationships.Compatibility(npcId);
                compatibility.Add(new ValueRow(localizer.Get(CompatibilityKey), fit.Score.ToString(DashboardViewModel.ValueFormat, CultureInfo.InvariantCulture)));
                compatibility.AddRange(fit.Lines.Select(l => new ValueRow(localizer.Get(l.Key), l.Points.ToString(NumberFormat, CultureInfo.InvariantCulture),
                    string.Empty, ValueRow.ToneOf(l.Points))));
                interactions.AddRange(session.Relationships.Options(npcId).Select(o => Option(session, o.Interaction, o.Quote, localizer, money)));
            }

            List<ValueRow> memories = relationship.Memories
                .OrderByDescending(m => m.Week)
                .Select(m => new ValueRow(localizer.Get(m.Key), m.Delta.ToString(NumberFormat, CultureInfo.InvariantCulture),
                    GameDateText.MonthYear(m.Week, localizer), ValueRow.ToneOf(m.Delta)))
                .ToList();

            return new PersonState(npcId, npc.Name.GivenName + " " + npc.Name.FamilyName, detail,
                relationship.Value.ToString(NumberFormat, CultureInfo.InvariantCulture), compatibility, memories, interactions);
        }

        /// <summary>
        /// Builds the result of an interaction.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="interactionId">Interaction id.</param>
        /// <param name="result">The result.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="money">Money formatter.</param>
        /// <returns>The state.</returns>
        public static ActionResultState BuildResult(LifeSession session, string interactionId, InteractionResult result, ILocalizer localizer, MoneyFormatter money)
        {
            if (session == null || result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            string name = session.Relationships.Interactions.TryGet(interactionId, out InteractionDefinition interaction) ? localizer.Get(interaction.NameKey) : interactionId;
            var lines = new List<string>();
            if (!result.Performed)
            {
                lines.AddRange(result.Quote.ReasonKeys.Select(localizer.Get));
                return new ActionResultState(name, false, lines);
            }
            if (result.Roll != null)
            {
                lines.Add(localizer.Format(result.Success ? EventDialogViewModel.RollSuccessKey : EventDialogViewModel.RollFailureKey,
                    ("value", result.Roll.Breakdown.Final.ToString(EventDialogViewModel.ChanceFormat, CultureInfo.InvariantCulture))));
            }
            if (result.ValueChange != 0f)
            {
                lines.Add(localizer.Format(ResultValueKey, ("value", result.ValueChange.ToString(NumberFormat, CultureInfo.InvariantCulture))));
            }
            if (result.NewType.HasValue)
            {
                lines.Add(localizer.Format(ResultTypeKey, ("type", TypeName(result.NewType.Value, localizer))));
            }
            if (result.Child != null)
            {
                lines.Add(localizer.Format(ResultChildKey, ("name", result.Child.Name.GivenName)));
            }
            lines.AddRange(new RuleTextFormatter(localizer, money, session.Economy.State.Nominal).FormatAll(result.Effects?.Lines));
            return new ActionResultState(name, result.Success, lines);
        }

        /// <summary>Result line of a closeness change (placeholder <c>{value}</c>).</summary>
        public const string ResultValueKey = "interaction.result.value";
        /// <summary>Result line of a new relationship type (placeholder <c>{type}</c>).</summary>
        public const string ResultTypeKey = "interaction.result.type";
        /// <summary>Result line of a new child (placeholder <c>{name}</c>).</summary>
        public const string ResultChildKey = "interaction.result.child";

        /// <summary>
        /// Name of a relationship type.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="localizer">Localizer.</param>
        /// <returns>The text.</returns>
        public static string TypeName(RelationshipType type, ILocalizer localizer) => localizer.Get(TypePrefix + RelationshipNames.NameOf(type));

        private static PersonRow Row(Relationship relationship, Npc npc, GameDate now, ILocalizer localizer)
        {
            string type = TypeName(relationship.Type, localizer);
            string detail = npc.IsAlive
                ? localizer.Format(RowDetailKey, ("type", type), ("age", AgeOf(npc, now)))
                : localizer.Format(DepartedKey, ("type", type));
            return new PersonRow(npc.Id, npc.Name.GivenName + " " + npc.Name.FamilyName, detail, relationship.Value,
                relationship.Value.ToString(NumberFormat, CultureInfo.InvariantCulture), npc.IsAlive);
        }

        private static string AgeOf(Npc npc, GameDate now) => npc.AgeAt(npc.DiedOn ?? now).ToString(CultureInfo.InvariantCulture);

        private static InteractionOption Option(LifeSession session, InteractionDefinition interaction, InteractionQuote quote, ILocalizer localizer, MoneyFormatter money)
        {
            string detail;
            if (!quote.Available)
            {
                detail = string.Join(" · ", quote.ReasonKeys.Select(localizer.Get));
            }
            else
            {
                string chance = quote.Chance?.Final.ToString(EventDialogViewModel.ChanceFormat, CultureInfo.InvariantCulture);
                string cost = quote.Cost > 0m ? SessionText.Money(session, money, quote.Cost) : null;
                detail = chance != null && cost != null ? localizer.Format(ChanceCostKey, ("chance", chance), ("cost", cost))
                    : chance != null ? localizer.Format(ChanceKey, ("chance", chance))
                    : cost != null ? localizer.Format(CostKey, ("cost", cost))
                    : localizer.Get(FreeKey);
            }
            return new InteractionOption(interaction.Id, localizer.Get(interaction.NameKey), quote.Available, detail);
        }
    }
}
