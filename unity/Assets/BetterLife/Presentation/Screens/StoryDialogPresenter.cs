using System;
using System.Globalization;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Navigation;
using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;
using BetterLife.Presentation.Shell;
using UnityEngine.UIElements;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Modal story popup of a life event without choices (a milestone such as "Into your dream faculty"): the story text and
    /// the effects it applied. Reuses the event dialog layout in its result mode. The screen argument is the event instance id.
    /// </summary>
    public sealed class StoryDialogPresenter : IScreenPresenter
    {
        private readonly ScreenContext _context;
        private readonly Label _title;
        private readonly Label _resultText;
        private readonly Label _rollText;
        private readonly VisualElement _resultLines;
        private int _instanceId;

        /// <summary>
        /// Creates the popup.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public StoryDialogPresenter(ScreenContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Root = UiAssets.Layout(UiAssets.EventDialog).Instantiate();
            UiText.Localize(Root, context.Localizer);
            _title = Root.Q<Label>("title");
            _resultText = Root.Q<Label>("result-text");
            _rollText = Root.Q<Label>("roll-text");
            _resultLines = Root.Q<VisualElement>("result-lines");
            Root.Q<VisualElement>("question").style.display = DisplayStyle.None;
            Root.Q<VisualElement>("result").style.display = DisplayStyle.Flex;
            _rollText.style.display = DisplayStyle.None;
            Root.Q<Button>("continue").clicked += () => _context.Router.CloseModal();
        }

        /// <inheritdoc />
        public VisualElement Root { get; }

        /// <inheritdoc />
        public void Show(ScreenRequest request)
        {
            int.TryParse(request.Argument, NumberStyles.Integer, CultureInfo.InvariantCulture, out _instanceId);
        }

        /// <inheritdoc />
        public void Refresh()
        {
            LifeSession session = _context.Session;
            StoryEntry story = session?.Stories.Find(_instanceId);
            _resultLines.Clear();
            if (story == null)
            {
                _title.text = _resultText.text = string.Empty;
                return;
            }
            EventResultState state = EventDialogViewModel.BuildStory(session, story, _context.Localizer, _context.Money);
            _title.text = state.Title;
            _resultText.text = state.ResultText;
            foreach (string line in state.Lines)
            {
                var label = new Label(line);
                label.AddToClassList("event-dialog__result-line");
                _resultLines.Add(label);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
