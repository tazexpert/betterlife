using System.Collections.Generic;

namespace BetterLife.Core.Rules
{
    /// <summary>
    /// Result of explaining a condition: whether it passed, its description and the traces of its children.
    /// Used to show the player why a choice is locked.
    /// </summary>
    public sealed class ConditionTrace
    {
        /// <summary>
        /// Creates a trace.
        /// </summary>
        /// <param name="passed">Whether the condition passed.</param>
        /// <param name="description">Player-facing description.</param>
        /// <param name="isVisibleToPlayer">False for hidden requirements (flags, hidden stats and traits).</param>
        /// <param name="children">Child traces for composite conditions.</param>
        public ConditionTrace(bool passed, RuleText description, bool isVisibleToPlayer, IReadOnlyList<ConditionTrace> children = null)
        {
            Passed = passed;
            Description = description;
            IsVisibleToPlayer = isVisibleToPlayer;
            Children = children ?? new List<ConditionTrace>();
        }

        /// <summary>Whether the condition passed.</summary>
        public bool Passed { get; }

        /// <summary>Player-facing description.</summary>
        public RuleText Description { get; }

        /// <summary>False for hidden requirements.</summary>
        public bool IsVisibleToPlayer { get; }

        /// <summary>Child traces.</summary>
        public IReadOnlyList<ConditionTrace> Children { get; }
    }
}
