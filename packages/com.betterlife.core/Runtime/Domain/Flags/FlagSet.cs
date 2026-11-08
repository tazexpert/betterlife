using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Time;

namespace BetterLife.Core.Domain.Flags
{
    /// <summary>
    /// Story flags of one character (the memory behind the butterfly effect). Only declared flags can be used.
    /// </summary>
    public sealed class FlagSet
    {
        private const int SetValue = 1;

        private readonly DefinitionCatalog<FlagDefinition> _catalog;
        private readonly Dictionary<string, FlagEntry> _entries = new Dictionary<string, FlagEntry>(StringComparer.Ordinal);

        /// <summary>
        /// Creates an empty flag set.
        /// </summary>
        /// <param name="catalog">Flag declarations.</param>
        public FlagSet(DefinitionCatalog<FlagDefinition> catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        /// <summary>All active entries.</summary>
        public IReadOnlyCollection<FlagEntry> Entries => _entries.Values;

        /// <summary>
        /// Sets a bool flag. Re-setting refreshes its date and expiry.
        /// </summary>
        /// <param name="flagId">Declared bool flag id.</param>
        /// <param name="date">Current week.</param>
        /// <param name="expiryWeeks">Overrides the declared expiry; null uses the declaration.</param>
        public void Set(string flagId, GameDate date, int? expiryWeeks = null)
        {
            FlagDefinition definition = Require(flagId, FlagKind.Bool);
            _entries[flagId] = new FlagEntry(flagId, SetValue, date, ComputeExpiry(definition, date, expiryWeeks));
        }

        /// <summary>
        /// Increments a counter flag and refreshes its expiry.
        /// </summary>
        /// <param name="flagId">Declared counter flag id.</param>
        /// <param name="date">Current week.</param>
        /// <param name="amount">Positive amount.</param>
        /// <param name="expiryWeeks">Overrides the declared expiry; null uses the declaration.</param>
        /// <returns>The new count.</returns>
        public int Increment(string flagId, GameDate date, int amount = 1, int? expiryWeeks = null)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be positive.");
            }

            FlagDefinition definition = Require(flagId, FlagKind.Counter);
            int count = checked(GetCount(flagId) + amount);
            _entries[flagId] = new FlagEntry(flagId, count, date, ComputeExpiry(definition, date, expiryWeeks));
            return count;
        }

        /// <summary>
        /// Clears a flag of either kind.
        /// </summary>
        /// <param name="flagId">Declared flag id.</param>
        /// <returns>True when it was active.</returns>
        public bool Clear(string flagId)
        {
            _catalog.Get(flagId);
            return _entries.Remove(flagId);
        }

        /// <summary>
        /// Returns true when the flag is active (bool set, or counter above zero).
        /// </summary>
        /// <param name="flagId">Declared flag id.</param>
        /// <returns>True when active.</returns>
        public bool IsSet(string flagId)
        {
            _catalog.Get(flagId);
            return _entries.ContainsKey(flagId);
        }

        /// <summary>
        /// Returns the count of a flag (1 for a set bool flag, 0 when inactive).
        /// </summary>
        /// <param name="flagId">Declared flag id.</param>
        /// <returns>The count.</returns>
        public int GetCount(string flagId)
        {
            _catalog.Get(flagId);
            return _entries.TryGetValue(flagId, out FlagEntry entry) ? entry.Value : 0;
        }

        /// <summary>
        /// Returns the entry of an active flag, or null.
        /// </summary>
        /// <param name="flagId">Flag id.</param>
        /// <returns>Entry or null.</returns>
        public FlagEntry Find(string flagId)
        {
            return flagId != null && _entries.TryGetValue(flagId, out FlagEntry entry) ? entry : null;
        }

        /// <summary>
        /// Removes every flag whose expiry week has been reached.
        /// </summary>
        /// <param name="now">Current week.</param>
        /// <returns>Ids of removed flags.</returns>
        public IReadOnlyList<string> RemoveExpired(GameDate now)
        {
            List<string> expired = _entries.Values
                .Where(e => e.ExpiresOn.HasValue && e.ExpiresOn.Value <= now)
                .Select(e => e.FlagId)
                .ToList();
            foreach (string id in expired)
            {
                _entries.Remove(id);
            }
            return expired;
        }

        /// <summary>
        /// Restores a saved entry. Unknown ids and non-positive values are rejected.
        /// </summary>
        /// <param name="entry">Saved entry.</param>
        public void Restore(FlagEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }
            if (entry.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(entry), entry.Value, "Saved flag value must be positive.");
            }
            _catalog.Get(entry.FlagId);
            _entries[entry.FlagId] = entry;
        }

        private FlagDefinition Require(string flagId, FlagKind kind)
        {
            FlagDefinition definition = _catalog.Get(flagId);
            if (definition.Kind != kind)
            {
                throw new InvalidOperationException($"Flag '{flagId}' is {definition.Kind}, not {kind}.");
            }
            return definition;
        }

        private static GameDate? ComputeExpiry(FlagDefinition definition, GameDate date, int? overrideWeeks)
        {
            if (overrideWeeks.HasValue && overrideWeeks.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(overrideWeeks), overrideWeeks, "Expiry must be positive.");
            }

            int? weeks = overrideWeeks ?? definition.DefaultExpiryWeeks;
            return weeks.HasValue ? date.AddWeeks(weeks.Value) : (GameDate?)null;
        }
    }
}
