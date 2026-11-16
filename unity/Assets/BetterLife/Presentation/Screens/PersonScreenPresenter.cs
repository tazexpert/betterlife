using System.Globalization;
using BetterLife.Core.Relationships;
using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// One person: closeness, compatibility, shared memories and things to do together. The argument is the NPC id.
    /// </summary>
    public sealed class PersonScreenPresenter : ListScreenPresenter
    {
        /// <summary>
        /// Creates the screen.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public PersonScreenPresenter(ScreenContext context)
            : base(context, null)
        {
        }

        /// <inheritdoc />
        protected override void Fill(LifeSession session)
        {
            int npcId;
            int.TryParse(Request?.Argument, NumberStyles.Integer, CultureInfo.InvariantCulture, out npcId);
            PersonState person = PeopleViewModel.BuildPerson(session, npcId, Context.Localizer, Context.Money);
            if (person == null)
            {
                SetTitle(string.Empty);
                return;
            }
            SetTitle(person.Name);
            Content.Add(AvatarImage.Create(person.AvatarPath, AvatarImage.LargeClass));
            Content.Add(UiRows.Text(person.Detail, "list-screen__subtitle"));
            Content.Add(UiRows.Row(Context.Localizer.Get("person.closeness"), person.ValueText, string.Empty, RowTone.Neutral));

            if (person.Interactions.Count > 0 && !session.IsOver)
            {
                Content.Add(UiRows.Section(Context.Localizer.Get("person.actions")));
                foreach (InteractionOption option in person.Interactions)
                {
                    string id = option.Id;
                    Content.Add(UiRows.Action(option.Name, () => Interact(npcId, id), option.IsAvailable, option.Detail));
                }
            }
            AddSection("person.compatibility", person.Compatibility);
            AddSection("person.memories", person.Memories);
        }

        private void Interact(int npcId, string interactionId)
        {
            LifeSession session = Context.Session;
            if (session == null)
            {
                return;
            }
            InteractionResult result = Context.Decisions.Interact(session, npcId, interactionId);
            ShowResult(PeopleViewModel.BuildResult(session, interactionId, result, Context.Localizer, Context.Money));
        }
    }
}
