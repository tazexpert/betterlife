using System;
using System.Collections.Generic;
using BetterLife.Core.Events;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// The closest living person of a relationship type dies (a best friend's sudden death). Nothing happens if there is none.
    /// The event's own effects carry the grief.
    /// </summary>
    public sealed class NpcDeathEffect : IEffect
    {
        /// <summary>Rule text (placeholder <c>{subject}</c> = relationship type).</summary>
        public const string DiedKey = "rule.effect.npc_death";

        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="type">Relationship type of the person who dies.</param>
        public NpcDeathEffect(RelationshipType type)
        {
            Type = type;
        }

        /// <summary>Relationship type of the person who dies.</summary>
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
            Npc npc = context.Character.Relationships.Npc(relationship.NpcId);
            npc.Die(context.Now);
            return new EffectResult(new[] { Text() }, new Domain.Stats.StatChangeResult[0], 0m,
                new IGameEvent[] { new NpcDiedEvent(npc, relationship, context.Now) });
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context)
        {
            return context?.Character.Relationships.Closest(Type) != null ? new[] { Text() } : new RuleText[0];
        }

        private RuleText Text() => RuleText.Of(DiedKey, (RuleTextKeys.ArgSubject, RelationshipNames.NameOf(Type)));
    }
}
