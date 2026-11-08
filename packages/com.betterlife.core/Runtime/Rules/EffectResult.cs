using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Rules
{
    /// <summary>
    /// What an effect actually did: player-facing lines, stat changes for publishing, and any unpaid amount.
    /// </summary>
    public sealed class EffectResult
    {
        /// <summary>A result with no visible change.</summary>
        public static readonly EffectResult Empty = new EffectResult(new RuleText[0], new StatChangeResult[0], 0m);

        /// <summary>
        /// Creates a result.
        /// </summary>
        /// <param name="lines">Player-facing lines.</param>
        /// <param name="statChanges">Applied stat changes.</param>
        /// <param name="cashShortfall">Amount that could not be paid (non-negative).</param>
        public EffectResult(IReadOnlyList<RuleText> lines, IReadOnlyList<StatChangeResult> statChanges, decimal cashShortfall)
        {
            if (cashShortfall < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(cashShortfall), cashShortfall, "Shortfall must be non-negative.");
            }
            Lines = lines ?? new RuleText[0];
            StatChanges = statChanges ?? new StatChangeResult[0];
            CashShortfall = cashShortfall;
        }

        /// <summary>Player-facing lines.</summary>
        public IReadOnlyList<RuleText> Lines { get; }

        /// <summary>Applied stat changes.</summary>
        public IReadOnlyList<StatChangeResult> StatChanges { get; }

        /// <summary>Amount that could not be paid; the Economy system turns it into debt.</summary>
        public decimal CashShortfall { get; }

        /// <summary>
        /// Creates a result with only text.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns>The result.</returns>
        public static EffectResult FromLine(RuleText line)
        {
            return new EffectResult(new[] { line }, new StatChangeResult[0], 0m);
        }

        /// <summary>
        /// Combines several results in order.
        /// </summary>
        /// <param name="results">Results to combine.</param>
        /// <returns>The combined result.</returns>
        public static EffectResult Combine(IEnumerable<EffectResult> results)
        {
            List<EffectResult> list = (results ?? Enumerable.Empty<EffectResult>()).Where(r => r != null).ToList();
            return new EffectResult(
                list.SelectMany(r => r.Lines).ToList(),
                list.SelectMany(r => r.StatChanges).ToList(),
                list.Sum(r => r.CashShortfall));
        }
    }
}
