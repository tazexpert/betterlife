using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Astrology;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Luck
{
    /// <summary>
    /// How a success chance is computed for one choice: base percent, stat terms, optional fortune area and karma.
    /// </summary>
    public sealed class ChanceSpec
    {
        /// <summary>
        /// Creates a spec.
        /// </summary>
        /// <param name="basePercent">Base chance in percent (0-100).</param>
        /// <param name="statTerms">Stat terms.</param>
        /// <param name="fortuneArea">Fortune area that applies, or null.</param>
        /// <param name="useKarma">Whether karma applies.</param>
        public ChanceSpec(float basePercent, IEnumerable<ChanceStatTerm> statTerms, FortuneArea? fortuneArea, bool useKarma)
        {
            if (!StatBounds.IsInRange(basePercent))
            {
                throw new ArgumentOutOfRangeException(nameof(basePercent), basePercent, "Base chance must be within 0-100.");
            }
            BasePercent = basePercent;
            StatTerms = (statTerms ?? Enumerable.Empty<ChanceStatTerm>()).ToList();
            FortuneArea = fortuneArea;
            UseKarma = useKarma;
        }

        /// <summary>Base chance in percent.</summary>
        public float BasePercent { get; }

        /// <summary>Stat terms.</summary>
        public IReadOnlyList<ChanceStatTerm> StatTerms { get; }

        /// <summary>Fortune area, or null.</summary>
        public FortuneArea? FortuneArea { get; }

        /// <summary>Whether karma applies.</summary>
        public bool UseKarma { get; }

        /// <summary>
        /// Returns a copy whose base chance is moved by a number of points (clamped to 0-100), for situational bonuses
        /// such as an exam score above the difficulty.
        /// </summary>
        /// <param name="points">Points to add (negative to subtract).</param>
        /// <returns>The adjusted spec.</returns>
        public ChanceSpec WithBonus(float points) => new ChanceSpec(StatBounds.Clamp(BasePercent + points), StatTerms, FortuneArea, UseKarma);
    }
}
