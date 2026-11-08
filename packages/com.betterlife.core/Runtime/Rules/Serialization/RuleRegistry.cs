using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization
{
    /// <summary>
    /// Maps JSON "type" values to converters. Adding a rule type means adding a class and a converter and
    /// registering it here at startup; parsing code never changes.
    /// </summary>
    public sealed class RuleRegistry
    {
        private readonly Dictionary<string, IConditionConverter> _conditionsByName = new Dictionary<string, IConditionConverter>(StringComparer.Ordinal);
        private readonly Dictionary<Type, IConditionConverter> _conditionsByType = new Dictionary<Type, IConditionConverter>();
        private readonly Dictionary<string, IEffectConverter> _effectsByName = new Dictionary<string, IEffectConverter>(StringComparer.Ordinal);
        private readonly Dictionary<Type, IEffectConverter> _effectsByType = new Dictionary<Type, IEffectConverter>();

        /// <summary>Registered condition type names.</summary>
        public IReadOnlyCollection<string> ConditionTypes => _conditionsByName.Keys;

        /// <summary>Registered effect type names.</summary>
        public IReadOnlyCollection<string> EffectTypes => _effectsByName.Keys;

        /// <summary>
        /// Registers a condition converter.
        /// </summary>
        /// <param name="converter">Converter with a unique type name and CLR type.</param>
        /// <returns>This registry, for chaining.</returns>
        public RuleRegistry Register(IConditionConverter converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }
            if (_conditionsByName.ContainsKey(converter.Type) || _conditionsByType.ContainsKey(converter.ConditionType))
            {
                throw new ArgumentException($"Condition type '{converter.Type}' is already registered.", nameof(converter));
            }
            _conditionsByName.Add(converter.Type, converter);
            _conditionsByType.Add(converter.ConditionType, converter);
            return this;
        }

        /// <summary>
        /// Registers an effect converter.
        /// </summary>
        /// <param name="converter">Converter with a unique type name and CLR type.</param>
        /// <returns>This registry, for chaining.</returns>
        public RuleRegistry Register(IEffectConverter converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }
            if (_effectsByName.ContainsKey(converter.Type) || _effectsByType.ContainsKey(converter.EffectType))
            {
                throw new ArgumentException($"Effect type '{converter.Type}' is already registered.", nameof(converter));
            }
            _effectsByName.Add(converter.Type, converter);
            _effectsByType.Add(converter.EffectType, converter);
            return this;
        }

        /// <summary>
        /// Parses a condition object.
        /// </summary>
        /// <param name="token">JSON object with a "type".</param>
        /// <param name="context">Read context of the token.</param>
        /// <returns>The condition.</returns>
        public ICondition ParseCondition(JToken token, RuleReadContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            JObject json = RuleJson.RequireObject(token, context);
            string type = RuleJson.RequireString(json, RuleJson.TypeProperty, context);
            if (!_conditionsByName.TryGetValue(type, out IConditionConverter converter))
            {
                throw context.Property(RuleJson.TypeProperty).Error($"unknown condition type '{type}'.");
            }
            return converter.Read(json, context, this);
        }

        /// <summary>
        /// Parses an array of effect objects.
        /// </summary>
        /// <param name="token">JSON array.</param>
        /// <param name="context">Read context of the token.</param>
        /// <returns>The effects in order.</returns>
        public EffectSequence ParseEffects(JToken token, RuleReadContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            JArray array = RuleJson.RequireArray(token, context);
            return new EffectSequence(array.Select((item, i) => ParseEffect(item, context.Index(i))).ToList());
        }

        /// <summary>
        /// Parses one effect object.
        /// </summary>
        /// <param name="token">JSON object with a "type".</param>
        /// <param name="context">Read context of the token.</param>
        /// <returns>The effect.</returns>
        public IEffect ParseEffect(JToken token, RuleReadContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            JObject json = RuleJson.RequireObject(token, context);
            string type = RuleJson.RequireString(json, RuleJson.TypeProperty, context);
            if (!_effectsByName.TryGetValue(type, out IEffectConverter converter))
            {
                throw context.Property(RuleJson.TypeProperty).Error($"unknown effect type '{type}'.");
            }
            return converter.Read(json, context, this);
        }

        /// <summary>
        /// Writes a condition.
        /// </summary>
        /// <param name="condition">Condition of a registered type.</param>
        /// <returns>The JSON object.</returns>
        public JObject WriteCondition(ICondition condition)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }
            if (!_conditionsByType.TryGetValue(condition.GetType(), out IConditionConverter converter))
            {
                throw new ArgumentException($"No converter registered for {condition.GetType().Name}.", nameof(condition));
            }
            return converter.Write(condition, this);
        }

        /// <summary>
        /// Writes an effect sequence as an array.
        /// </summary>
        /// <param name="effects">Effects of registered types.</param>
        /// <returns>The JSON array.</returns>
        public JArray WriteEffects(EffectSequence effects)
        {
            if (effects == null)
            {
                throw new ArgumentNullException(nameof(effects));
            }
            return new JArray(effects.Effects.Select(WriteEffect));
        }

        /// <summary>
        /// Writes one effect.
        /// </summary>
        /// <param name="effect">Effect of a registered type.</param>
        /// <returns>The JSON object.</returns>
        public JObject WriteEffect(IEffect effect)
        {
            if (effect == null)
            {
                throw new ArgumentNullException(nameof(effect));
            }
            if (!_effectsByType.TryGetValue(effect.GetType(), out IEffectConverter converter))
            {
                throw new ArgumentException($"No converter registered for {effect.GetType().Name}.", nameof(effect));
            }
            return converter.Write(effect, this);
        }
    }
}
