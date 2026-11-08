namespace BetterLife.Core.Rules
{
    /// <summary>
    /// Localization keys and argument names used by built-in conditions and effects.
    /// </summary>
    public static class RuleTextKeys
    {
        /// <summary>Argument: a localization key of the subject name (stat, trait, skill...).</summary>
        public const string ArgSubjectKey = "subjectKey";
        /// <summary>Argument: subject id when no localized name exists (domain, condition).</summary>
        public const string ArgSubject = "subject";
        /// <summary>Argument: comparison symbol.</summary>
        public const string ArgOp = "op";
        /// <summary>Argument: comparison value or fixed amount.</summary>
        public const string ArgValue = "value";
        /// <summary>Argument: range minimum.</summary>
        public const string ArgMin = "min";
        /// <summary>Argument: range maximum.</summary>
        public const string ArgMax = "max";

        /// <summary>All children must pass.</summary>
        public const string CondAll = "rule.cond.all";
        /// <summary>At least one child must pass.</summary>
        public const string CondAny = "rule.cond.any";
        /// <summary>The child must fail.</summary>
        public const string CondNot = "rule.cond.not";
        /// <summary>Stat comparison.</summary>
        public const string CondStat = "rule.cond.stat";
        /// <summary>Flag is set.</summary>
        public const string CondFlagSet = "rule.cond.flag_set";
        /// <summary>Flag count comparison.</summary>
        public const string CondFlagCount = "rule.cond.flag_count";
        /// <summary>Has trait.</summary>
        public const string CondTrait = "rule.cond.trait";
        /// <summary>Skill comparison.</summary>
        public const string CondSkill = "rule.cond.skill";
        /// <summary>Age comparison.</summary>
        public const string CondAge = "rule.cond.age";
        /// <summary>Year comparison.</summary>
        public const string CondYear = "rule.cond.year";
        /// <summary>Cash comparison.</summary>
        public const string CondCash = "rule.cond.cash";
        /// <summary>Net worth comparison.</summary>
        public const string CondNetWorth = "rule.cond.net_worth";
        /// <summary>Total debt comparison.</summary>
        public const string CondTotalDebt = "rule.cond.total_debt";
        /// <summary>Total debt of one kind comparison.</summary>
        public const string CondTotalDebtKind = "rule.cond.total_debt_kind";
        /// <summary>Reputation comparison.</summary>
        public const string CondReputation = "rule.cond.reputation";
        /// <summary>Has health condition.</summary>
        public const string CondHealthCondition = "rule.cond.health_condition";

        /// <summary>Stat change (fixed or range).</summary>
        public const string EffectStat = "rule.effect.stat";
        /// <summary>Cash received.</summary>
        public const string EffectCashGain = "rule.effect.cash_gain";
        /// <summary>Cash paid.</summary>
        public const string EffectCashPay = "rule.effect.cash_pay";
        /// <summary>Payment could not be covered in full.</summary>
        public const string EffectCashShortfall = "rule.effect.cash_shortfall";
        /// <summary>Trait gained (visible traits only).</summary>
        public const string EffectTraitAdd = "rule.effect.trait_add";
        /// <summary>Trait lost (visible traits only).</summary>
        public const string EffectTraitRemove = "rule.effect.trait_remove";
        /// <summary>Hidden trait discovered.</summary>
        public const string EffectTraitReveal = "rule.effect.trait_reveal";
        /// <summary>Skill practiced.</summary>
        public const string EffectSkill = "rule.effect.skill";
        /// <summary>Health condition gained.</summary>
        public const string EffectHealthConditionAdd = "rule.effect.health_condition_add";
        /// <summary>Health condition cured.</summary>
        public const string EffectHealthConditionRemove = "rule.effect.health_condition_remove";
    }
}
