using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;
using UnityEngine.UIElements;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Life tab: the weekly hour plan (with −5/−1/+1/+5 buttons and what the hours do) and quitting habits.
    /// </summary>
    public sealed class LifePlanScreenPresenter : ListScreenPresenter
    {
        /// <summary>Hour steps of the buttons.</summary>
        public static readonly int[] Steps = { -5, -1, 1, 5 };

        /// <summary>
        /// Creates the screen.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public LifePlanScreenPresenter(ScreenContext context)
            : base(context, "life.title")
        {
        }

        /// <inheritdoc />
        protected override void Fill(LifeSession session)
        {
            LifePlanState plan = LifePlanViewModel.Build(session, Context.Localizer, Context.Money);
            Content.Add(UiRows.Text(plan.Summary, "list-screen__subtitle"));
            foreach (string issue in plan.Issues)
            {
                Content.Add(UiRows.Text(issue, "list-screen__warning"));
            }
            foreach (ActivityRow activity in plan.Activities)
            {
                Content.Add(Activity(activity, !session.IsOver));
            }

            if (plan.Habits.Count > 0)
            {
                Content.Add(UiRows.Section(Context.Localizer.Get("dashboard.habits")));
                foreach (OfferGroup habit in plan.Habits)
                {
                    Content.Add(UiRows.Row(habit.Title, string.Empty, habit.Detail, RowTone.Bad));
                    foreach (AmountChoice method in habit.Choices)
                    {
                        string viceId = habit.Id;
                        string methodId = method.Id;
                        Content.Add(UiRows.Action(method.Label, () => Quit(viceId, methodId), method.IsAvailable && !session.IsOver, method.Detail));
                    }
                }
            }
        }

        private VisualElement Activity(ActivityRow activity, bool canEdit)
        {
            var card = new VisualElement();
            card.AddToClassList("activity");
            card.Add(UiRows.Row(activity.Name, activity.HoursText, activity.Note.Length > 0 ? activity.Note : activity.Description, RowTone.Neutral));
            foreach (string effect in activity.Effects)
            {
                card.Add(UiRows.Text(effect, "activity__effect"));
            }
            if (activity.IsEditable && canEdit)
            {
                var steps = new VisualElement();
                steps.AddToClassList("activity__steps");
                foreach (int step in Steps)
                {
                    int target = activity.Stepped(step);
                    string id = activity.Id;
                    var button = new Button(() => SetHours(id, target)) { text = step > 0 ? "+" + step : step.ToString() };
                    button.AddToClassList("activity__step");
                    button.SetEnabled(target != activity.Hours);
                    steps.Add(button);
                }
                card.Add(steps);
            }
            return card;
        }

        private void SetHours(string activityId, int hours)
        {
            LifeSession session = Context.Session;
            if (session != null && Context.Decisions.SetHours(session, activityId, hours))
            {
                Context.NotifyChanged();
            }
        }

        private void Quit(string viceId, string methodId)
        {
            LifeSession session = Context.Session;
            if (session == null)
            {
                return;
            }
            var quote = Context.Decisions.StartQuit(session, viceId, methodId);
            ShowResult(LifePlanViewModel.BuildQuitResult(session, methodId, quote, Context.Localizer));
        }
    }
}
