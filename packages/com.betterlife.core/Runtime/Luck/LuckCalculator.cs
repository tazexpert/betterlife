using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Astrology;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Karma;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time;

namespace BetterLife.Core.Luck
{
    /// <summary>
    /// Computes and rolls success chances: FinalChance = Base + Stat + Luck + Karma + Item, clamped to the settings' bounds.
    /// </summary>
    public sealed class LuckCalculator
    {
        private const float PercentScale = 100f;

        private readonly LuckSettings _settings;
        private readonly FortuneCalculator _fortune;
        private readonly KarmaService _karma;
        private readonly List<IChanceBonusSource> _bonusSources;

        /// <summary>
        /// Creates the calculator.
        /// </summary>
        /// <param name="settings">Luck settings.</param>
        /// <param name="fortune">Fortune calculator.</param>
        /// <param name="karma">Karma service.</param>
        /// <param name="bonusSources">Item bonus sources (may be empty).</param>
        public LuckCalculator(LuckSettings settings, FortuneCalculator fortune, KarmaService karma, IEnumerable<IChanceBonusSource> bonusSources = null)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _fortune = fortune ?? throw new ArgumentNullException(nameof(fortune));
            _karma = karma ?? throw new ArgumentNullException(nameof(karma));
            _bonusSources = (bonusSources ?? Enumerable.Empty<IChanceBonusSource>()).ToList();
        }

        /// <summary>
        /// Explains the chance without rolling.
        /// </summary>
        /// <param name="spec">Chance spec.</param>
        /// <param name="character">Character.</param>
        /// <param name="now">Current week.</param>
        /// <returns>The breakdown.</returns>
        public ChanceBreakdown Explain(ChanceSpec spec, Character character, GameDate now)
        {
            if (spec == null)
            {
                throw new ArgumentNullException(nameof(spec));
            }
            if (character == null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            float stat = spec.StatTerms.Sum(t => t.Contribution(character.Stats.GetValue(t.StatId)));
            float luck = spec.FortuneArea.HasValue
                ? (_fortune.Fortune(character, spec.FortuneArea.Value, now) - _settings.FortuneNeutral) * _settings.PointsPerFortune
                : 0f;
            float karma = spec.UseKarma ? _karma.ChancePoints(character.Karma) : 0f;
            float items = _bonusSources.Sum(s => s.BonusFor(character, spec, now));
            float item = Math.Max(0f, Math.Min(_settings.ItemBonusCap, items));
            float raw = spec.BasePercent + stat + luck + karma + item;
            float final = Math.Max(_settings.ChanceMin, Math.Min(_settings.ChanceMax, raw));
            return new ChanceBreakdown(spec.BasePercent, stat, luck, karma, item, final);
        }

        /// <summary>
        /// Computes and rolls the chance (consumes one random value).
        /// </summary>
        /// <param name="spec">Chance spec.</param>
        /// <param name="character">Character.</param>
        /// <param name="now">Current week.</param>
        /// <param name="random">Random stream.</param>
        /// <returns>The roll.</returns>
        public ChanceRoll Roll(ChanceSpec spec, Character character, GameDate now, IRandomProvider random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }
            ChanceBreakdown breakdown = Explain(spec, character, now);
            return new ChanceRoll(breakdown, random.NextFloat() * PercentScale);
        }
    }
}
