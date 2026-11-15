using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// The life timeline, newest first.
    /// </summary>
    public sealed class TimelineScreenPresenter : ListScreenPresenter
    {
        /// <summary>Entry class prefix (<c>timeline-entry--career</c>...).</summary>
        public const string KindClassPrefix = "timeline-entry--";

        /// <summary>
        /// Creates the screen.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public TimelineScreenPresenter(ScreenContext context)
            : base(context, "timeline.title")
        {
        }

        /// <inheritdoc />
        protected override void Fill(LifeSession session)
        {
            foreach (TimelineEntry entry in TimelineViewModel.Build(session, Context.Localizer, Context.Money))
            {
                string detail = entry.Detail.Length > 0 ? entry.DateText + " · " + entry.Detail : entry.DateText;
                var row = UiRows.Row(entry.Title, entry.AgeText, detail, RowTone.Neutral);
                row.AddToClassList("timeline-entry");
                row.AddToClassList(KindClassPrefix + entry.Kind.ToString().ToLowerInvariant());
                Content.Add(row);
            }
        }
    }
}
