using System;
using System.Collections.Generic;
using BetterLife.Core.Localization;
using BetterLife.Core.Navigation;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.ViewModels;
using BetterLife.Presentation.Screens;
using UnityEngine.UIElements;

namespace BetterLife.Presentation.Shell
{
    /// <summary>
    /// Drives the app shell: bottom navigation from the router's tabs, the current screen, the modal layer and the
    /// top bar. Screens are created once per id and reused.
    /// </summary>
    public sealed class ShellPresenter : IDisposable
    {
        /// <summary>Class of a bottom navigation tab.</summary>
        public const string TabClass = "nav-tab";
        /// <summary>Class of the selected tab.</summary>
        public const string TabSelectedClass = "nav-tab--selected";
        /// <summary>Class of a tab icon.</summary>
        public const string TabIconClass = "nav-tab__icon";
        /// <summary>Class of a tab label.</summary>
        public const string TabLabelClass = "nav-tab__label";
        /// <summary>Class that shows the modal layer.</summary>
        public const string ModalOpenClass = "shell__modal-layer--open";

        private readonly ScreenRouter _router;
        private readonly ScreenRegistry _screens;
        private readonly ILocalizer _localizer;
        private readonly MoneyFormatter _money;
        private readonly VisualElement _screenContainer;
        private readonly VisualElement _modalLayer;
        private readonly TopBarPresenter _topBar;
        private readonly Dictionary<string, Button> _tabButtons = new Dictionary<string, Button>();
        private readonly Dictionary<string, IScreenPresenter> _screenPresenters = new Dictionary<string, IScreenPresenter>();
        private readonly Dictionary<string, IScreenPresenter> _modalPresenters = new Dictionary<string, IScreenPresenter>();
        private IScreenPresenter _currentScreen;
        private IScreenPresenter _currentModal;
        private LifeSession _session;

        /// <summary>
        /// Builds the shell inside the root of the shell layout.
        /// </summary>
        /// <param name="root">Root of <c>Shell.uxml</c>.</param>
        /// <param name="router">Navigation state.</param>
        /// <param name="screens">Screen factories.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="money">Money formatter.</param>
        public ShellPresenter(VisualElement root, ScreenRouter router, ScreenRegistry screens, ILocalizer localizer, MoneyFormatter money)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }
            _router = router ?? throw new ArgumentNullException(nameof(router));
            _screens = screens ?? throw new ArgumentNullException(nameof(screens));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _money = money ?? throw new ArgumentNullException(nameof(money));
            _screenContainer = root.Q<VisualElement>("screen-container");
            _modalLayer = root.Q<VisualElement>("modal-layer");

            _topBar = new TopBarPresenter(root.Q<VisualElement>("top-bar-slot"), localizer);
            _topBar.SkipRequested += Advance;
            _topBar.BackRequested += () => _router.Back();
            BuildTabs(root.Q<VisualElement>("bottom-nav"));

            _router.Changed += Render;
            Render();
        }

        /// <summary>Raised after time advanced from the top bar.</summary>
        public event Action<AdvanceResult> TimeAdvanced;

        /// <summary>
        /// Shows a life (or none) and refreshes everything.
        /// </summary>
        /// <param name="session">The running life.</param>
        public void Bind(LifeSession session)
        {
            _session = session;
            _router.Reset();
            Refresh();
        }

        /// <summary>
        /// Re-reads the game state into the top bar and the visible screens.
        /// </summary>
        public void Refresh()
        {
            TopBarState state = _session == null ? null : TopBarViewModel.Build(_session, _localizer, _money);
            _topBar.Bind(state, _router.CanGoBack);
            _currentScreen?.Refresh();
            _currentModal?.Refresh();
        }

        /// <summary>
        /// Handles the device back button.
        /// </summary>
        /// <returns>What happened; <see cref="BackResult.Exit"/> means the app should close.</returns>
        public BackResult Back() => _router.Back();

        /// <summary>
        /// Stops listening and disposes all screens.
        /// </summary>
        public void Dispose()
        {
            _router.Changed -= Render;
            foreach (IScreenPresenter presenter in _screenPresenters.Values)
            {
                presenter.Dispose();
            }
            foreach (IScreenPresenter presenter in _modalPresenters.Values)
            {
                presenter.Dispose();
            }
            _screenPresenters.Clear();
            _modalPresenters.Clear();
        }

        private void BuildTabs(VisualElement nav)
        {
            foreach (TabDefinition tab in _router.Tabs)
            {
                string tabId = tab.Id;
                var button = new Button(() => _router.SelectTab(tabId)) { name = "tab-" + tabId };
                button.AddToClassList(TabClass);
                var icon = new VisualElement();
                icon.AddToClassList(TabIconClass);
                icon.AddToClassList(tab.IconClass);
                var label = new Label(_localizer.Get(tab.LabelKey));
                label.AddToClassList(TabLabelClass);
                button.Add(icon);
                button.Add(label);
                nav.Add(button);
                _tabButtons.Add(tabId, button);
            }
        }

        private void Advance(TimeSkip skip)
        {
            if (_session == null || _session.IsOver)
            {
                return;
            }
            AdvanceResult result = _session.Advance(skip);
            Refresh();
            TimeAdvanced?.Invoke(result);
        }

        private void Render()
        {
            foreach (KeyValuePair<string, Button> tab in _tabButtons)
            {
                tab.Value.EnableInClassList(TabSelectedClass, tab.Key == _router.CurrentTab.Id);
            }

            _currentScreen = Place(_screenContainer, _screenPresenters, _router.CurrentScreen, _currentScreen);
            _currentModal = Place(_modalLayer, _modalPresenters, _router.Modal, _currentModal);
            _modalLayer.EnableInClassList(ModalOpenClass, _currentModal != null);
            _topBar.Bind(_session == null ? null : TopBarViewModel.Build(_session, _localizer, _money), _router.CanGoBack);
        }

        private IScreenPresenter Place(VisualElement container, Dictionary<string, IScreenPresenter> cache, ScreenRequest request, IScreenPresenter shown)
        {
            if (request == null)
            {
                container.Clear();
                return null;
            }
            if (!cache.TryGetValue(request.ScreenId, out IScreenPresenter presenter))
            {
                presenter = _screens.Create(request.ScreenId);
                cache.Add(request.ScreenId, presenter);
            }
            if (presenter != shown)
            {
                container.Clear();
                container.Add(presenter.Root);
            }
            presenter.Show(request);
            presenter.Refresh();
            return presenter;
        }
    }
}
