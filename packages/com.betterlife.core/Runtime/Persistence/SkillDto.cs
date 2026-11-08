using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of skill progress.</summary>
    [Serializable]
    public sealed class SkillDto
    {
        /// <summary>Skill id.</summary>
        public string Id;

        /// <summary>Current level.</summary>
        public float Level;

        /// <summary>Peak level.</summary>
        public float Peak;

        /// <summary>Last practice week (GameDate.TotalWeeks).</summary>
        public int LastPracticedWeek;
    }
}
