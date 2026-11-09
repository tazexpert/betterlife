using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Time;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// The character's habits: one state per vice they have met, plus the last week that may never be undone.
    /// </summary>
    public sealed class CharacterVices
    {
        private readonly Dictionary<string, ViceState> _states = new Dictionary<string, ViceState>(StringComparer.Ordinal);

        /// <summary>States by vice id.</summary>
        public IReadOnlyCollection<ViceState> States => _states.Values;

        /// <summary>Last week with an action that may not be undone (a bet), or null.</summary>
        public GameDate? LastIrreversibleWeek { get; private set; }

        /// <summary>
        /// Returns the state of a vice, creating an empty one on first use.
        /// </summary>
        /// <param name="viceId">Vice id.</param>
        /// <returns>The state.</returns>
        public ViceState Get(string viceId)
        {
            if (!_states.TryGetValue(viceId ?? throw new ArgumentNullException(nameof(viceId)), out ViceState state))
            {
                state = new ViceState(viceId);
                _states.Add(viceId, state);
            }
            return state;
        }

        /// <summary>
        /// Returns the state of a vice, or null when never met.
        /// </summary>
        /// <param name="viceId">Vice id.</param>
        /// <returns>The state.</returns>
        public ViceState Find(string viceId) => viceId != null && _states.TryGetValue(viceId, out ViceState state) ? state : null;

        /// <summary>
        /// Marks a week as containing an action that may never be undone.
        /// </summary>
        /// <param name="week">Week.</param>
        public void MarkIrreversible(GameDate week)
        {
            if (!LastIrreversibleWeek.HasValue || week > LastIrreversibleWeek.Value)
            {
                LastIrreversibleWeek = week;
            }
        }

        /// <summary>
        /// Restores saved state.
        /// </summary>
        /// <param name="states">States.</param>
        /// <param name="lastIrreversibleWeek">Last irreversible week, or null.</param>
        public void Restore(IEnumerable<ViceState> states, GameDate? lastIrreversibleWeek)
        {
            _states.Clear();
            foreach (ViceState state in states ?? Enumerable.Empty<ViceState>())
            {
                _states[state.ViceId] = state;
            }
            LastIrreversibleWeek = lastIrreversibleWeek;
        }

        /// <summary>
        /// Removes vices that no longer exist in content; returns one warning per removal.
        /// </summary>
        /// <param name="viceExists">True for known vices.</param>
        /// <returns>Warnings.</returns>
        public IReadOnlyList<string> DropUnknown(Func<string, bool> viceExists)
        {
            if (viceExists == null)
            {
                throw new ArgumentNullException(nameof(viceExists));
            }
            var removed = _states.Keys.Where(id => !viceExists(id)).ToList();
            foreach (string id in removed)
            {
                _states.Remove(id);
            }
            return removed.Select(id => $"Unknown vice '{id}' removed.").ToList();
        }
    }
}
