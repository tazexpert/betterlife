using System;
using System.Collections.Generic;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Rules.Effects
{
    /// <summary>
    /// Adds or removes cash. A payment larger than the cash on hand takes everything and reports the shortfall,
    /// which the Economy system turns into debt.
    /// </summary>
    public sealed class CashEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="amount">Fixed or ranged amount; positive receives, negative pays.</param>
        public CashEffect(DecimalRange amount)
        {
            Amount = amount;
        }

        /// <summary>Amount.</summary>
        public DecimalRange Amount { get; }

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
            return new EffectResult(lines, new StatChangeResult[0], shortfall);
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context)
        {
            if (Amount.IsFixed && Amount.Min == 0m)
            {
                return new RuleText[0];
            }
            if (Amount.Max <= 0m)
            {
                return new[] { RangeText.Preview(RuleTextKeys.EffectCashPay, new DecimalRange(-Amount.Max, -Amount.Min)) };
            }
            return new[] { RangeText.Preview(RuleTextKeys.EffectCashGain, Amount) };
        }
    }
}
