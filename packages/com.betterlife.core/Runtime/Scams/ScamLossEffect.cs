using System;
using System.Collections.Generic;
using BetterLife.Core.Economy;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Scams
{
    /// <summary>
    /// The character is scammed out of part of their cash (used by decision events such as a Ponzi pitch).
    /// Records an incident that can be reported.
    /// </summary>
    public sealed class ScamLossEffect : IEffect
    {
        /// <summary>Incidents kept when the effect records one.</summary>
        public const int IncidentsKept = 20;

        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="scamId">Scam id.</param>
        /// <param name="cashShare">Share of cash taken, 0-1.</param>
        /// <param name="min">Smallest loss (limited to the cash on hand).</param>
        /// <param name="max">Largest loss.</param>
        public ScamLossEffect(string scamId, decimal cashShare, decimal min, decimal max)
        {
            if (cashShare < 0m || cashShare > 1m || min < 0m || max < min)
            {
                throw new ArgumentOutOfRangeException(nameof(cashShare), "Share or range out of range.");
            }
            ScamId = scamId ?? throw new ArgumentNullException(nameof(scamId));
            CashShare = cashShare;
            Min = min;
            Max = max;
        }

        /// <summary>Scam id.</summary>
        public string ScamId { get; }

        /// <summary>Share of cash taken.</summary>
        public decimal CashShare { get; }

        /// <summary>Smallest loss.</summary>
        public decimal Min { get; }

        /// <summary>Largest loss.</summary>
        public decimal Max { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            decimal cash = context.Character.Wallet.Cash;
            decimal lost = Money.Round(Math.Min(cash, Math.Min(Max, Math.Max(Min, cash * CashShare))));
            if (lost > 0m)
            {
                context.Character.Wallet.WithdrawUpTo(lost);
            }
            ScamIncident incident = context.Character.Scams.AddIncident(ScamId, context.Now, lost, IncidentsKept);
            context.Character.Scams.RecordAttempt(false);
            return EffectResult.FromMessage(new ScammedEvent(incident, context.Now));
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context) => new RuleText[0];
    }
}
