using System;
using BetterLife.Core.Navigation;
using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;
using BetterLife.Presentation.Shell;
using UnityEngine.UIElements;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Home tab: core stat cards (value, trend, sparkline; tap for the breakdown), habit cards and the inbox shortcut.
    /// </summary>
    public sealed class DashboardScreenPresenter : IScreenPresenter
    {
        /// <summary>Card class.</summary>
        public const string CardClass = "stat-card";
        /// <summary>Card class when the trend is good.</summary>
        public const string ImprovingClass = "stat-card--improving";
        /// <summary>Card class when the trend is bad.</summary>
        public const string WorseningClass = "stat-card--worsening";
        /// <summary>Habit card class.</summary>
        public const string HabitClass = "habit-card";
        /// <summary>Habit card class while quitting.</summary>
        public const string QuittingClass = "habit-card--quitting";

        private readonly ScreenContext _context;
        private readonly VisualElement _family;
        private readonly VisualElement _cards;
        private readonly VisualElement _habits;
        private readonly Label _habitsTitle;
        private readonly Button _inbox;

        /// <summary>
        /// Creates the dashboard.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public DashboardScreenPresenter(ScreenContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Root = UiAssets.Layout(UiAssets.Dashboard).Instantiate();
            Root.style.flexGrow = 1;
            UiText.Localize(Root, context.Localizer);
            _family = Root.Q<VisualElement>("family");
            _cards = Root.Q<VisualElement>("cards");
            _habits = Root.Q<VisualElement>("habits");
            _habitsTitle = Root.Q<Label>("habits-title");
            _inbox = Root.Q<Button>("inbox-shortcut");
            _inbox.clicked += () => _context.Router.Push(new ScreenRequest(ScreenIds.Inbox));
        }

        /// <inheritdoc />
        public VisualElement Root { get; }

        /// <inheritdoc />
        public void Show(ScreenRequest request)
        {
        }

        /// <inheritdoc />
        public void Refresh()
        {
            LifeSession session = _context.Session;
            _family.Clear();
            _cards.Clear();
            _habits.Clear();
            if (session == null)
            {
                return;
            }
            DashboardState state = DashboardViewModel.Build(session, _context.Localizer);
            foreach (ValueRow row in state.Family)
            {
                _family.Add(UiRows.Row(row, () => _context.Router.SelectTab(ScreenIds.People)));
            }
            foreach (StatCardState card in state.Cards)
            {
                _cards.Add(Card(card));
            }
            foreach (HabitCardState habit in state.Habits)
            {
                _habits.Add(Habit(habit));
            }
            bool hasHabits = state.Habits.Count > 0;
            _habitsTitle.style.display = hasHabits ? DisplayStyle.Flex : DisplayStyle.None;
            _inbox.text = state.InboxText;
            _inbox.style.display = state.PendingDecisions > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        private VisualElement Card(StatCardState card)
        {
            var element = new VisualElement();
            element.AddToClassList(CardClass);
            element.EnableInClassList(ImprovingClass, card.Direction != TrendDirection.Flat && card.IsImproving);
            element.EnableInClassList(WorseningClass, card.Direction != TrendDirection.Flat && !card.IsImproving);

            var header = new VisualElement();
            header.AddToClassList("stat-card__header");
            header.Add(Text(card.Name, "stat-card__name"));
            header.Add(Text(card.ValueText, "stat-card__value"));
            element.Add(header);
            element.Add(Text(card.DeltaText, "stat-card__delta"));

            var sparkline = new SparklineElement();
            sparkline.AddToClassList("stat-card__sparkline");
            sparkline.SetPoints(card.Sparkline);
            element.Add(sparkline);

            string statId = card.StatId.Value;
            element.AddManipulator(new Clickable(() => _context.Router.OpenModal(new ScreenRequest(ScreenIds.Breakdown, statId))));
            return element;
        }

        private static VisualElement Habit(HabitCardState habit)
        {
            var element = new VisualElement();
            element.AddToClassList(HabitClass);
            element.EnableInClassList(QuittingClass, habit.IsQuitting);
            element.Add(Text(habit.Name, "habit-card__name"));
            element.Add(Text(habit.StatusText, "habit-card__status"));
            return element;
        }

        private static Label Text(string text, string ussClass)
        {
            var label = new Label(text);
            label.AddToClassList(ussClass);
            return label;
        }
    }
}
