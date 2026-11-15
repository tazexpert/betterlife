using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Birth chart, this year's fortune per area (tap to see its parts) and karma.
    /// </summary>
    public sealed class AstrologyScreenPresenter : ListScreenPresenter
    {
        private string _expandedArea;

        /// <summary>
        /// Creates the screen.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public AstrologyScreenPresenter(ScreenContext context)
            : base(context, "astrology.title")
        {
        }

        /// <inheritdoc />
        protected override void Fill(LifeSession session)
        {
            AstrologyState state = AstrologyViewModel.Build(session, Context.Localizer);
            AddSection("astrology.chart", state.Profile);
            Content.Add(UiRows.Section(state.YearTitle));
            foreach (FortuneRow fortune in state.Fortunes)
            {
                string area = fortune.Area;
                bool expanded = area == _expandedArea;
                Content.Add(UiRows.Row(fortune.Area, fortune.ValueText, string.Empty, RowTone.Neutral, () =>
                {
                    _expandedArea = expanded ? null : area;
                    Refresh();
                }));
                if (expanded)
                {
                    foreach (ValueRow part in fortune.Parts)
                    {
                        var row = UiRows.Row(part);
                        row.AddToClassList("value-row--nested");
                        Content.Add(row);
                    }
                }
            }
            AddSection("astrology.karma", state.Karma);
        }
    }
}
