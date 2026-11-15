using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Every achievement: unlocked first (with who unlocked it), secret ones hidden until unlocked.
    /// </summary>
    public sealed class AchievementsScreenPresenter : ListScreenPresenter
    {
        /// <summary>
        /// Creates the screen.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public AchievementsScreenPresenter(ScreenContext context)
            : base(context, "achievements.title")
        {
        }

        /// <inheritdoc />
        protected override void Fill(LifeSession session)
        {
            var rows = AchievementsViewModel.Build(Context.Achievements, Context.Localizer, out string progress);
            Content.Add(UiRows.Text(progress, "list-screen__subtitle"));
            foreach (AchievementRow row in rows)
            {
                string detail = row.Detail.Length > 0 ? row.Description + " · " + row.Detail : row.Description;
                var element = UiRows.Row(row.Name, string.Empty, detail, row.IsUnlocked ? RowTone.Good : RowTone.Neutral);
                element.EnableInClassList("value-row--locked", !row.IsUnlocked);
                Content.Add(element);
            }
        }
    }
}
