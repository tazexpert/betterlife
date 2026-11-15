namespace BetterLife.Core.Rules
{
    /// <summary>
    /// A data-driven requirement. New kinds are new classes registered in the <see cref="Serialization.RuleRegistry"/>.
    /// Conditions must not change state.
    /// </summary>
    public interface ICondition
    {
        /// <summary>
        /// Returns true when the requirement is met.
        /// </summary>
        /// <param name="context">Rule context.</param>
        /// <returns>True when met.</returns>
        bool Evaluate(RuleContext context);

        /// <summary>
        /// Evaluates and describes the requirement.
        /// </summary>
        /// <param name="context">Rule context.</param>
        /// <returns>The trace.</returns>
        ConditionTrace Explain(RuleContext context);
    }
}
