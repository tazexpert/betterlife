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
        }
    }
}
