using System;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization
{
    /// <summary>
    /// Typed base for effect converters. Wraps domain validation errors with the JSON path.
    /// </summary>
    /// <typeparam name="T">Effect type.</typeparam>
    public abstract class EffectConverter<T> : IEffectConverter where T : class, IEffect
    {
        /// <inheritdoc />
        public abstract string Type { get; }

        /// <inheritdoc />
        public Type EffectType => typeof(T);

        /// <inheritdoc />
        public IEffect Read(JObject json, RuleReadContext context, RuleRegistry registry)
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
        public JObject Write(IEffect effect, RuleRegistry registry)
        {
            if (!(effect is T typed))
            {
                throw new ArgumentException($"Expected {typeof(T).Name}.", nameof(effect));
            }
            var json = new JObject { [RuleJson.TypeProperty] = Type };
            WriteTyped(typed, json, registry);
            return json;
        }

        /// <summary>
        /// Reads the typed effect.
        /// </summary>
        /// <param name="json">The effect object.</param>
        /// <param name="context">Read context of the object.</param>
        /// <param name="registry">Registry for nested rules.</param>
        /// <returns>The effect.</returns>
        protected abstract T ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry);

        /// <summary>
        /// Writes properties other than "type".
        /// </summary>
        /// <param name="effect">The effect.</param>
        /// <param name="json">Target object (already holds "type").</param>
        /// <param name="registry">Registry for nested rules.</param>
        protected abstract void WriteTyped(T effect, JObject json, RuleRegistry registry);
    }
}
