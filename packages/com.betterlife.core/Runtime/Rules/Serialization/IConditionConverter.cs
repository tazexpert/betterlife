using System;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization
{
    /// <summary>
    /// Reads and writes one condition type. Register implementations in a <see cref="RuleRegistry"/>.
    /// </summary>
    public interface IConditionConverter
    {
        /// <summary>The JSON "type" value.</summary>
        string Type { get; }

        /// <summary>The CLR type produced.</summary>
        Type ConditionType { get; }

        /// <summary>
        /// Reads a condition.
        /// </summary>
        /// <param name="json">The condition object.</param>
        /// <param name="context">Read context of the object.</param>
        /// <param name="registry">Registry for nested rules.</param>
        /// <returns>The condition.</returns>
        ICondition Read(JObject json, RuleReadContext context, RuleRegistry registry);

        /// <summary>
        /// Writes a condition.
        /// </summary>
        /// <param name="condition">Condition of <see cref="ConditionType"/>.</param>
        /// <param name="registry">Registry for nested rules.</param>
        /// <returns>The JSON object.</returns>
        JObject Write(ICondition condition, RuleRegistry registry);
    }
}
