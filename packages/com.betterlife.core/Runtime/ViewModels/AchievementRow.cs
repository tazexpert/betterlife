namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// One achievement in the list.
    /// </summary>
    public sealed class AchievementRow
    {
        /// <summary>
        /// Creates a row.
        /// </summary>
        /// <param name="id">Achievement id.</param>
        /// <param name="name">Name (hidden ones read "???" until unlocked).</param>
        /// <param name="description">How to get it, or a secret note.</param>
        /// <param name="isUnlocked">Whether it is unlocked.</param>
        /// <param name="detail">Who unlocked it, or empty.</param>
        public AchievementRow(string id, string name, string description, bool isUnlocked, string detail)
        {
            Id = id;
            Name = name;
            Description = description;
            IsUnlocked = isUnlocked;
            Detail = detail ?? string.Empty;
        }

        /// <summary>Achievement id.</summary>
        public string Id { get; }

        /// <summary>Name.</summary>
        public string Name { get; }

        /// <summary>How to get it, or a secret note.</summary>
        public string Description { get; }

        /// <summary>Whether it is unlocked.</summary>
        public bool IsUnlocked { get; }

        /// <summary>Who unlocked it, or empty.</summary>
        public string Detail { get; }
    }
}
