using System;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization
{
    /// <summary>
    /// Typed base for condition converters. Wraps domain validation errors with the JSON path.
    /// </summary>
    /// <typeparam name="T">Condition type.</typeparam>
    public abstract class ConditionConverter<T> : IConditionConverter where T : class, ICondition
    {
        /// <inheritdoc />
        public abstract string Type { get; }

        /// <inheritdoc />
        public Type ConditionType => typeof(T);

        /// <inheritdoc />
        public ICondition Read(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            try
            {
                return ReadTyped(json, context, registry);
            }
            catch (ArgumentException ex)
            {
                throw context.Error(ex.Message, ex);
            }
        }

        /// <inheritdoc />
        public JObject Write(ICondition condition, RuleRegistry registry)
        {
            if (!(condition is T typed))
            {
                throw new ArgumentException($"Expected {typeof(T).Name}.", nameof(condition));
            }
            var json = new JObject { [RuleJson.TypeProperty] = Type };
            WriteTyped(typed, json, registry);
            return json;
        }

        /// <summary>
        /// Reads the typed condition.
        /// </summary>
        /// <param name="json">The condition object.</param>
        /// <param name="context">Read context of the object.</param>
        /// <param name="registry">Registry for nested rules.</param>
        /// <returns>The condition.</returns>
        protected abstract T ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry);

        /// <summary>
        /// Writes properties other than "type".
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="json">Target object (already holds "type").</param>
        /// <param name="registry">Registry for nested rules.</param>
        protected abstract void WriteTyped(T condition, JObject json, RuleRegistry registry);
    }
}
