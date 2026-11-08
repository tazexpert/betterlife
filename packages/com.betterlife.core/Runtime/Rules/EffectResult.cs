using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Events;

namespace BetterLife.Core.Rules
{
    /// <summary>
    /// What an effect actually did: player-facing lines, stat changes, any unpaid amount, and messages for other systems.
    /// Whoever applies effects must publish <see cref="Messages"/> (see <see cref="PublishTo"/>).
    /// </summary>
    public sealed class EffectResult
    {
        /// <summary>A result with no visible change.</summary>
        public static readonly EffectResult Empty = new EffectResult(new RuleText[0], new StatChangeResult[0], 0m);

        /// <summary>
        /// Creates a result with messages.
        /// </summary>
        /// <param name="lines">Player-facing lines.</param>
        /// <param name="statChanges">Applied stat changes.</param>
        /// <param name="cashShortfall">Amount that could not be paid (non-negative).</param>
        /// <param name="messages">Messages to publish on the event bus.</param>
        public EffectResult(IReadOnlyList<RuleText> lines, IReadOnlyList<StatChangeResult> statChanges, decimal cashShortfall, IReadOnlyList<IGameEvent> messages)
            : this(lines, statChanges, cashShortfall)
        {
            Messages = messages ?? new IGameEvent[0];
        }

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

        /// <summary>Messages for other systems (for example scheduling a delayed event).</summary>
        public IReadOnlyList<IGameEvent> Messages { get; } = new IGameEvent[0];

        /// <summary>
        /// Creates a result carrying only a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The result.</returns>
        public static EffectResult FromMessage(IGameEvent message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            return new EffectResult(new RuleText[0], new StatChangeResult[0], 0m, new[] { message });
        }

        /// <summary>
        /// Publishes every message in order.
        /// </summary>
        /// <param name="bus">Event bus.</param>
        public void PublishTo(IEventBus bus)
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }
            foreach (IGameEvent message in Messages)
            {
                bus.Publish(message);
            }
        }

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
                list.Sum(r => r.CashShortfall),
                list.SelectMany(r => r.Messages).ToList());
        }
    }
}
