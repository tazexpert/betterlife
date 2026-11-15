using BetterLife.Core.Domain.Skills;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/skills.json into a skill catalog.
    /// </summary>
    public sealed class SkillsLoader : DefinitionLoader<SkillDefinition>
    {
        /// <inheritdoc />
        public override string TypeName => ContentTypes.Skills;

        /// <inheritdoc />
        protected override string ItemsProperty => "skills";

        /// <inheritdoc />
        protected override SkillDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "category", "gainFalloffExponent", "decay");
            var (decayToken, decayContext) = RuleJson.RequireProperty(item, "decay", context);
            JObject decay = RuleJson.RequireObject(decayToken, decayContext);
            RuleJson.RequireOnly(decay, decayContext, "graceWeeks", "ratePerWeek", "retentionOfPeak");
            return new SkillDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                RuleJson.RequireId(item, "category", context),
                RuleJson.RequireFloat(item, "gainFalloffExponent", context),
                new SkillDecayRule(
                    RuleJson.RequireInt(decay, "graceWeeks", decayContext),
                    RuleJson.RequireFloat(decay, "ratePerWeek", decayContext),
                    RuleJson.RequireFloat(decay, "retentionOfPeak", decayContext)));
        }
    }
}
