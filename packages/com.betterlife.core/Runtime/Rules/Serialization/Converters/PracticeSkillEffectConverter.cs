using BetterLife.Core.Rules.Effects;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"skill.practice","skill":id,"gain":3 or {min,max}}.
    /// </summary>
    public sealed class PracticeSkillEffectConverter : EffectConverter<PracticeSkillEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "skill.practice";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override PracticeSkillEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "skill", "gain");
            return new PracticeSkillEffect(RuleJson.RequireSkill(json, "skill", context), RuleJson.RequireFloatRange(json, "gain", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(PracticeSkillEffect effect, JObject json, RuleRegistry registry)
        {
            json["skill"] = effect.SkillId;
            json["gain"] = RuleJson.Range(effect.Gain);
        }
    }
}
