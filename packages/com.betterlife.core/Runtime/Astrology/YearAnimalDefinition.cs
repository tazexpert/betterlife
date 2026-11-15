using BetterLife.Core.Domain;

namespace BetterLife.Core.Astrology
{
    /// <summary>
    /// One of the twelve year animals (nakshatra years) and its element.
    /// </summary>
    public sealed class YearAnimalDefinition : IDefinition
    {
        /// <summary>
        /// Creates a year animal.
        /// </summary>
        /// <param name="id">Animal id.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="elementId">Element id.</param>
        public YearAnimalDefinition(string id, string nameKey, string elementId)
        {
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            ElementId = ContentId.Require(elementId, nameof(elementId));
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Element id.</summary>
        public string ElementId { get; }
    }
}
