using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Domain.Life
{
    /// <summary>
    /// Passes when the character is in one of the given life stages.
    /// </summary>
    public sealed class LifeStageCondition : ICondition
    {
        /// <summary>Localization key of the description.</summary>
        public const string TextKey = "rule.cond.life_stage";

        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="stageIds">At least one stage id.</param>
        public LifeStageCondition(IEnumerable<string> stageIds)
        {
            StageIds = (stageIds ?? throw new ArgumentNullException(nameof(stageIds))).Select(id => ContentId.Require(id, nameof(stageIds))).ToList();
            if (StageIds.Count == 0)
            {
                throw new ArgumentException("At least one stage is required.", nameof(stageIds));
            }
        }

        /// <summary>Accepted stage ids.</summary>
        public IReadOnlyList<string> StageIds { get; }

        /// <inheritdoc />
        public bool Evaluate(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return StageIds.Contains(context.Character.LifeStage(context.Now).Id);
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            string names = string.Join(",", StageIds.Select(id => context.Character.Catalogs.LifeStages.Get(id).NameKey));
            return new ConditionTrace(Evaluate(context), RuleText.Of(TextKey, (RuleTextKeys.ArgSubject, names)), true);
        }
    }
}
