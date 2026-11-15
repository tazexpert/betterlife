using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// The family tree: every generation of the line, founder first.
    /// </summary>
    public sealed class FamilyTreeScreenPresenter : ListScreenPresenter
    {
        /// <summary>
        /// Creates the screen.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public FamilyTreeScreenPresenter(ScreenContext context)
            : base(context, "tree.title")
        {
        }

        /// <inheritdoc />
        protected override void Fill(LifeSession session)
        {
            foreach (ValueRow row in FamilyTreeViewModel.Build(session, Context.Localizer, Context.Money))
            {
                Content.Add(UiRows.Row(row));
            }
        }
    }
}
