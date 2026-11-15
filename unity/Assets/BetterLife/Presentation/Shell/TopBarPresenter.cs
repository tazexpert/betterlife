using System;
using System.Globalization;
using BetterLife.Core.Localization;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.ViewModels;
using UnityEngine.UIElements;

namespace BetterLife.Presentation.Shell
{
    /// <summary>
    /// Shows the character summary (name, age, date, money, inbox) and the time skip buttons.
    /// </summary>
    public sealed class TopBarPresenter
    {
        /// <summary>Inbox count text key (placeholder <c>{count}</c>).</summary>
        public const string InboxKey = "topbar.inbox";
        /// <summary>Class added when the life has ended (hides the time buttons).</summary>
        public const string LifeOverClass = "top-bar--life-over";

        private readonly ILocalizer _localizer;
        private readonly VisualElement _bar;
        private readonly Button _back;
        private readonly Label _name;
        private readonly Label _details;
        private readonly Label _cash;
        private readonly Label _inbox;
        private readonly Button[] _skips;

        /// <summary>
        /// Builds the top bar inside a container.
        /// </summary>
        /// <param name="container">Slot element of the shell.</param>
        /// <param name="localizer">Localizer.</param>
        public TopBarPresenter(VisualElement container, ILocalizer localizer)
        {
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            VisualElement tree = UiAssets.Layout(UiAssets.TopBar).Instantiate();
            container.Add(tree);
            UiText.Localize(tree, localizer);

            _bar = tree.Q<VisualElement>("top-bar");
            _back = tree.Q<Button>("back-button");
            _name = tree.Q<Label>("name");
            _details = tree.Q<Label>("details");
            _cash = tree.Q<Label>("cash");
            _inbox = tree.Q<Label>("inbox");
            _skips = new[] { tree.Q<Button>("skip-week"), tree.Q<Button>("skip-month"), tree.Q<Button>("skip-year") };

            _back.clicked += () => BackRequested?.Invoke();
            _skips[0].clicked += () => SkipRequested?.Invoke(TimeSkip.Week);
            _skips[1].clicked += () => SkipRequested?.Invoke(TimeSkip.Month);
            _skips[2].clicked += () => SkipRequested?.Invoke(TimeSkip.Year);
        }

        /// <summary>Raised when a time skip button is pressed.</summary>
        public event Action<TimeSkip> SkipRequested;

        /// <summary>Raised when the back button is pressed.</summary>
        public event Action BackRequested;

        /// <summary>
        /// Shows a state.
        /// </summary>
        /// <param name="state">Top bar state (null shows an empty bar).</param>
        /// <param name="canGoBack">Whether the back button is visible.</param>
        public void Bind(TopBarState state, bool canGoBack)
        {
            _back.style.display = canGoBack ? DisplayStyle.Flex : DisplayStyle.None;
            if (state == null)
            {
                _name.text = _details.text = _cash.text = _inbox.text = string.Empty;
                SetSkipsEnabled(false);
                return;
            }
            _name.text = state.Name;
            _details.text = state.AgeText + " · " + state.DateText;
            _cash.text = state.CashText;
            _inbox.text = _localizer.Format(InboxKey, ("count", state.InboxCount.ToString(CultureInfo.InvariantCulture)));
            _inbox.style.display = state.InboxCount > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _bar.EnableInClassList(LifeOverClass, !state.CanAdvance);
            SetSkipsEnabled(state.CanAdvance);
        }

        private void SetSkipsEnabled(bool enabled)
        {
            foreach (Button skip in _skips)
            {
                skip.SetEnabled(enabled);
            }
        }
    }
}
