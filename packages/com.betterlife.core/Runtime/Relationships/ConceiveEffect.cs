using System;
using System.Collections.Generic;
using BetterLife.Core.Events;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Starts the first child with the spouse (or partner): a pregnancy when the couple can conceive, otherwise an adoption.
    /// The relationship service applies it (it knows the fertility rules).
    /// </summary>
    public sealed class ConceiveEffect : IEffect
    {
        /// <summary>Rule text of the coming child.</summary>
        public const string ChildComingKey = "rule.effect.child_coming";

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (!HasCouple(context))
            {
                return EffectResult.Empty;
            }
            return new EffectResult(new[] { RuleText.Of(ChildComingKey) }, new Domain.Stats.StatChangeResult[0], 0m,
                new IGameEvent[] { new ChildRequestedEvent(context.Now) });
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context)
        {
            return context != null && HasCouple(context) ? new[] { RuleText.Of(ChildComingKey) } : new RuleText[0];
        }

        private static bool HasCouple(RuleContext context)
        {
            CharacterRelationships people = context.Character.Relationships;
            return people.Closest(RelationshipType.Spouse) != null || people.Closest(RelationshipType.Partner) != null;
        }
    }
}
