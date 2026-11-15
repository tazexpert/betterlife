using System;
using BetterLife.Core.Achievements;
using BetterLife.Core.Legacy;
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
        /// <param name="achievements">Achievement tracker.</param>
        /// <param name="legacy">Inheritance rules (expected shares on the life summary).</param>
        /// <param name="lives">Replaces the running life (heir or new family).</param>
        public ScreenContext(Func<LifeSession> session, ILocalizer localizer, MoneyFormatter money, ScreenRouter router, NoticeFeed notices,
            DecisionService decisions, AchievementTracker achievements, LegacySettings legacy, ILifeController lives)
        {
            Achievements = achievements ?? throw new ArgumentNullException(nameof(achievements));
            Legacy = legacy ?? throw new ArgumentNullException(nameof(legacy));
            Lives = lives ?? throw new ArgumentNullException(nameof(lives));
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

        /// <summary>Achievement tracker.</summary>
        public AchievementTracker Achievements { get; }

        /// <summary>Inheritance rules.</summary>
        public LegacySettings Legacy { get; }

        /// <summary>Replaces the running life.</summary>
        public ILifeController Lives { get; }

        /// <summary>
        /// Tells the shell that the game state changed.
        /// </summary>
        public void NotifyChanged() => Changed?.Invoke();
    }
}
