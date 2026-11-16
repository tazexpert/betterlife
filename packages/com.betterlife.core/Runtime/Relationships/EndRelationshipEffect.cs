using System;
using System.Collections.Generic;
using BetterLife.Core.Events;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Ends the closest living romantic relationship of a type (a divorce or a break-up): the person becomes an ex.
    /// </summary>
    public sealed class EndRelationshipEffect : IEffect
    {
        /// <summary>Rule text (placeholder <c>{subject}</c> = relationship type that ended).</summary>
        public const string EndedKey = "rule.effect.relationship_end";

        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="type">Partner or spouse.</param>
        public EndRelationshipEffect(RelationshipType type)
        {
            if (!RelationshipNames.IsRomantic(type))
            {
                throw new ArgumentOutOfRangeException(nameof(type), type, "Only partners and spouses can be left.");
            }
            Type = type;
        }

        /// <summary>Partner or spouse.</summary>
        public RelationshipType Type { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            Relationship relationship = context.Character.Relationships.Closest(Type);
            if (relationship == null)
            {
                return EffectResult.Empty;
            }
            relationship.Become(RelationshipType.Ex, context.Now);
            return new EffectResult(new[] { Text() }, new Domain.Stats.StatChangeResult[0], 0m,
                new IGameEvent[] { new RelationshipTypeChangedEvent(relationship, Type, RelationshipType.Ex, context.Now) });
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context)
        {
            return context?.Character.Relationships.Closest(Type) != null ? new[] { Text() } : new RuleText[0];
        }

        private RuleText Text() => RuleText.Of(EndedKey, (RuleTextKeys.ArgSubject, RelationshipNames.NameOf(Type)));
    }
}
