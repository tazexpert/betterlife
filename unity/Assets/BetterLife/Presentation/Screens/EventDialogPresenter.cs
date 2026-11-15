using System;
using System.Collections.Generic;
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
    /// Modal event dialog: the question and its choices; after a choice, the outcome in the same dialog.
    /// The screen argument is the pending event instance id.
    /// </summary>
    public sealed class EventDialogPresenter : IScreenPresenter
    {
        /// <summary>Text above failure previews.</summary>
        public const string IfFailedKey = "event.if_failed";
        /// <summary>Choice block class.</summary>
        public const string ChoiceClass = "event-choice";
        /// <summary>Class of a locked choice.</summary>
        public const string LockedClass = "event-choice--locked";
        /// <summary>Class of a failed outcome.</summary>
        public const string FailedClass = "event-dialog__result--failed";

        private readonly ScreenContext _context;
        private readonly Label _title;
        private readonly Label _body;
        private readonly Label _deadline;
        private readonly VisualElement _question;
        private readonly VisualElement _choices;
        private readonly VisualElement _result;
        private readonly Label _resultText;
        private readonly Label _rollText;
        private readonly VisualElement _resultLines;
        private int _instanceId;
        private bool _answered;

        /// <summary>
        /// Creates the dialog.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public EventDialogPresenter(ScreenContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Root = UiAssets.Layout(UiAssets.EventDialog).Instantiate();
            UiText.Localize(Root, context.Localizer);
            _title = Root.Q<Label>("title");
            _body = Root.Q<Label>("body");
            _deadline = Root.Q<Label>("deadline");
            _question = Root.Q<VisualElement>("question");
            _choices = Root.Q<VisualElement>("choices");
            _result = Root.Q<VisualElement>("result");
            _resultText = Root.Q<Label>("result-text");
            _rollText = Root.Q<Label>("roll-text");
            _resultLines = Root.Q<VisualElement>("result-lines");
            Root.Q<Button>("later").clicked += Close;
            Root.Q<Button>("continue").clicked += Close;
        }

        /// <inheritdoc />
        public VisualElement Root { get; }

        /// <inheritdoc />
        public void Show(ScreenRequest request)
        {
            int id;
            int.TryParse(request.Argument, NumberStyles.Integer, CultureInfo.InvariantCulture, out id);
            if (id != _instanceId)
            {
                _answered = false;
            }
            _instanceId = id;
        }

        /// <inheritdoc />
        public void Refresh()
        {
            if (_answered)
            {
                return;
            }
            LifeSession session = _context.Session;
            EventDialogState state = session == null ? null : EventDialogViewModel.Build(session, _instanceId, _context.Localizer, _context.Money);
            SetMode(false);
            _choices.Clear();
            if (state == null)
            {
                // Already answered or expired: only the "decide later" button remains, which closes the dialog.
                _title.text = _body.text = _deadline.text = string.Empty;
                return;
            }
            _title.text = state.Title;
            _body.text = state.Body;
            _deadline.text = state.DeadlineText;
            foreach (ChoiceState choice in state.Choices)
            {
                _choices.Add(Choice(choice));
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        private VisualElement Choice(ChoiceState choice)
        {
            var block = new VisualElement();
            block.AddToClassList(ChoiceClass);
            block.EnableInClassList(LockedClass, !choice.IsAvailable);

            string choiceId = choice.Id;
            var button = new Button(() => Answer(choiceId)) { text = choice.Text };
            button.AddToClassList("event-choice__button");
            button.SetEnabled(choice.IsAvailable);
            block.Add(button);

            if (choice.ChanceText != null)
            {
                block.Add(Line(choice.ChanceText, "event-choice__chance"));
            }
            if (choice.LockedText != null)
            {
                block.Add(Line(choice.LockedText, "event-choice__locked"));
            }
            AddLines(block, choice.SuccessPreview, "event-choice__effect");
            if (choice.FailurePreview.Count > 0)
            {
                block.Add(Line(_context.Localizer.Get(IfFailedKey), "event-choice__fail-title"));
                AddLines(block, choice.FailurePreview, "event-choice__effect");
            }
            return block;
        }

        private void Answer(string choiceId)
        {
            LifeSession session = _context.Session;
            if (session == null || _answered)
            {
                return;
            }
            EventResolution resolution = _context.Decisions.Answer(session, _instanceId, choiceId);
            EventResultState result = EventDialogViewModel.BuildResult(session, resolution, _context.Localizer, _context.Money);
            _answered = true;
            _title.text = result.Title;
            _resultText.text = result.ResultText;
            _rollText.text = result.RollText ?? string.Empty;
            _rollText.style.display = result.RollText == null ? DisplayStyle.None : DisplayStyle.Flex;
            _result.EnableInClassList(FailedClass, !result.Succeeded);
            _resultLines.Clear();
            AddLines(_resultLines, result.Lines, "event-dialog__result-line");
            SetMode(true);
            _context.NotifyChanged();
        }

        private void Close()
        {
            _answered = false;
            _instanceId = 0;
            _context.Router.CloseModal();
        }

        private void SetMode(bool showingResult)
        {
            _question.style.display = showingResult ? DisplayStyle.None : DisplayStyle.Flex;
            _result.style.display = showingResult ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static void AddLines(VisualElement parent, IReadOnlyList<string> lines, string ussClass)
        {
            foreach (string line in lines)
            {
                parent.Add(Line(line, ussClass));
            }
        }

        private static Label Line(string text, string ussClass)
        {
            var label = new Label(text);
            label.AddToClassList(ussClass);
            return label;
        }
    }
}
