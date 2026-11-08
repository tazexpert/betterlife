using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Astrology
{
    /// <summary>
    /// JSON names of <see cref="FortuneArea"/> values.
    /// </summary>
    public static class FortuneAreas
    {
        private static readonly Dictionary<string, FortuneArea> ByName = new Dictionary<string, FortuneArea>(StringComparer.Ordinal)
        {
            { "finance", FortuneArea.Finance },
            { "love", FortuneArea.Love },
            { "health", FortuneArea.Health },
            { "career", FortuneArea.Career },
            { "study", FortuneArea.Study },
        };

        /// <summary>All areas in declaration order.</summary>
        public static readonly IReadOnlyList<FortuneArea> All = ByName.Values.ToList();

        /// <summary>All JSON names.</summary>
        public static IReadOnlyDictionary<string, FortuneArea> Names => ByName;

        /// <summary>
        /// Returns the JSON name of an area.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <returns>Name such as "career".</returns>
        public static string ToName(FortuneArea area) => ByName.First(p => p.Value == area).Key;
    }
}
