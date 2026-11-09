using System;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Per-type tuning: how fast a neglected relationship fades, and the grief when the NPC dies.
    /// </summary>
    public sealed class RelationshipTypeRules
    {
        /// <summary>
        /// Creates the rules.
        /// </summary>
        /// <param name="type">Relationship type.</param>
        /// <param name="monthlyDecay">Points lost per month without attention.</param>
        /// <param name="floor">Value that decay stops at.</param>
        /// <param name="griefStress">Stress added when the NPC dies.</param>
        /// <param name="griefHappiness">Happiness change when the NPC dies.</param>
        public RelationshipTypeRules(RelationshipType type, float monthlyDecay, float floor, float griefStress, float griefHappiness)
        {
            if (monthlyDecay < 0f || floor < Relationship.Min || floor > Relationship.Max || griefStress < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(type), "Values are out of range.");
            }
            Type = type;
            MonthlyDecay = monthlyDecay;
            Floor = floor;
            GriefStress = griefStress;
            GriefHappiness = griefHappiness;
        }

        /// <summary>Relationship type.</summary>
        public RelationshipType Type { get; }

        /// <summary>Points lost per month without attention.</summary>
        public float MonthlyDecay { get; }

        /// <summary>Value that decay stops at.</summary>
        public float Floor { get; }

        /// <summary>Stress added when the NPC dies.</summary>
        public float GriefStress { get; }

        /// <summary>Happiness change when the NPC dies.</summary>
        public float GriefHappiness { get; }
    }
}
