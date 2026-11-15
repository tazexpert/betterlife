using System;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Navigation;
using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;
using BetterLife.Presentation.Shell;
using UnityEngine.UIElements;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Modal stat breakdown: where a value comes from. The screen argument is the stat id.
    /// </summary>
    public sealed class BreakdownPresenter : IScreenPresenter
    {
        /// <summary>Class of a positive contribution.</summary>
        public const string PositiveClass = "breakdown-line--positive";
        /// <summary>Class of a negative contribution.</summary>
        public const string NegativeClass = "breakdown-line--negative";

        private readonly ScreenContext _context;
        private readonly Label _title;
        private readonly Label _value;
        private readonly Label _heading;
        private readonly VisualElement _lines;
        private readonly Label _empty;
        private string _statId;

        /// <summary>
        /// Creates the breakdown.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public BreakdownPresenter(ScreenContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Root = UiAssets.Layout(UiAssets.Breakdown).Instantiate();
            UiText.Localize(Root, context.Localizer);
            _title = Root.Q<Label>("title");
            _value = Root.Q<Label>("value");
            _heading = Root.Q<Label>("heading");
            _lines = Root.Q<VisualElement>("lines");
            _empty = Root.Q<Label>("empty");
            Root.Q<Button>("close").clicked += () => _context.Router.CloseModal();
        }

        /// <inheritdoc />
        public VisualElement Root { get; }

        /// <inheritdoc />
        public void Show(ScreenRequest request)
        {
            _statId = request.Argument;
        }

        /// <inheritdoc />
        public void Refresh()
        {
            _lines.Clear();
            LifeSession session = _context.Session;
            if (session == null || string.IsNullOrEmpty(_statId) || !session.Character.Catalogs.Stats.Contains(new StatId(_statId)))
            {
                _title.text = _value.text = _heading.text = string.Empty;
                return;
            }
            BreakdownState state = BreakdownViewModel.Build(session, new StatId(_statId), _context.Localizer);
            _title.text = state.Title;
            _value.text = state.ValueText;
            _heading.text = state.Heading;
            _empty.text = state.EmptyText;
            _empty.style.display = state.Lines.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            foreach (BreakdownLineState line in state.Lines)
            {
                _lines.Add(Line(line));
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        private static VisualElement Line(BreakdownLineState line)
        {
            var row = new VisualElement();
            row.AddToClassList("breakdown-line");
            row.EnableInClassList(PositiveClass, line.Contribution > 0f);
            row.EnableInClassList(NegativeClass, line.Contribution < 0f);
            var text = new VisualElement();
            text.AddToClassList("breakdown-line__text");
            text.Add(Text(line.Label, "breakdown-line__label"));
            if (line.Detail.Length > 0)
            {
                text.Add(Text(line.Detail, "breakdown-line__detail"));
            }
            row.Add(text);
            row.Add(Text(line.ContributionText, "breakdown-line__value"));
            return row;
        }

        private static Label Text(string text, string ussClass)
        {
            var label = new Label(text);
            label.AddToClassList(ussClass);
            return label;
        }
    }
}
