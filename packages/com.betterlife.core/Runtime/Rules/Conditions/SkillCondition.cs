using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Rules.Conditions
{
    /// <summary>
    /// Compares a skill level with a value.
    /// </summary>
    public sealed class SkillCondition : ICondition
    {
        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="skillId">Skill id.</param>
        /// <param name="op">Comparison.</param>
        /// <param name="value">Level to compare with (0-100).</param>
        public SkillCondition(string skillId, ComparisonOperator op, float value)
        {
            if (!StatBounds.IsInRange(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be within 0-100.");
            }
            SkillId = ContentId.Require(skillId, nameof(skillId));
            Operator = op;
            Value = value;
        }

        /// <summary>Skill id.</summary>
        public string SkillId { get; }

        /// <summary>Comparison.</summary>
        public ComparisonOperator Operator { get; }

        /// <summary>Level to compare with.</summary>
        public float Value { get; }
        /// <inheritdoc />
        public bool Evaluate(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return Comparison.Compare(context.Character.Skills.GetLevel(SkillId), Operator, Value);
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            string nameKey = context.Character.Catalogs.Skills.Get(SkillId).NameKey;
            RuleText text = RuleText.Of(RuleTextKeys.CondSkill,
                (RuleTextKeys.ArgSubjectKey, nameKey), (RuleTextKeys.ArgOp, Comparison.ToSymbol(Operator)), (RuleTextKeys.ArgValue, RuleText.Number(Value)));
            return new ConditionTrace(Evaluate(context), text, true);
        }
    }
}
