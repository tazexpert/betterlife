using System;
using System.Linq;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Checks relationships with living NPCs: how many of a type, or the value of the closest one of a type.
    /// </summary>
    public sealed class RelationshipCondition : ICondition
    {
        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="type">Type, or null for any.</param>
        /// <param name="countMode">True to compare the count; false to compare the closest value.</param>
        /// <param name="op">Comparison.</param>
        /// <param name="value">Count or value to compare with.</param>
        public RelationshipCondition(RelationshipType? type, bool countMode, ComparisonOperator op, float value)
        {
            Type = type;
            CountMode = countMode;
            Operator = op;
            Value = value;
        }

        /// <summary>Type, or null for any.</summary>
        public RelationshipType? Type { get; }

        /// <summary>True to compare the count; false to compare the closest value.</summary>
        public bool CountMode { get; }

        /// <summary>Comparison.</summary>
        public ComparisonOperator Operator { get; }

        /// <summary>Count or value to compare with.</summary>
        public float Value { get; }

        /// <inheritdoc />
        public bool Evaluate(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            CharacterRelationships people = context.Character.Relationships;
            if (CountMode)
            {
                return Comparison.Compare(people.Living(Type).Count(), Operator, Value);
            }
            Relationship closest = people.Closest(Type);
            return closest != null && Comparison.Compare(closest.Value, Operator, Value);
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            string subject = Type.HasValue ? RelationshipNames.NameOf(Type.Value) : RelationshipConditionConverter.AnyRelation;
            RuleText text = RuleText.Of(CountMode ? RuleTextKeys.CondRelationshipCount : RuleTextKeys.CondRelationshipValue,
                (RuleTextKeys.ArgSubject, subject), (RuleTextKeys.ArgOp, Comparison.ToSymbol(Operator)), (RuleTextKeys.ArgValue, RuleText.Number(Value)));
            return new ConditionTrace(Evaluate(context), text, true);
        }
    }
}
