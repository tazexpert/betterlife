using System;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Astrology
{
    /// <summary>
    /// An element of the birth year (earth, water, fire, wood, metal).
    /// </summary>
    public sealed class ElementDefinition : IDefinition
    {
        /// <summary>
        /// Creates an element.
        /// </summary>
        /// <param name="id">Element id.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="modifiers">Fortune points.</param>
        public ElementDefinition(string id, string nameKey, FortuneModifiers modifiers)
        {
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            Modifiers = modifiers ?? throw new ArgumentNullException(nameof(modifiers));
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Fortune points.</summary>
        public FortuneModifiers Modifiers { get; }
    }
}
