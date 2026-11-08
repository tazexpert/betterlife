using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Rules.Effects
{
    /// <summary>
    /// Practices a skill (diminishing returns apply).
    /// </summary>
    public sealed class PracticeSkillEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="skillId">Skill id.</param>
        /// <param name="gain">Fixed or ranged base gain; non-negative.</param>
        public PracticeSkillEffect(string skillId, FloatRange gain)
        {
            if (gain.Min < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(gain), gain.Min, "Gain must be non-negative.");
            }
            SkillId = ContentId.Require(skillId, nameof(skillId));
            Gain = gain;
        }

        /// <summary>Skill id.</summary>
        public string SkillId { get; }

        /// <summary>Base gain.</summary>
        public FloatRange Gain { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            float applied = context.Character.Skills.Practice(SkillId, Gain.Roll(context.Random), context.Now);
            if (applied == 0f)
            {
                return EffectResult.Empty;
            }
            string nameKey = context.Character.Catalogs.Skills.Get(SkillId).NameKey;
            return EffectResult.FromLine(RuleText.Of(RuleTextKeys.EffectSkill, (RuleTextKeys.ArgSubjectKey, nameKey), (RuleTextKeys.ArgValue, RuleText.Number(applied))));
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            string nameKey = context.Character.Catalogs.Skills.Get(SkillId).NameKey;
            return new[] { RangeText.Preview(RuleTextKeys.EffectSkill, Gain, (RuleTextKeys.ArgSubjectKey, nameKey)) };
        }
    }
}
