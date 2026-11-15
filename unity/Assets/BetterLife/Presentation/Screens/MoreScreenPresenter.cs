using BetterLife.Core.Navigation;
using BetterLife.Core.Sessions;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// More tab: a menu of the screens that do not have their own tab.
    /// </summary>
    public sealed class MoreScreenPresenter : ListScreenPresenter
    {
        /// <summary>
        /// Creates the screen.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public MoreScreenPresenter(ScreenContext context)
            : base(context, "more.title")
        {
        }

        /// <inheritdoc />
        protected override void Fill(LifeSession session)
        {
            Content.Add(UiRows.Action(Context.Localizer.Get("more.astrology"), () => Context.Router.Push(new ScreenRequest(ScreenIds.Astrology))));
            Content.Add(UiRows.Action(Context.Localizer.Get("more.timeline"), () => Context.Router.Push(new ScreenRequest(ScreenIds.Timeline))));
            Content.Add(UiRows.Action(Context.Localizer.Get("more.family_tree"), () => Context.Router.Push(new ScreenRequest(ScreenIds.FamilyTree))));
            Content.Add(UiRows.Action(Context.Localizer.Get("more.achievements"), () => Context.Router.Push(new ScreenRequest(ScreenIds.Achievements))));
            if (session.IsOver)
            {
                Content.Add(UiRows.Action(Context.Localizer.Get("more.summary"), () => Context.Router.Push(new ScreenRequest(ScreenIds.Summary))));
            }
        }
    }
}
