using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Achievements
{
    /// <summary>
    /// An achievement from content: unlocked once, forever, when its condition holds for the running life.
    /// </summary>
    public sealed class AchievementDefinition : IDefinition
    {
        /// <summary>
        /// Creates a definition.
        /// </summary>
        /// <param name="id">Achievement id.</param>
        /// <param name="nameKey">Name key.</param>
        /// <param name="descKey">Description key (how to get it).</param>
        /// <param name="hidden">Whether the description stays secret until unlocked.</param>
        /// <param name="condition">Condition checked against the running life.</param>
        public AchievementDefinition(string id, string nameKey, string descKey, bool hidden, ICondition condition)
        {
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            DescKey = ContentId.RequireLocKey(descKey, nameof(descKey));
            Hidden = hidden;
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        /// <summary>Achievement id.</summary>
        public string Id { get; }

        /// <summary>Name key.</summary>
        public string NameKey { get; }

        /// <summary>Description key.</summary>
        public string DescKey { get; }

        /// <summary>Whether the description stays secret until unlocked.</summary>
        public bool Hidden { get; }

        /// <summary>Condition checked against the running life.</summary>
        public ICondition Condition { get; }
    }
}
