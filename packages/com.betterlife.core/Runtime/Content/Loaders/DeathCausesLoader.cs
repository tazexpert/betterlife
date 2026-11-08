using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Life;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/death_causes.json and registers the "character.die" effect with every cause id.
    /// </summary>
    public sealed class DeathCausesLoader : DefinitionLoader<DeathCauseDefinition>
    {
        private static readonly string[] Dependencies = { ContentTypes.RuleTypes };

        private readonly HashSet<string> _known = new HashSet<string>(StringComparer.Ordinal);

        /// <inheritdoc />
        public override string TypeName => ContentTypes.DeathCauses;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override string ItemsProperty => "causes";

        /// <inheritdoc />
        protected override void BeforeItems(IReadOnlyList<ContentDocument> documents, ContentLoadContext context)
        {
            _known.Clear();
            foreach (JObject item in documents.SelectMany(d => (d.Root[ItemsProperty] as JArray) ?? new JArray()).OfType<JObject>())
            {
                if (item["id"]?.Type == JTokenType.String)
                {
                    _known.Add((string)item["id"]);
                }
            }
            if (!context.Rules.EffectTypes.Contains(DieEffectConverter.TypeName))
            {
                context.Rules.Register(new DieEffectConverter(_known));
            }
        }

        /// <inheritdoc />
        protected override DeathCauseDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "eulogyKey", "condition", "weight");
            RuleRegistry rules = content.Rules;
            ICondition condition = item["condition"] == null ? null : rules.ParseCondition(item["condition"], context.Property("condition"));
            var (weightToken, weightContext) = RuleJson.RequireProperty(item, "weight", context);
            JObject weight = RuleJson.RequireObject(weightToken, weightContext);
            RuleJson.RequireOnly(weight, weightContext, "base", "modifiers");
            var modifiers = new List<WeightModifier>();
            if (weight["modifiers"] != null)
            {
                var (array, arrayContext) = RuleJson.RequireArrayProperty(weight, "modifiers", weightContext);
                for (int i = 0; i < array.Count; i++)
                {
                    RuleReadContext c = arrayContext.Index(i);
                    JObject modifier = RuleJson.RequireObject(array[i], c);
                    RuleJson.RequireOnly(modifier, c, "condition", "multiplier");
                    var (conditionToken, conditionContext) = RuleJson.RequireProperty(modifier, "condition", c);
                    modifiers.Add(new WeightModifier(rules.ParseCondition(conditionToken, conditionContext), RuleJson.RequireFloat(modifier, "multiplier", c)));
                }
            }
            return new DeathCauseDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                RuleJson.OptionalId(item, "eulogyKey", context),
                condition,
                new EventWeight(RuleJson.RequireFloat(weight, "base", weightContext), modifiers));
        }
    }
}
