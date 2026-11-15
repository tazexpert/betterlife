using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Changes relationships with living NPCs of a type: the closest one or all of them, optionally leaving a memory.
    /// Produces no player-facing text.
    /// </summary>
    public sealed class RelationshipEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="type">Type, or null for any.</param>
        /// <param name="amount">Fixed or ranged change.</param>
        /// <param name="all">True for everyone of the type; false for the closest.</param>
        /// <param name="memoryKey">Memory to leave, or null.</param>
        public RelationshipEffect(RelationshipType? type, FloatRange amount, bool all, string memoryKey)
        {
            Type = type;
            Amount = amount;
            All = all;
            MemoryKey = memoryKey;
        }

        /// <summary>Type, or null for any.</summary>
        public RelationshipType? Type { get; }

        /// <summary>Change.</summary>
        public FloatRange Amount { get; }

        /// <summary>True for everyone of the type.</summary>
        public bool All { get; }

        /// <summary>Memory key, or null.</summary>
        public string MemoryKey { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            CharacterRelationships people = context.Character.Relationships;
            IEnumerable<Relationship> targets = All ? people.Living(Type).ToList() : new[] { people.Closest(Type) }.Where(r => r != null);
            foreach (Relationship relationship in targets)
            {
                float change = relationship.Change(Amount.Roll(context.Random));
                if (MemoryKey != null)
                {
                    relationship.Remember(new RelationshipMemory(MemoryKey, context.Now, change));
                }
            }
            return EffectResult.Empty;
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context) => new RuleText[0];
    }
}
