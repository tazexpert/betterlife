using System;
using System.Collections.Generic;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Events;

namespace BetterLife.Core.Rules.Effects
{
    /// <summary>
    /// Adds or removes cash. A payment larger than the cash on hand takes everything and reports the shortfall,
    /// (also as a <see cref="CashShortfallEvent"/> message), which the Economy system turns into debt.
    /// A payment marked "family pays" (medical bills) is paid in full by the family while the character is a dependent.
    /// </summary>
    public sealed class CashEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="amount">Fixed or ranged amount; positive receives, negative pays.</param>
        /// <param name="familyPays">True when the family pays this cost for a dependent (medical bills).</param>
        public CashEffect(DecimalRange amount, bool familyPays = false)
        {
            Amount = amount;
            FamilyPays = familyPays;
        }

        /// <summary>Rule text when the family pays (placeholder <c>{value}</c>).</summary>
        public const string FamilyPaysKey = "rule.effect.cash_family_pays";

        /// <summary>Amount.</summary>
        public DecimalRange Amount { get; }

        /// <summary>True when the family pays this cost for a dependent (medical bills).</summary>
        public bool FamilyPays { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            decimal amount = Amount.Roll(context.Random);
            if (amount > 0m)
            {
                context.Character.Wallet.Deposit(amount);
                return EffectResult.FromLine(RuleText.Of(RuleTextKeys.EffectCashGain, (RuleTextKeys.ArgValue, RuleText.Number(amount))));
            }
            if (amount == 0m)
            {
                return EffectResult.Empty;
            }

            decimal requested = -amount;
            if (FamilyPays && context.Character.Finances.IsDependent)
            {
                return EffectResult.FromLine(RuleText.Of(FamilyPaysKey, (RuleTextKeys.ArgValue, RuleText.Number(requested))));
            }
            decimal paid = context.Character.Wallet.WithdrawUpTo(requested);
            decimal shortfall = requested - paid;
            var lines = new List<RuleText>();
            if (paid > 0m)
            {
                lines.Add(RuleText.Of(RuleTextKeys.EffectCashPay, (RuleTextKeys.ArgValue, RuleText.Number(paid))));
            }
            if (shortfall > 0m)
            {
                lines.Add(RuleText.Of(RuleTextKeys.EffectCashShortfall, (RuleTextKeys.ArgValue, RuleText.Number(shortfall))));
            }
            IGameEvent[] messages = shortfall > 0m
                ? new IGameEvent[] { new CashShortfallEvent(shortfall, context.SourceKey, context.Now) }
                : new IGameEvent[0];
            return new EffectResult(lines, new StatChangeResult[0], shortfall, messages);
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context)
        {
            if (Amount.IsFixed && Amount.Min == 0m)
            {
                return new RuleText[0];
            }
            if (Amount.Max <= 0m && FamilyPays && context?.Character.Finances.IsDependent == true)
            {
                return new[] { RangeText.Preview(FamilyPaysKey, new DecimalRange(-Amount.Max, -Amount.Min)) };
            }
            if (Amount.Max <= 0m)
            {
                return new[] { RangeText.Preview(RuleTextKeys.EffectCashPay, new DecimalRange(-Amount.Max, -Amount.Min)) };
            }
            return new[] { RangeText.Preview(RuleTextKeys.EffectCashGain, Amount) };
        }
    }
}
