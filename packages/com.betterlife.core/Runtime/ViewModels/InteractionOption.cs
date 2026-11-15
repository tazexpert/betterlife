namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// One thing the character can do with a person.
    /// </summary>
    public sealed class InteractionOption
    {
        /// <summary>
        /// Creates an option.
        /// </summary>
        /// <param name="id">Interaction id.</param>
        /// <param name="name">Interaction name.</param>
        /// <param name="isAvailable">Whether it can be done now.</param>
        /// <param name="detail">Chance and cost, or why it is locked.</param>
        public InteractionOption(string id, string name, bool isAvailable, string detail)
        {
            Id = id;
            Name = name;
            IsAvailable = isAvailable;
            Detail = detail;
        }

        /// <summary>Interaction id.</summary>
        public string Id { get; }

        /// <summary>Interaction name.</summary>
        public string Name { get; }

        /// <summary>Whether it can be done now.</summary>
        public bool IsAvailable { get; }

        /// <summary>Chance and cost, or why it is locked.</summary>
        public string Detail { get; }
    }
}
