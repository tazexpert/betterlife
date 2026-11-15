using System;

namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// Type-safe identifier of a stat, for example "health.fitness".
    /// </summary>
    public readonly struct StatId : IEquatable<StatId>
    {
        private readonly string _value;

        /// <summary>
        /// Creates a stat id after validating its format.
        /// </summary>
        /// <param name="value">Content id of the stat.</param>
        public StatId(string value)
        {
            _value = ContentId.Require(value, nameof(value));
        }

        /// <summary>The underlying content id.</summary>
        public string Value => _value ?? string.Empty;

        /// <summary>Value equality.</summary>
        /// <param name="other">Id to compare.</param>
        /// <returns>True when equal.</returns>
        public bool Equals(StatId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        /// <summary>Value equality.</summary>
        /// <param name="obj">Object to compare.</param>
        /// <returns>True when equal.</returns>
        public override bool Equals(object obj) => obj is StatId other && Equals(other);

        /// <summary>Ordinal hash of the id.</summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

        /// <summary>The id text.</summary>
        /// <returns>The id.</returns>
        public override string ToString() => Value;

        /// <summary>Equality operator.</summary>
        /// <param name="left">Left id.</param>
        /// <param name="right">Right id.</param>
        /// <returns>True when equal.</returns>
        public static bool operator ==(StatId left, StatId right) => left.Equals(right);

        /// <summary>Inequality operator.</summary>
        /// <param name="left">Left id.</param>
        /// <param name="right">Right id.</param>
        /// <returns>True when not equal.</returns>
        public static bool operator !=(StatId left, StatId right) => !left.Equals(right);
    }
}
