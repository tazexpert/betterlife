using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Navigation
{
    /// <summary>
    /// Navigation state of the shell: one screen history per tab, the selected tab, and modals on top.
    /// Pure logic; the UI listens to <see cref="Changed"/> and shows <see cref="Current"/>.
    /// </summary>
    public sealed class ScreenRouter
    {
        private readonly List<TabDefinition> _tabs;
        private readonly Dictionary<string, List<ScreenRequest>> _stacks = new Dictionary<string, List<ScreenRequest>>(StringComparer.Ordinal);
        private readonly List<ScreenRequest> _modals = new List<ScreenRequest>();

        /// <summary>
        /// Creates the router with every tab at its root screen and the first tab selected.
        /// </summary>
        /// <param name="tabs">Tabs, left to right; the first is home.</param>
        public ScreenRouter(IEnumerable<TabDefinition> tabs)
        {
            _tabs = (tabs ?? throw new ArgumentNullException(nameof(tabs))).ToList();
            if (_tabs.Count == 0 || _tabs.Select(t => t.Id).Distinct(StringComparer.Ordinal).Count() != _tabs.Count)
            {
                throw new ArgumentException("Tabs must be non-empty with unique ids.", nameof(tabs));
            }
            Reset();
        }

        /// <summary>Raised after every navigation change.</summary>
        public event Action Changed;

        /// <summary>Tabs, left to right.</summary>
        public IReadOnlyList<TabDefinition> Tabs => _tabs;

        /// <summary>Selected tab.</summary>
        public TabDefinition CurrentTab { get; private set; }

        /// <summary>Screen showing in the selected tab (under any modal).</summary>
        public ScreenRequest CurrentScreen => _stacks[CurrentTab.Id].Last();

        /// <summary>Top modal, or null.</summary>
        public ScreenRequest Modal => _modals.LastOrDefault();

        /// <summary>What the player sees on top: the top modal, or the current screen.</summary>
        public ScreenRequest Current => Modal ?? CurrentScreen;

        /// <summary>Screens in the selected tab's history (1 at its root).</summary>
        public int Depth => _stacks[CurrentTab.Id].Count;

        /// <summary>True when Back would do something other than exit.</summary>
        public bool CanGoBack => _modals.Count > 0 || Depth > 1 || CurrentTab != _tabs[0];

        /// <summary>
        /// Selects a tab. Selecting the current tab again goes back to its root screen.
        /// Modals are closed.
        /// </summary>
        /// <param name="tabId">Tab id.</param>
        public void SelectTab(string tabId)
        {
            TabDefinition tab = _tabs.FirstOrDefault(t => t.Id == tabId) ?? throw new ArgumentException($"Unknown tab '{tabId}'.", nameof(tabId));
            _modals.Clear();
            if (tab == CurrentTab)
            {
                List<ScreenRequest> stack = _stacks[tab.Id];
                stack.RemoveRange(1, stack.Count - 1);
            }
            CurrentTab = tab;
            Changed?.Invoke();
        }

        /// <summary>
        /// Opens a screen inside the selected tab (closing modals).
        /// </summary>
        /// <param name="request">Screen.</param>
        public void Push(ScreenRequest request)
        {
            _modals.Clear();
            _stacks[CurrentTab.Id].Add(request ?? throw new ArgumentNullException(nameof(request)));
            Changed?.Invoke();
        }

        /// <summary>
        /// Opens a modal over everything (dialogs, event choices).
        /// </summary>
        /// <param name="request">Modal screen.</param>
        public void OpenModal(ScreenRequest request)
        {
            _modals.Add(request ?? throw new ArgumentNullException(nameof(request)));
            Changed?.Invoke();
        }

        /// <summary>
        /// Closes the top modal.
        /// </summary>
        /// <returns>True when one was open.</returns>
        public bool CloseModal()
        {
            if (_modals.Count == 0)
            {
                return false;
            }
            _modals.RemoveAt(_modals.Count - 1);
            Changed?.Invoke();
            return true;
        }

        /// <summary>
        /// Goes back: closes a modal, else pops a screen, else returns to the home tab, else asks to exit.
        /// </summary>
        /// <returns>What happened.</returns>
        public BackResult Back()
        {
            if (CloseModal())
            {
                return BackResult.ClosedModal;
            }
            List<ScreenRequest> stack = _stacks[CurrentTab.Id];
            if (stack.Count > 1)
            {
                stack.RemoveAt(stack.Count - 1);
                Changed?.Invoke();
                return BackResult.Popped;
            }
            if (CurrentTab != _tabs[0])
            {
                CurrentTab = _tabs[0];
                Changed?.Invoke();
                return BackResult.ReturnedHome;
            }
            return BackResult.Exit;
        }

        /// <summary>
        /// Puts every tab back at its root, closes modals and selects home (for example after loading another life).
        /// </summary>
        public void Reset()
        {
            _stacks.Clear();
            foreach (TabDefinition tab in _tabs)
            {
                _stacks[tab.Id] = new List<ScreenRequest> { new ScreenRequest(tab.RootScreenId) };
            }
            _modals.Clear();
            CurrentTab = _tabs[0];
            Changed?.Invoke();
        }
    }
}
