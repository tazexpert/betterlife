using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Traits;

namespace BetterLife.Core.Rules.Effects
{
    /// <summary>
    /// Gives the character a trait (no-op when held or blocked by an exclusive group).
    /// Hidden traits are gained silently.
    /// </summary>
    public sealed class AddTraitEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="traitId">Trait id.</param>
        public AddTraitEffect(string traitId)
        {
            TraitId = ContentId.Require(traitId, nameof(traitId));
        }

        /// <summary>Trait id.</summary>
        public string TraitId { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            TraitDefinition definition = context.Character.Catalogs.Traits.Get(TraitId);
            bool added = context.Character.Traits.Add(TraitId, context.Now) == TraitAddOutcome.Added;
            return added && !definition.StartsHidden
                ? EffectResult.FromLine(RuleText.Of(RuleTextKeys.EffectTraitAdd, (RuleTextKeys.ArgSubjectKey, definition.NameKey)))
                : EffectResult.Empty;
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            TraitDefinition definition = context.Character.Catalogs.Traits.Get(TraitId);
            return definition.StartsHidden
                ? new RuleText[0]
                : new[] { RuleText.Of(RuleTextKeys.EffectTraitAdd, (RuleTextKeys.ArgSubjectKey, definition.NameKey)) };
        }
    }
}
