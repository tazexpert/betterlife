using System;
using System.Globalization;
using System.Text.RegularExpressions;
using BetterLife.Core.Randomness;

namespace BetterLife.Core.Domain.Characters
{
    /// <summary>
    /// Unique character identifier such as "chr_0123456789abcdef". Generated from a seeded stream so it is reproducible.
    /// </summary>
    public readonly struct CharacterId : IEquatable<CharacterId>
    {
        /// <summary>Prefix of every character id.</summary>
        public const string Prefix = "chr_";

        private const int HalfShift = 32;
        private static readonly Regex Format = new Regex("^chr_[0-9a-f]{16}$", RegexOptions.CultureInvariant);

        private readonly string _value;

        /// <summary>
        /// Wraps an existing id after validating its format.
        /// </summary>
        /// <param name="value">Id text.</param>
        public CharacterId(string value)
        {
            if (value == null || !Format.IsMatch(value))
            {
                throw new ArgumentException($"'{value}' is not a valid character id.", nameof(value));
            }
            _value = value;
        }

        /// <summary>The id text.</summary>
        public string Value => _value ?? string.Empty;

        /// <summary>
        /// Generates a new id from a random stream.
        /// </summary>
        /// <param name="random">Seeded stream.</param>
        /// <returns>The new id.</returns>
        public static CharacterId Create(IRandomProvider random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            ulong bits = ((ulong)random.NextUInt() << HalfShift) | random.NextUInt();
            return new CharacterId(Prefix + bits.ToString("x16", CultureInfo.InvariantCulture));
        }

        /// <summary>Value equality.</summary>
        /// <param name="other">Id to compare.</param>
        /// <returns>True when equal.</returns>
        public bool Equals(CharacterId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        /// <summary>Value equality.</summary>
        /// <param name="obj">Object to compare.</param>
        /// <returns>True when equal.</returns>
        public override bool Equals(object obj) => obj is CharacterId other && Equals(other);

        /// <summary>Ordinal hash.</summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

        /// <summary>The id text.</summary>
        /// <returns>The id.</returns>
        public override string ToString() => Value;

        /// <summary>Equality operator.</summary>
        /// <param name="left">Left id.</param>
        /// <param name="right">Right id.</param>
        /// <returns>True when equal.</returns>
        public static bool operator ==(CharacterId left, CharacterId right) => left.Equals(right);

        /// <summary>Inequality operator.</summary>
        /// <param name="left">Left id.</param>
        /// <param name="right">Right id.</param>
        /// <returns>True when not equal.</returns>
        public static bool operator !=(CharacterId left, CharacterId right) => !left.Equals(right);
    }
}
