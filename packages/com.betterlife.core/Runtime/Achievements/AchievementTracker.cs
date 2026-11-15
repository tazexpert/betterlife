using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Randomness;
using BetterLife.Core.Rules;
using BetterLife.Core.Saving;
using BetterLife.Core.Sessions;

namespace BetterLife.Core.Achievements
{
    /// <summary>
    /// Checks the achievements against the running life (after time passes and at death) and unlocks the new ones
    /// once, forever.
    /// </summary>
    public sealed class AchievementTracker
    {
        /// <summary>Rule source key of achievement checks.</summary>
        public const string SourceKey = "source.achievements";

        private const ulong CheckSeed = 0xACE5UL;
        private const int DayYearFactor = 10000;
        private const int DayMonthFactor = 100;

        private readonly DefinitionCatalog<AchievementDefinition> _achievements;
        private readonly AchievementStore _store;
        private readonly IRealTimeClock _clock;
        private AchievementProfile _profile;

        /// <summary>
        /// Creates the tracker.
        /// </summary>
        /// <param name="achievements">Achievement catalog.</param>
        /// <param name="store">Profile store.</param>
        /// <param name="clock">Real-world clock (unlock day).</param>
        public AchievementTracker(DefinitionCatalog<AchievementDefinition> achievements, AchievementStore store, IRealTimeClock clock)
        {
            _achievements = achievements ?? throw new ArgumentNullException(nameof(achievements));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        /// <summary>Achievement catalog.</summary>
        public DefinitionCatalog<AchievementDefinition> Achievements => _achievements;

        /// <summary>The player's unlocked achievements.</summary>
        public AchievementProfile Profile => _profile ?? (_profile = _store.Load());

        /// <summary>
        /// Forgets every unlocked achievement (from the settings screen, after the player confirms).
        /// </summary>
        public void Reset()
        {
            _profile = new AchievementProfile();
            _store.Save(_profile);
        }

        /// <summary>
        /// Unlocks every achievement whose condition now holds.
        /// </summary>
        /// <param name="session">Running (or just finished) life.</param>
        /// <returns>Achievements unlocked by this call, in catalog order.</returns>
        public IReadOnlyList<AchievementDefinition> Evaluate(LifeSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            // A private generator: conditions never roll, and checking must not touch the life's saved streams.
            var context = new RuleContext(session.Character, session.TimeState.Clock.Now, new Pcg32(CheckSeed, CheckSeed), SourceKey);
            List<AchievementDefinition> unlocked = _achievements.All
                .Where(a => !Profile.Has(a.Id) && a.Condition.Evaluate(context))
                .ToList();
            if (unlocked.Count == 0)
            {
                return unlocked;
            }
            DateTime today = _clock.Today;
            string name = session.Character.Name.GivenName + " " + session.Character.Name.FamilyName;
            foreach (AchievementDefinition achievement in unlocked)
            {
                Profile.Unlocked.Add(new AchievementUnlock
                {
                    Id = achievement.Id,
                    Day = today.Year * DayYearFactor + today.Month * DayMonthFactor + today.Day,
                    Generation = session.Character.Generation,
                    Name = name,
                });
            }
            _store.Save(Profile);
            return unlocked;
        }
    }
}
