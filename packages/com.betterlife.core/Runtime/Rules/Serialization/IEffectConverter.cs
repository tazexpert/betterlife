using System;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization
{
    /// <summary>
    /// Reads and writes one effect type. Register implementations in a <see cref="RuleRegistry"/>.
    /// </summary>
    public interface IEffectConverter
    {
        /// <summary>The JSON "type" value.</summary>
        string Type { get; }

        /// <summary>The CLR type produced.</summary>
        Type EffectType { get; }

        /// <summary>
        /// Reads an effect.
        /// </summary>
        /// <param name="json">The effect object.</param>
        /// <param name="context">Read context of the object.</param>
        /// <param name="registry">Registry for nested rules.</param>
        /// <returns>The effect.</returns>
        IEffect Read(JObject json, RuleReadContext context, RuleRegistry registry);

        /// <summary>
        /// Writes an effect.
        /// </summary>
        /// <param name="effect">Effect of <see cref="EffectType"/>.</param>
        /// <param name="registry">Registry for nested rules.</param>
        /// <returns>The JSON object.</returns>
        JObject Write(IEffect effect, RuleRegistry registry);
    }
}
