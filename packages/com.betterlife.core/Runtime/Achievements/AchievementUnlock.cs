using System;

namespace BetterLife.Core.Achievements
{
    /// <summary>
    /// When and by whom an achievement was unlocked.
    /// </summary>
    [Serializable]
    public sealed class AchievementUnlock
    {
        /// <summary>Achievement id.</summary>
        public string Id;

        /// <summary>Real-world day (yyyymmdd).</summary>
        public int Day;

        /// <summary>Generation of the character who unlocked it.</summary>
        public int Generation;

        /// <summary>Name of the character who unlocked it.</summary>
        public string Name;
    }
}
