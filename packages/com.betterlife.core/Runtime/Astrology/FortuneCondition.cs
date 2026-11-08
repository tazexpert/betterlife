using System;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Astrology
{
    /// <summary>
    /// Compares this year's fortune in an area with a value. Visible to the player (fortune is shown on the astrology screen).
    /// </summary>
    public sealed class FortuneCondition : ICondition
    {
        /// <summary>Localization key of the description.</summary>
        public const string TextKey = "rule.cond.fortune";

        private readonly FortuneCalculator _fortune;

        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="fortune">Fortune calculator.</param>
        /// <param name="area">Area.</param>
        /// <param name="op">Comparison.</param>
        /// <param name="value">Value within 0-100.</param>
        public FortuneCondition(FortuneCalculator fortune, FortuneArea area, ComparisonOperator op, float value)
        {
            if (!StatBounds.IsInRange(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be within 0-100.");
            }
            _fortune = fortune ?? throw new ArgumentNullException(nameof(fortune));
            Area = area;
            Operator = op;
            Value = value;
        }

        /// <summary>Area.</summary>
        public FortuneArea Area { get; }

        /// <summary>Comparison.</summary>
        public ComparisonOperator Operator { get; }

        /// <summary>Value to compare with.</summary>
        public float Value { get; }

        /// <inheritdoc />
        public bool Evaluate(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return Comparison.Compare(_fortune.Fortune(context.Character, Area, context.Now), Operator, Value);
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            RuleText text = RuleText.Of(TextKey,
                (RuleTextKeys.ArgSubject, FortuneAreas.ToName(Area)),
                (RuleTextKeys.ArgOp, Comparison.ToSymbol(Operator)),
                (RuleTextKeys.ArgValue, RuleText.Number(Value)));
            return new ConditionTrace(Evaluate(context), text, true);
        }
    }
}
