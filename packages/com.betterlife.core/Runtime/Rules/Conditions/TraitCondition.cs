using System;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Traits;

namespace BetterLife.Core.Rules.Conditions
{
    /// <summary>
    /// Passes when the character has a trait, optionally requiring it to be revealed or still hidden.
    /// </summary>
    public sealed class TraitCondition : ICondition
    {
        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="traitId">Trait id.</param>
        /// <param name="revealed">Required revealed state, or null to ignore it.</param>
        public TraitCondition(string traitId, bool? revealed = null)
        {
            TraitId = ContentId.Require(traitId, nameof(traitId));
            Revealed = revealed;
        }

        /// <summary>Trait id.</summary>
        public string TraitId { get; }

        /// <summary>Required revealed state, or null.</summary>
        public bool? Revealed { get; }
        /// <inheritdoc />
        public bool Evaluate(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            TraitInstance instance = context.Character.Traits.Instances.FirstOrDefault(i => i.TraitId == TraitId);
            return instance != null && (!Revealed.HasValue || instance.IsRevealed == Revealed.Value);
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            TraitDefinition definition = context.Character.Catalogs.Traits.Get(TraitId);
            RuleText text = RuleText.Of(RuleTextKeys.CondTrait, (RuleTextKeys.ArgSubjectKey, definition.NameKey));
            return new ConditionTrace(Evaluate(context), text, !definition.StartsHidden);
        }
    }
}
