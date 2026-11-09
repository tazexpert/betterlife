using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// What happens after an interaction succeeds or fails.
    /// </summary>
    public sealed class InteractionOutcome
    {
        /// <summary>
        /// Creates the outcome.
        /// </summary>
        /// <param name="value">Relationship change.</param>
        /// <param name="becomes">New relationship type, or null.</param>
        /// <param name="child">Child brought by the outcome.</param>
        /// <param name="memoryKey">Memory the NPC keeps, or null.</param>
        /// <param name="effects">Rule effects on the character.</param>
        public InteractionOutcome(FloatRange value, RelationshipType? becomes, ChildMode child, string memoryKey, EffectSequence effects)
        {
            Value = value;
            Becomes = becomes;
            Child = child;
            MemoryKey = memoryKey == null ? null : ContentId.RequireLocKey(memoryKey, nameof(memoryKey));
            Effects = effects ?? throw new ArgumentNullException(nameof(effects));
        }

        /// <summary>Relationship change.</summary>
        public FloatRange Value { get; }

        /// <summary>New relationship type, or null.</summary>
        public RelationshipType? Becomes { get; }

        /// <summary>Child brought by the outcome.</summary>
        public ChildMode Child { get; }

        /// <summary>Memory key, or null.</summary>
        public string MemoryKey { get; }

        /// <summary>Rule effects on the character.</summary>
        public EffectSequence Effects { get; }
    }
}
