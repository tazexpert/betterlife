using System.Collections.Generic;
using BetterLife.Core.Karma;
using BetterLife.Core.Luck;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/luck.json into <see cref="LuckSettings"/>, <see cref="KarmaSettings"/> and <see cref="KarmaService"/>,
    /// and registers the "karma.bad" and "karma.merit" effects.
    /// </summary>
    public sealed class LuckSettingsLoader : SingleDocumentLoader
    {
        private static readonly string[] Dependencies = { ContentTypes.Astrology };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.LuckSettings;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override void Read(JObject root, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(root, context, "chance", "fortune", "karma", ContentReading.SchemaProperty);
            var (chance, chanceContext) = Section(root, "chance", context, "min", "max", "itemBonusCap");
            var (fortune, fortuneContext) = Section(root, "fortune", context, "neutral", "pointsPerFortune");
            var (karma, karmaContext) = Section(root, "karma", context, "goodWeight", "badWeight", "cap", "debtRatio", "debtDelayWeeks", "meritOffsetRatio");
            var (delay, delayContext) = Section(karma, "debtDelayWeeks", karmaContext, "min", "max");

            var luck = new LuckSettings(
                RuleJson.RequireFloat(chance, "min", chanceContext),
                RuleJson.RequireFloat(chance, "max", chanceContext),
                RuleJson.RequireFloat(chance, "itemBonusCap", chanceContext),
                RuleJson.RequireFloat(fortune, "neutral", fortuneContext),
                RuleJson.RequireFloat(fortune, "pointsPerFortune", fortuneContext));
            var karmaSettings = new KarmaSettings(
                RuleJson.RequireFloat(karma, "goodWeight", karmaContext),
                RuleJson.RequireFloat(karma, "badWeight", karmaContext),
                RuleJson.RequireFloat(karma, "cap", karmaContext),
                RuleJson.RequireFloat(karma, "debtRatio", karmaContext),
                RuleJson.RequireInt(delay, "min", delayContext),
                RuleJson.RequireInt(delay, "max", delayContext),
                RuleJson.RequireFloat(karma, "meritOffsetRatio", karmaContext));
            var service = new KarmaService(karmaSettings);

            content.Add(luck);
            content.Add(karmaSettings);
            content.Add(service);
            content.Rules.Register(new BadKarmaEffectConverter(service));
            content.Rules.Register(new MeritEffectConverter(service));
        }

        private static (JObject Section, RuleReadContext Context) Section(JObject parent, string name, RuleReadContext context, params string[] allowed)
        {
            var (token, sectionContext) = RuleJson.RequireProperty(parent, name, context);
            JObject section = RuleJson.RequireObject(token, sectionContext);
            RuleJson.RequireOnly(section, sectionContext, allowed);
            return (section, sectionContext);
        }
    }
}
