using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// The family a life starts with: parent relationships by personality, siblings, and new children.
    /// </summary>
    public sealed class FamilyRules
    {
        /// <summary>
        /// Creates the rules.
        /// </summary>
        /// <param name="parentStart">Starting value per parent personality id.</param>
        /// <param name="defaultParentStart">Starting value for other personalities.</param>
        /// <param name="siblingStart">Starting value with siblings.</param>
        /// <param name="childStart">Starting value with a new child.</param>
        /// <param name="siblingWeights">Weighted sibling counts.</param>
        /// <param name="siblingAgeGapYears">Largest age gap between siblings.</param>
        public FamilyRules(IReadOnlyDictionary<string, float> parentStart, float defaultParentStart, float siblingStart, float childStart,
            IEnumerable<(int Count, float Weight)> siblingWeights, int siblingAgeGapYears)
        {
            ParentStart = parentStart ?? throw new ArgumentNullException(nameof(parentStart));
            SiblingWeights = (siblingWeights ?? Enumerable.Empty<(int, float)>()).ToList();
            if (SiblingWeights.Any(w => w.Count < 0 || w.Weight < 0f) || siblingAgeGapYears < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(siblingWeights), "Sibling counts, weights and gaps must be non-negative.");
            }
            DefaultParentStart = defaultParentStart;
            SiblingStart = siblingStart;
            ChildStart = childStart;
            SiblingAgeGapYears = siblingAgeGapYears;
        }

        /// <summary>Starting value per parent personality id.</summary>
        public IReadOnlyDictionary<string, float> ParentStart { get; }

        /// <summary>Starting value for other personalities.</summary>
        public float DefaultParentStart { get; }

        /// <summary>Starting value with siblings.</summary>
        public float SiblingStart { get; }

        /// <summary>Starting value with a new child.</summary>
        public float ChildStart { get; }

        /// <summary>Weighted sibling counts.</summary>
        public IReadOnlyList<(int Count, float Weight)> SiblingWeights { get; }

        /// <summary>Largest age gap between siblings.</summary>
        public int SiblingAgeGapYears { get; }

        /// <summary>
        /// Returns the starting value with a parent of a personality.
        /// </summary>
        /// <param name="personalityId">Personality id.</param>
        /// <returns>Value.</returns>
        public float ParentValue(string personalityId)
        {
            return personalityId != null && ParentStart.TryGetValue(personalityId, out float value) ? value : DefaultParentStart;
        }
    }
}
