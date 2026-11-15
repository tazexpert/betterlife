using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Rules.Effects
{
    /// <summary>
    /// Reveals a hidden trait the character already has. Not previewed, so the discovery stays a surprise.
    /// </summary>
    public sealed class RevealTraitEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="traitId">Trait id.</param>
        public RevealTraitEffect(string traitId)
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
            if (!context.Character.Traits.Reveal(TraitId))
            {
                return EffectResult.Empty;
            }
            string nameKey = context.Character.Catalogs.Traits.Get(TraitId).NameKey;
            return EffectResult.FromLine(RuleText.Of(RuleTextKeys.EffectTraitReveal, (RuleTextKeys.ArgSubjectKey, nameKey)));
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context) => new RuleText[0];
    }
}
