using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Achievements
{
    /// <summary>
    /// Achievements unlocked by the player across all lives (saved in a profile slot).
    /// </summary>
    [Serializable]
    public sealed class AchievementProfile
    {
        /// <summary>Unlocked achievements, in unlock order.</summary>
        public List<AchievementUnlock> Unlocked = new List<AchievementUnlock>();

        /// <summary>
        /// Whether an achievement is unlocked.
        /// </summary>
        /// <param name="id">Achievement id.</param>
        /// <returns>True when unlocked.</returns>
        public bool Has(string id) => Find(id) != null;

        /// <summary>
        /// Finds an unlock.
        /// </summary>
        /// <param name="id">Achievement id.</param>
        /// <returns>The unlock, or null.</returns>
        public AchievementUnlock Find(string id) => (Unlocked ?? new List<AchievementUnlock>()).FirstOrDefault(u => u != null && u.Id == id);
    }
}
