using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Money tab: overview, economy, holdings, debts, last month, lifestyle choice and scam incidents with reporting.
    /// </summary>
    public sealed class FinanceScreenPresenter : ListScreenPresenter
    {
        /// <summary>
        /// Creates the screen.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public FinanceScreenPresenter(ScreenContext context)
            : base(context, "finance.title")
        {
        }

        /// <inheritdoc />
        protected override void Fill(LifeSession session)
        {
            FinanceState state = FinanceViewModel.Build(session, Context.Localizer, Context.Money);
            foreach (ValueRow row in state.Summary)
            {
                Content.Add(UiRows.Row(row));
            }
            AddSection("finance.section.economy", state.Economy);
            AddSection("finance.section.holdings", state.Holdings, "finance.none");
            AddSection("finance.section.debts", state.Debts, "finance.none");

            Content.Add(UiRows.Section(Context.Localizer.Get("finance.section.report")));
            if (state.ReportNote.Length > 0)
            {
                Content.Add(UiRows.Text(state.ReportNote, "list-screen__empty"));
            }
            foreach (ValueRow row in state.Report)
            {
                Content.Add(UiRows.Row(row));
            }

            Content.Add(UiRows.Section(Context.Localizer.Get("finance.section.lifestyle")));
            if (!state.CanChooseLifestyle)
            {
                Content.Add(UiRows.Text(Context.Localizer.Get("finance.lifestyle.locked"), "list-screen__empty"));
            }
            foreach (LifestyleOption option in state.Lifestyles)
            {
                string id = option.Id;
                var element = UiRows.Row(option.Name, option.CostText, string.Empty, option.IsSelected ? RowTone.Good : RowTone.Neutral,
                    state.CanChooseLifestyle && !option.IsSelected && !session.IsOver ? () => ChooseLifestyle(id) : (System.Action)null);
                element.EnableInClassList("value-row--selected", option.IsSelected);
                Content.Add(element);
            }

            if (state.Scams.Count > 0)
            {
                Content.Add(UiRows.Section(Context.Localizer.Get("finance.section.scams")));
                foreach (ScamIncidentRow incident in state.Scams)
                {
                    Content.Add(UiRows.Row(incident.Title, string.Empty, incident.Detail, RowTone.Bad));
                    if (incident.CanReport && !session.IsOver)
                    {
                        int id = incident.IncidentId;
                        Content.Add(UiRows.Action(incident.ReportText, () => Report(id)));
                    }
                }
            }
        }

        private void ChooseLifestyle(string tierId)
        {
            LifeSession session = Context.Session;
            if (session == null)
            {
                return;
            }
            Context.Decisions.ChooseLifestyle(session, tierId);
            Context.NotifyChanged();
        }

        private void Report(int incidentId)
        {
            LifeSession session = Context.Session;
            if (session == null)
            {
                return;
            }
            var result = Context.Decisions.ReportScam(session, incidentId);
            ShowResult(FinanceViewModel.BuildReportResult(session, result, Context.Localizer, Context.Money));
        }
    }
}
