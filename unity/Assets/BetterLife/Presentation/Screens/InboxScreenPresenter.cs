using System;
using System.Collections.Generic;
using System.Globalization;
using BetterLife.Core.Domain;
using BetterLife.Core.Navigation;
using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;
using BetterLife.Presentation.Shell;
using UnityEngine.UIElements;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// The inbox: events waiting for an answer (tap to open the dialog) and recent news.
    /// </summary>
    public sealed class InboxScreenPresenter : IScreenPresenter
    {
        /// <summary>Row class.</summary>
        public const string ItemClass = "inbox-item";
        /// <summary>Row class of a decision.</summary>
        public const string DecisionClass = "inbox-item--decision";
        /// <summary>Row class when the answer is due this week.</summary>
        public const string UrgentClass = "inbox-item--urgent";
        /// <summary>Row class of major or critical items.</summary>
        public const string MajorClass = "inbox-item--major";

        private readonly ScreenContext _context;
        private readonly VisualElement _decisions;
        private readonly VisualElement _notices;
        private readonly Label _decisionsTitle;
        private readonly Label _noticesTitle;
        private readonly Label _empty;

        /// <summary>
        /// Creates the inbox.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public InboxScreenPresenter(ScreenContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Root = UiAssets.Layout(UiAssets.Inbox).Instantiate();
            Root.style.flexGrow = 1;
            UiText.Localize(Root, context.Localizer);
            _decisions = Root.Q<VisualElement>("decisions");
            _notices = Root.Q<VisualElement>("notices");
            _decisionsTitle = Root.Q<Label>("decisions-title");
            _noticesTitle = Root.Q<Label>("notices-title");
            _empty = Root.Q<Label>("empty");
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
            _decisions.Clear();
            _notices.Clear();
            LifeSession session = _context.Session;
            if (session == null)
            {
                return;
            }
            InboxState state = InboxViewModel.Build(session, _context.Notices, _context.Localizer);
            Fill(_decisions, _decisionsTitle, state.Decisions);
            Fill(_notices, _noticesTitle, state.Notices);
            _empty.text = state.EmptyText;
            _empty.style.display = state.IsEmpty ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        private void Fill(VisualElement list, Label title, IReadOnlyList<InboxItemState> items)
        {
            title.style.display = items.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            foreach (InboxItemState item in items)
            {
                list.Add(Row(item));
            }
        }

        private VisualElement Row(InboxItemState item)
        {
            var row = new VisualElement();
            row.AddToClassList(ItemClass);
            row.EnableInClassList(DecisionClass, item.Kind == InboxItemKind.Decision);
            row.EnableInClassList(UrgentClass, item.IsUrgent);
            row.EnableInClassList(MajorClass, item.Severity >= Severity.Major);
            var title = new Label(item.Title);
            title.AddToClassList("inbox-item__title");
            var subtitle = new Label(item.Subtitle);
            subtitle.AddToClassList("inbox-item__subtitle");
            row.Add(title);
            row.Add(subtitle);
            if (item.Kind == InboxItemKind.Decision && item.InstanceId.HasValue)
            {
                string argument = item.InstanceId.Value.ToString(CultureInfo.InvariantCulture);
                row.AddManipulator(new Clickable(() => _context.Router.OpenModal(new ScreenRequest(ScreenIds.EventDialog, argument))));
            }
            return row;
        }
    }
}
