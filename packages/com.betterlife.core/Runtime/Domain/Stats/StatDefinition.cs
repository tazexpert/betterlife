using System;

namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// Immutable definition of one stat, loaded from content/stats.json.
    /// </summary>
    public sealed class StatDefinition
    {
        /// <summary>
        /// Creates a stat definition.
        /// </summary>
        /// <param name="id">Stat id.</param>
        /// <param name="layer">Visibility tier.</param>
        /// <param name="polarity">Whether high is good.</param>
        /// <param name="defaultValue">Starting value (0-100).</param>
        /// <param name="nameKey">Localization key of the display name.</param>
        /// <param name="descKey">Localization key of the description.</param>
        /// <param name="parentId">Core stat this sub-stat belongs to, or null.</param>
        /// <param name="decay">Optional natural drift rule.</param>
        public StatDefinition(
            StatId id,
            StatLayer layer,
            StatPolarity polarity,
            float defaultValue,
            string nameKey,
            string descKey,
            StatId? parentId = null,
            StatDecayRule decay = null)
        {
            if (!StatBounds.IsInRange(defaultValue))
            {
                throw new ArgumentOutOfRangeException(nameof(defaultValue), defaultValue, "Default must be within stat bounds.");
            }

            Id = id;
            Layer = layer;
            Polarity = polarity;
            DefaultValue = defaultValue;
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            DescKey = ContentId.RequireLocKey(descKey, nameof(descKey));
            ParentId = parentId;
            Decay = decay;
        }

        /// <summary>Stat id.</summary>
        public StatId Id { get; }

        /// <summary>Visibility tier.</summary>
        public StatLayer Layer { get; }

        /// <summary>Whether high is good.</summary>
        public StatPolarity Polarity { get; }

        /// <summary>Starting value.</summary>
        public float DefaultValue { get; }

        /// <summary>Localization key of the display name.</summary>
        public string NameKey { get; }

        /// <summary>Localization key of the description.</summary>
        public string DescKey { get; }

        /// <summary>Core stat this sub-stat belongs to, or null.</summary>
        public StatId? ParentId { get; }

        /// <summary>Natural drift rule, or null when the stat does not drift.</summary>
        public StatDecayRule Decay { get; }
    }
}
