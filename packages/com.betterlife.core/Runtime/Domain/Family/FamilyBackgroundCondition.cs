using System;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Domain.Family
{
    /// <summary>
    /// Passes when the character was born into the given family background.
    /// </summary>
    public sealed class FamilyBackgroundCondition : ICondition
    {
        /// <summary>Localization key of the description.</summary>
        public const string TextKey = "rule.cond.family_background";

        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="backgroundId">Background id.</param>
        public FamilyBackgroundCondition(string backgroundId)
        {
            BackgroundId = ContentId.Require(backgroundId, nameof(backgroundId));
        }

        /// <summary>Background id.</summary>
        public string BackgroundId { get; }

        /// <inheritdoc />
        public bool Evaluate(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return context.Character.Family != null && context.Character.Family.BackgroundId == BackgroundId;
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            string nameKey = context.Character.Catalogs.FamilyBackgrounds.Get(BackgroundId).NameKey;
            return new ConditionTrace(Evaluate(context), RuleText.Of(TextKey, (RuleTextKeys.ArgSubjectKey, nameKey)), true);
        }
    }
}
