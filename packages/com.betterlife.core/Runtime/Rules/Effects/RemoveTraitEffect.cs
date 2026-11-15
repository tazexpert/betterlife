using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Traits;

namespace BetterLife.Core.Rules.Effects
{
    /// <summary>
    /// Removes a trait. Text is shown only when the player knew about the trait.
    /// </summary>
    public sealed class RemoveTraitEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="traitId">Trait id.</param>
        public RemoveTraitEffect(string traitId)
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

            TraitInstance held = context.Character.Traits.Instances.FirstOrDefault(i => i.TraitId == TraitId);
            bool wasKnown = held != null && held.IsRevealed;
            if (!context.Character.Traits.Remove(TraitId) || !wasKnown)
            {
                return EffectResult.Empty;
            }
            string nameKey = context.Character.Catalogs.Traits.Get(TraitId).NameKey;
            return EffectResult.FromLine(RuleText.Of(RuleTextKeys.EffectTraitRemove, (RuleTextKeys.ArgSubjectKey, nameKey)));
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            TraitInstance held = context.Character.Traits.Instances.FirstOrDefault(i => i.TraitId == TraitId);
            if (held == null || !held.IsRevealed)
            {
                return new RuleText[0];
            }
            string nameKey = context.Character.Catalogs.Traits.Get(TraitId).NameKey;
            return new[] { RuleText.Of(RuleTextKeys.EffectTraitRemove, (RuleTextKeys.ArgSubjectKey, nameKey)) };
        }
    }
}
