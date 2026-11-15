using System.Globalization;
using BetterLife.Core.Navigation;
using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// People tab: everyone in the character's life, grouped; tap a person to open their screen.
    /// </summary>
    public sealed class PeopleScreenPresenter : ListScreenPresenter
    {
        /// <summary>
        /// Creates the screen.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public PeopleScreenPresenter(ScreenContext context)
            : base(context, "people.title")
        {
        }

        /// <inheritdoc />
        protected override void Fill(LifeSession session)
        {
            var groups = PeopleViewModel.Build(session, Context.Localizer);
            if (groups.Count == 0)
            {
                Content.Add(UiRows.Text(Context.Localizer.Get(PeopleViewModel.EmptyKey), "list-screen__empty"));
                return;
            }
            foreach (PeopleGroup group in groups)
            {
                Content.Add(UiRows.Section(group.Title));
                foreach (PersonRow person in group.People)
                {
                    string argument = person.NpcId.ToString(CultureInfo.InvariantCulture);
                    RowTone tone = !person.IsAlive ? RowTone.Neutral : person.Value >= 0f ? RowTone.Good : RowTone.Bad;
                    var row = UiRows.Row(person.Name, person.ValueText, person.Detail, tone,
                        () => Context.Router.Push(new ScreenRequest(ScreenIds.Person, argument)));
                    row.EnableInClassList("value-row--departed", !person.IsAlive);
                    Content.Add(row);
                }
            }
        }
    }
}
