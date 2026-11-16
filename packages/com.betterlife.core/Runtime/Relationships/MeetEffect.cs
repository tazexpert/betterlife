using System;
using System.Collections.Generic;
using System.Globalization;
using BetterLife.Core.Events;
using BetterLife.Core.Randomness;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// A chance to meet someone new (a friend at a party, an acquaintance at a temple fair). The relationship
    /// service creates the person when the roll succeeds.
    /// </summary>
    public sealed class MeetEffect : IEffect
    {
        /// <summary>Rule text of a meeting (placeholder <c>{subject}</c> = relationship type).</summary>
        public const string MetKey = "rule.effect.meet";
        /// <summary>Preview text (placeholders <c>{subject}</c>, <c>{value}</c> = chance in percent).</summary>
        public const string ChanceKey = "rule.effect.meet_chance";

        private const float Hundred = 100f;

        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="type">Kind of relationship to start (friend, colleague, contact or partner).</param>
        /// <param name="chance">Chance 0..1.</param>
        /// <param name="oppositeSex">True to meet someone of the other sex.</param>
        public MeetEffect(RelationshipType type, float chance, bool oppositeSex = false)
        {
            OppositeSex = oppositeSex;
            if (!(chance > 0f) || chance > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(chance), chance, "Chance must be within (0, 1].");
            }
            Type = type;
            Chance = chance;
        }

        /// <summary>Kind of relationship to start.</summary>
        public RelationshipType Type { get; }

        /// <summary>Chance 0..1.</summary>
        public float Chance { get; }

        /// <summary>True to meet someone of the other sex.</summary>
        public bool OppositeSex { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (!context.Random.Chance(Chance))
            {
                return EffectResult.Empty;
            }
            return new EffectResult(new[] { RuleText.Of(MetKey, (RuleTextKeys.ArgSubject, RelationshipNames.NameOf(Type))) }, new Domain.Stats.StatChangeResult[0], 0m,
                new IGameEvent[] { new MeetRequestedEvent(Type, context.Now, OppositeSex) });
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context)
        {
            return new[]
            {
                RuleText.Of(ChanceKey, (RuleTextKeys.ArgSubject, RelationshipNames.NameOf(Type)),
                    (RuleTextKeys.ArgValue, (Chance * Hundred).ToString("0", CultureInfo.InvariantCulture))),
            };
        }
    }
}
