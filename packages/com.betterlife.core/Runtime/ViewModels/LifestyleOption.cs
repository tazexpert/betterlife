namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// One lifestyle tier the character can live at.
    /// </summary>
    public sealed class LifestyleOption
    {
        /// <summary>
        /// Creates an option.
        /// </summary>
        /// <param name="id">Tier id.</param>
        /// <param name="name">Tier name.</param>
        /// <param name="costText">Monthly cost text.</param>
        /// <param name="isSelected">Whether it is the current tier.</param>
        public LifestyleOption(string id, string name, string costText, bool isSelected)
        {
            Id = id;
            Name = name;
            CostText = costText;
            IsSelected = isSelected;
        }

        /// <summary>Tier id.</summary>
        public string Id { get; }

        /// <summary>Tier name.</summary>
        public string Name { get; }

        /// <summary>Monthly cost text.</summary>
        public string CostText { get; }

        /// <summary>Whether it is the current tier.</summary>
        public bool IsSelected { get; }
    }
}
