using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Saving
{
    /// <summary>
    /// Save storage in memory (tests and the editor).
    /// </summary>
    public sealed class InMemorySaveStorage : ISaveStorage
    {
        private readonly Dictionary<string, string> _slots = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <inheritdoc />
        public bool Exists(string slot) => _slots.ContainsKey(SlotNames.Require(slot));

        /// <inheritdoc />
        public string Read(string slot) => _slots.TryGetValue(SlotNames.Require(slot), out string text) ? text : null;

        /// <inheritdoc />
        public void Write(string slot, string text) => _slots[SlotNames.Require(slot)] = text ?? throw new ArgumentNullException(nameof(text));

        /// <inheritdoc />
        public bool Delete(string slot) => _slots.Remove(SlotNames.Require(slot));

        /// <inheritdoc />
        public IReadOnlyList<string> Slots() => _slots.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();
    }
}
