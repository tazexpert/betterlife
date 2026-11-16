namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// One choice of a picker: an id and its text.
    /// </summary>
    public sealed class ChoiceItem
    {
        /// <summary>
        /// Creates the item.
        /// </summary>
        /// <param name="id">Id.</param>
        /// <param name="label">Text shown to the player.</param>
        public ChoiceItem(string id, string label)
        {
            Id = id;
            Label = label;
        }

        /// <summary>Id.</summary>
        public string Id { get; }

        /// <summary>Text shown to the player.</summary>
        public string Label { get; }
    }
}
