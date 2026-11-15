using System;

namespace BetterLife.Core.Domain.Skills
{
    /// <summary>
    /// Immutable definition of a skill, loaded from content/skills.json.
    /// </summary>
    public sealed class SkillDefinition : IDefinition
    {
        /// <summary>
        /// Creates a skill definition.
        /// </summary>
        /// <param name="id">Skill id.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="category">Category id.</param>
        /// <param name="gainFalloffExponent">Higher values make progress slow down faster near mastery; non-negative.</param>
        /// <param name="decay">Decay rule.</param>
        public SkillDefinition(string id, string nameKey, string category, float gainFalloffExponent, SkillDecayRule decay)
        {
            if (float.IsNaN(gainFalloffExponent) || gainFalloffExponent < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(gainFalloffExponent), gainFalloffExponent, "Exponent must be non-negative.");
            }

            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            Category = ContentId.Require(category, nameof(category));
            GainFalloffExponent = gainFalloffExponent;
            Decay = decay ?? throw new ArgumentNullException(nameof(decay));
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Category id.</summary>
        public string Category { get; }

        /// <summary>Falloff exponent of gains near mastery.</summary>
        public float GainFalloffExponent { get; }

        /// <summary>Decay rule.</summary>
        public SkillDecayRule Decay { get; }
    }
}
