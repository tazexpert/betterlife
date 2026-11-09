using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Luck;
using BetterLife.Core.Rules.Serialization;
using BetterLife.Core.Scams;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/scams.json into a catalog of <see cref="ScamDefinition"/> and registers the "scam.loss" effect
    /// (scam ids are collected first so events can refer to them).
    /// </summary>
    public sealed class ScamsLoader : DefinitionLoader<ScamDefinition>
    {
        private static readonly string[] Dependencies = { ContentTypes.RuleTypes };

        private static readonly IReadOnlyDictionary<string, ScamChannel> Channels = new Dictionary<string, ScamChannel>
        {
            ["phone"] = ScamChannel.Phone,
            ["sms"] = ScamChannel.Sms,
            ["online"] = ScamChannel.Online,
            ["romance"] = ScamChannel.Romance,
            ["in_person"] = ScamChannel.InPerson,
            ["game"] = ScamChannel.Game,
        };

        private readonly HashSet<string> _known = new HashSet<string>(StringComparer.Ordinal);

        /// <inheritdoc />
        public override string TypeName => ContentTypes.Scams;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override string ItemsProperty => "scams";

        /// <inheritdoc />
        protected override void BeforeItems(IReadOnlyList<ContentDocument> documents, ContentLoadContext context)
        {
            _known.Clear();
            foreach (JObject item in documents.Select(d => d.Root[ItemsProperty]).OfType<JArray>().SelectMany(a => a.OfType<JObject>()))
            {
                if (item["id"]?.Type == JTokenType.String)
                {
                    _known.Add((string)item["id"]);
                }
            }
            if (!context.Rules.EffectTypes.Contains(ScamLossEffectConverter.TypeName))
            {
                context.Rules.Register(new ScamLossEffectConverter(_known));
            }
        }

        /// <inheritdoc />
        protected override ScamDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "channel", "minAge", "monthlyChance", "ageBands", "lonelinessWeight", "wealthWeight",
                "modifiers", "detection", "loss", "effects", "recovery");
            var (bands, bandsContext) = RuleJson.RequireArrayProperty(item, "ageBands", context);
            var (modifiers, modifiersContext) = RuleJson.RequireArrayProperty(item, "modifiers", context);
            var (loss, lossContext) = SettingsReading.Section(item, "loss", context, "cashShare", "savingsShare", "min", "max");
            var (recovery, recoveryContext) = SettingsReading.Section(item, "recovery", context, "chance", "decayPerWeek", "windowWeeks", "share");
            return new ScamDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                ContentReading.RequireEnum(item, "channel", context, Channels),
                RuleJson.RequireInt(item, "minAge", context),
                RuleJson.RequireFloat(item, "monthlyChance", context),
                bands.Select((token, i) =>
                {
                    RuleReadContext c = bandsContext.Index(i);
                    JObject band = RuleJson.RequireObject(token, c);
                    RuleJson.RequireOnly(band, c, "minAge", "multiplier");
                    return (RuleJson.RequireInt(band, "minAge", c), RuleJson.RequireFloat(band, "multiplier", c));
                }).ToList(),
                RuleJson.RequireFloat(item, "lonelinessWeight", context),
                RuleJson.RequireFloat(item, "wealthWeight", context),
                modifiers.Select((token, i) =>
                {
                    RuleReadContext c = modifiersContext.Index(i);
                    JObject modifier = RuleJson.RequireObject(token, c);
                    RuleJson.RequireOnly(modifier, c, "condition", "multiplier");
                    return new ScamModifier(content.Rules.ParseCondition(modifier["condition"], c.Property("condition")), RuleJson.RequireFloat(modifier, "multiplier", c));
                }).ToList(),
                ChanceSpecReader.Read(item["detection"], context.Property("detection")),
                new ScamLoss(RuleJson.RequireDecimal(loss, "cashShare", lossContext), RuleJson.RequireDecimal(loss, "savingsShare", lossContext),
                    RuleJson.RequireDecimal(loss, "min", lossContext), RuleJson.RequireDecimal(loss, "max", lossContext)),
                content.Rules.ParseEffects(item["effects"], context.Property("effects")),
                new ScamRecovery(ChanceSpecReader.Read(recovery["chance"], recoveryContext.Property("chance")), RuleJson.RequireFloat(recovery, "decayPerWeek", recoveryContext),
                    RuleJson.RequireInt(recovery, "windowWeeks", recoveryContext), RuleJson.RequireDecimal(recovery, "share", recoveryContext)));
        }
    }
}
