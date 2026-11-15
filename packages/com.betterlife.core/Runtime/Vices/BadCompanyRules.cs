using System;
using System.Collections.Generic;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// How friends with bad habits raise temptation.
    /// </summary>
    public sealed class BadCompanyRules
    {
        /// <summary>
        /// Creates the value.
        /// </summary>
        /// <param name="traitIds">NPC traits that make someone bad company.</param>
        /// <param name="minValue">Relationship value from which a friend influences.</param>
        /// <param name="perFriend">Multiplier added per bad friend.</param>
        /// <param name="max">Largest multiplier.</param>
        public BadCompanyRules(IReadOnlyCollection<string> traitIds, float minValue, float perFriend, float max)
        {
            if (perFriend < 0f || max < 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(traitIds), "Values are out of range.");
            }
            TraitIds = traitIds ?? throw new ArgumentNullException(nameof(traitIds));
            MinValue = minValue;
            PerFriend = perFriend;
            Max = max;
        }

        /// <summary>NPC traits that make someone bad company.</summary>
        public IReadOnlyCollection<string> TraitIds { get; }

        /// <summary>Relationship value from which a friend influences.</summary>
        public float MinValue { get; }

        /// <summary>Multiplier added per bad friend.</summary>
        public float PerFriend { get; }

        /// <summary>Largest multiplier.</summary>
        public float Max { get; }
    }
}
