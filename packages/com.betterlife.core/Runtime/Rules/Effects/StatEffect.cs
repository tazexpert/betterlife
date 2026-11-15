using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Rules.Effects
{
    /// <summary>
    /// Changes a stored stat through <see cref="Domain.Characters.Character.ApplyStatChange"/>, so traits and logging apply.
    /// Changes to hidden stats produce no player-facing text.
    /// </summary>
    public sealed class StatEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="statId">Stored stat.</param>
        /// <param name="amount">Fixed or ranged delta.</param>
        /// <param name="sourceKey">Overrides the context's source key, or null.</param>
        public StatEffect(StatId statId, FloatRange amount, string sourceKey = null)
        {
            StatId = statId;
            Amount = amount;
            SourceKey = sourceKey == null ? null : ContentId.RequireLocKey(sourceKey, nameof(sourceKey));
        }

        /// <summary>Stored stat.</summary>
        public StatId StatId { get; }

        /// <summary>Delta.</summary>
        public FloatRange Amount { get; }

        /// <summary>Source override, or null.</summary>
        public string SourceKey { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            float delta = Amount.Roll(context.Random);
            StatChangeResult change = context.Character.ApplyStatChange(new StatChange(StatId, delta, SourceKey ?? context.SourceKey), context.Now);
            var lines = new List<RuleText>();
            StatDefinition definition = context.Character.Catalogs.Stats.GetDefinition(StatId);
            if (definition.Layer != StatLayer.Hidden && change.AppliedDelta != 0f)
            {
                lines.Add(RuleText.Of(RuleTextKeys.EffectStat, (RuleTextKeys.ArgSubjectKey, definition.NameKey), (RuleTextKeys.ArgValue, RuleText.Number(change.AppliedDelta))));
            }
            return new EffectResult(lines, new[] { change }, 0m);
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            StatDefinition definition = context.Character.Catalogs.Stats.GetDefinition(StatId);
            if (definition.Layer == StatLayer.Hidden)
            {
                return new RuleText[0];
            }
            return new[] { RangeText.Preview(RuleTextKeys.EffectStat, Amount, (RuleTextKeys.ArgSubjectKey, definition.NameKey)) };
        }
    }
}
