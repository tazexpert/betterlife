using System;
using System.Collections.Generic;
using System.Globalization;

namespace BetterLife.Core.Rules.Effects
{
    /// <summary>
    /// Takes a share of the cash on hand ("-20% savings"). It never creates debt: a character with no cash loses nothing.
    /// </summary>
    public sealed class CashShareEffect : IEffect
    {
        /// <summary>Preview text (placeholder <c>{value}</c> = share in percent).</summary>
        public const string PreviewKey = "rule.effect.cash_share";

        private const decimal Hundred = 100m;

        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="share">Share of the cash lost, within (0, 1].</param>
        public CashShareEffect(decimal share)
        {
            if (!(share > 0m) || share > 1m)
            {
                throw new ArgumentOutOfRangeException(nameof(share), share, "Share must be within (0, 1].");
            }
            Share = share;
        }

        /// <summary>Share of the cash lost, within (0, 1].</summary>
        public decimal Share { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            decimal cash = context.Character.Wallet.Cash;
            if (cash <= 0m)
            {
                return EffectResult.Empty;
            }
            decimal paid = context.Character.Wallet.WithdrawUpTo(Math.Round(cash * Share, 2));
            if (paid <= 0m)
            {
                return EffectResult.Empty;
            }
            return EffectResult.FromLine(RuleText.Of(RuleTextKeys.EffectCashPay, (RuleTextKeys.ArgValue, RuleText.Number(paid))));
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context)
        {
            return new[] { RuleText.Of(PreviewKey, (RuleTextKeys.ArgValue, (Share * Hundred).ToString("0", CultureInfo.InvariantCulture))) };
        }
    }
}
