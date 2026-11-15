using BetterLife.Core.Luck;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// What an interaction did.
    /// </summary>
    public sealed class InteractionResult
    {
        /// <summary>
        /// Creates the result.
        /// </summary>
        /// <param name="quote">The quote it was based on.</param>
        /// <param name="performed">True when it happened (it was available).</param>
        /// <param name="success">True when it succeeded.</param>
        /// <param name="roll">Chance roll, or null when certain or not performed.</param>
        /// <param name="valueChange">Applied relationship change.</param>
        /// <param name="newType">New relationship type, or null.</param>
        /// <param name="child">Adopted child, or null.</param>
        /// <param name="effects">Effects applied to the character.</param>
        public InteractionResult(InteractionQuote quote, bool performed, bool success, ChanceRoll roll, float valueChange, RelationshipType? newType,
            Npc child, EffectResult effects)
        {
            Quote = quote;
            Performed = performed;
            Success = success;
            Roll = roll;
            ValueChange = valueChange;
            NewType = newType;
            Child = child;
            Effects = effects ?? EffectResult.Empty;
        }

        /// <summary>The quote.</summary>
        public InteractionQuote Quote { get; }

        /// <summary>True when it happened.</summary>
        public bool Performed { get; }

        /// <summary>True when it succeeded.</summary>
        public bool Success { get; }

        /// <summary>Chance roll, or null.</summary>
        public ChanceRoll Roll { get; }

        /// <summary>Applied relationship change.</summary>
        public float ValueChange { get; }

        /// <summary>New relationship type, or null.</summary>
        public RelationshipType? NewType { get; }

        /// <summary>Adopted child, or null.</summary>
        public Npc Child { get; }

        /// <summary>Effects applied to the character.</summary>
        public EffectResult Effects { get; }
    }
}
