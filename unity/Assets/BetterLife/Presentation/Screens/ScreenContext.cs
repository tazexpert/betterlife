using System;
using BetterLife.Core.Localization;
using BetterLife.Core.Navigation;
using BetterLife.Core.Notices;
using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// What every screen needs: the running life, texts, navigation, news and decisions. Screens raise
    /// <see cref="Changed"/> after they change the game state so the shell refreshes the top bar and visible screens.
    /// </summary>
    public sealed class ScreenContext
    {
        private readonly Func<LifeSession> _session;

        /// <summary>
        /// Creates the context.
        /// </summary>
        /// <param name="session">Returns the running life (may change when a new life starts).</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="money">Money formatter.</param>
        /// <param name="router">Navigation.</param>
        /// <param name="notices">News feed.</param>
        /// <param name="decisions">Applies player decisions (with an undo checkpoint).</param>
        public ScreenContext(Func<LifeSession> session, ILocalizer localizer, MoneyFormatter money, ScreenRouter router, NoticeFeed notices,
            DecisionService decisions)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            Localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            Money = money ?? throw new ArgumentNullException(nameof(money));
            Router = router ?? throw new ArgumentNullException(nameof(router));
            Notices = notices ?? throw new ArgumentNullException(nameof(notices));
            Decisions = decisions ?? throw new ArgumentNullException(nameof(decisions));
        }

        /// <summary>Raised after a screen changed the game state.</summary>
        public event Action Changed;

        /// <summary>The running life, or null.</summary>
        public LifeSession Session => _session();

        /// <summary>Localizer.</summary>
        public ILocalizer Localizer { get; }

        /// <summary>Money formatter.</summary>
        public MoneyFormatter Money { get; }

        /// <summary>Navigation.</summary>
        public ScreenRouter Router { get; }

        /// <summary>News feed.</summary>
        public NoticeFeed Notices { get; }

        /// <summary>Applies player decisions.</summary>
        public DecisionService Decisions { get; }

        /// <summary>
        /// Tells the shell that the game state changed.
        /// </summary>
        public void NotifyChanged() => Changed?.Invoke();
    }
}
