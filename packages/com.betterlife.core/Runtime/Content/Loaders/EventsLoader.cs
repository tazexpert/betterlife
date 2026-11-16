using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Luck;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/events/*.json. Event ids from every file are collected first so "event.schedule" can
    /// reference events defined in any file.
    /// </summary>
    public sealed class EventsLoader : DefinitionLoader<LifeEventDefinition>
    {
        private static readonly string[] Dependencies = { ContentTypes.RuleTypes, ContentTypes.DeathCauses, ContentTypes.Careers, ContentTypes.Vices, ContentTypes.Scams };

        private static readonly Dictionary<string, EventTrigger> Triggers = new Dictionary<string, EventTrigger>
        {
            { "random", EventTrigger.Random },
            { "milestone", EventTrigger.Milestone },
            { "scheduled", EventTrigger.Scheduled },
            { "karmic_debt", EventTrigger.KarmicDebt },
        };

        private static readonly Dictionary<string, EventTone> Tones = new Dictionary<string, EventTone>
        {
            { "negative", EventTone.Negative },
            { "neutral", EventTone.Neutral },
            { "positive", EventTone.Positive },
        };

        private const float DefaultBaseWeight = 1f;

        private readonly HashSet<string> _knownEvents = new HashSet<string>(StringComparer.Ordinal);

        /// <inheritdoc />
        public override string TypeName => ContentTypes.Events;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override string ItemsProperty => "events";

        /// <inheritdoc />
        protected override void BeforeItems(IReadOnlyList<ContentDocument> documents, ContentLoadContext context)
        {
            _knownEvents.Clear();
            foreach (ContentDocument document in documents)
            {
                if (document.Root[ItemsProperty] is JArray items)
                {
                    foreach (JObject item in items.OfType<JObject>())
                    {
                        if (item["id"]?.Type == JTokenType.String)
                        {
                            _knownEvents.Add((string)item["id"]);
                        }
                    }
                }
            }
            if (!context.Rules.EffectTypes.Contains(ScheduleEventEffectConverter.TypeName))
            {
                context.Rules.Register(new ScheduleEventEffectConverter(_knownEvents));
            }
        }

        /// <inheritdoc />
        protected override LifeEventDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "titleKey", "bodyKey", "category", "trigger", "severity", "tone", "storyline",
                "condition", "weight", "cooldownWeeks", "maxOccurrences", "debtAmount", "onFire", "defaultChoice", "choices", "milestone");
            RuleRegistry rules = content.Rules;
            var (choices, choicesContext) = RuleJson.RequireArrayProperty(item, "choices", context);
            return new LifeEventDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "titleKey", context),
                RuleJson.RequireId(item, "bodyKey", context),
                RuleJson.RequireId(item, "category", context),
                ContentReading.RequireEnum(item, "trigger", context, Triggers),
                ContentReading.RequireEnum(item, "severity", context, SeverityNames.All),
                ContentReading.RequireEnum(item, "tone", context, Tones),
                RuleJson.OptionalId(item, "storyline", context),
                OptionalCondition(item, "condition", context, rules),
                ReadWeight(item, context, rules),
                RuleJson.OptionalInt(item, "cooldownWeeks", context) ?? 0,
                RuleJson.OptionalInt(item, "maxOccurrences", context),
                item["debtAmount"] == null ? (FloatRange?)null : RuleJson.RequireFloatRange(item, "debtAmount", context),
                item["onFire"] == null ? EffectSequence.Empty : rules.ParseEffects(item["onFire"], context.Property("onFire")),
                choices.Select((token, i) => ReadChoice(token, choicesContext.Index(i), rules)).ToList(),
                RuleJson.OptionalId(item, "defaultChoice", context),
                item["milestone"] == null ? null : ReadMilestone(item, context, rules));
        }

        private static MilestoneChance ReadMilestone(JObject item, RuleReadContext context, RuleRegistry rules)
        {
            var (token, milestoneContext) = RuleJson.RequireProperty(item, "milestone", context);
            JObject milestone = RuleJson.RequireObject(token, milestoneContext);
            RuleJson.RequireOnly(milestone, milestoneContext, "minAge", "maxAge", "chance", "modifiers");
            var modifiers = new List<MilestoneModifier>();
            if (milestone["modifiers"] != null)
            {
                var (array, arrayContext) = RuleJson.RequireArrayProperty(milestone, "modifiers", milestoneContext);
                for (int i = 0; i < array.Count; i++)
                {
                    RuleReadContext modifierContext = arrayContext.Index(i);
                    JObject modifier = RuleJson.RequireObject(array[i], modifierContext);
                    RuleJson.RequireOnly(modifier, modifierContext, "condition", "add");
                    var (conditionToken, conditionContext) = RuleJson.RequireProperty(modifier, "condition", modifierContext);
                    modifiers.Add(new MilestoneModifier(rules.ParseCondition(conditionToken, conditionContext), RuleJson.RequireFloat(modifier, "add", modifierContext)));
                }
            }
            int minAge = RuleJson.RequireInt(milestone, "minAge", milestoneContext);
            int maxAge = RuleJson.RequireInt(milestone, "maxAge", milestoneContext);
            float chance = RuleJson.RequireFloat(milestone, "chance", milestoneContext);
            if (minAge < 0 || maxAge < minAge)
            {
                throw milestoneContext.Property("maxAge").Error("must be at least minAge, and ages non-negative.");
            }
            if (!(chance >= 0f) || chance > MilestoneChance.MaxPercent)
            {
                throw milestoneContext.Property("chance").Error("must be within 0-100.");
            }
            return new MilestoneChance(minAge, maxAge, chance, modifiers);
        }

        private static EventWeight ReadWeight(JObject item, RuleReadContext context, RuleRegistry rules)
        {
            if (item["weight"] == null)
            {
                return new EventWeight(DefaultBaseWeight, null);
            }
            var (token, weightContext) = RuleJson.RequireProperty(item, "weight", context);
            JObject weight = RuleJson.RequireObject(token, weightContext);
            RuleJson.RequireOnly(weight, weightContext, "base", "modifiers");
            var modifiers = new List<WeightModifier>();
            if (weight["modifiers"] != null)
            {
                var (array, arrayContext) = RuleJson.RequireArrayProperty(weight, "modifiers", weightContext);
                for (int i = 0; i < array.Count; i++)
                {
                    RuleReadContext modifierContext = arrayContext.Index(i);
                    JObject modifier = RuleJson.RequireObject(array[i], modifierContext);
                    RuleJson.RequireOnly(modifier, modifierContext, "condition", "multiplier");
                    var (conditionToken, conditionContext) = RuleJson.RequireProperty(modifier, "condition", modifierContext);
                    modifiers.Add(new WeightModifier(rules.ParseCondition(conditionToken, conditionContext), RuleJson.RequireFloat(modifier, "multiplier", modifierContext)));
                }
            }
            return new EventWeight(RuleJson.RequireFloat(weight, "base", weightContext), modifiers);
        }

        private static EventChoice ReadChoice(JToken token, RuleReadContext context, RuleRegistry rules)
        {
            JObject choice = RuleJson.RequireObject(token, context);
            RuleJson.RequireOnly(choice, context, "id", "textKey", "condition", "hideWhenLocked", "chance", "success", "failure");
            ChanceSpec chance = choice["chance"] == null ? null : ChanceSpecReader.Read(choice["chance"], context.Property("chance"));
            if (chance != null && choice["failure"] == null)
            {
                throw context.Property("failure").Error("is required when the choice has a chance.");
            }
            if (chance == null && choice["failure"] != null)
            {
                throw context.Property("failure").Error("is only allowed when the choice has a chance.");
            }
            return new EventChoice(
                RuleJson.RequireId(choice, "id", context),
                RuleJson.RequireId(choice, "textKey", context),
                OptionalCondition(choice, "condition", context, rules),
                RuleJson.OptionalBool(choice, "hideWhenLocked", context) ?? false,
                chance,
                ReadOutcome(choice, "success", context, rules),
                choice["failure"] == null ? null : ReadOutcome(choice, "failure", context, rules));
        }

        private static EventOutcome ReadOutcome(JObject parent, string name, RuleReadContext context, RuleRegistry rules)
        {
            var (token, outcomeContext) = RuleJson.RequireProperty(parent, name, context);
            JObject outcome = RuleJson.RequireObject(token, outcomeContext);
            RuleJson.RequireOnly(outcome, outcomeContext, "resultKey", "effects");
            EffectSequence effects = outcome["effects"] == null ? EffectSequence.Empty : rules.ParseEffects(outcome["effects"], outcomeContext.Property("effects"));
            return new EventOutcome(RuleJson.RequireId(outcome, "resultKey", outcomeContext), effects);
        }

        private static ICondition OptionalCondition(JObject parent, string name, RuleReadContext context, RuleRegistry rules)
        {
            return parent[name] == null ? null : rules.ParseCondition(parent[name], context.Property(name));
        }
    }
}
