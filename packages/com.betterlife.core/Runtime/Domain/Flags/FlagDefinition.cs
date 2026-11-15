using System;

namespace BetterLife.Core.Domain.Flags
{
    /// <summary>
    /// Immutable declaration of a story flag, loaded from content/flags.json.
    /// </summary>
    public sealed class FlagDefinition : IDefinition
    {
        /// <summary>
        /// Creates a flag definition.
        /// </summary>
        /// <param name="id">Flag id, for example "career.fired_once".</param>
        /// <param name="kind">Bool or counter.</param>
        /// <param name="defaultExpiryWeeks">Weeks until the flag clears itself, or null for permanent.</param>
        /// <param name="inheritable">True when heirs inherit it in Legacy mode.</param>
        /// <param name="eulogyKey">Localization key used in the eulogy, or null.</param>
        public FlagDefinition(string id, FlagKind kind, int? defaultExpiryWeeks, bool inheritable, string eulogyKey)
        {
            if (defaultExpiryWeeks.HasValue && defaultExpiryWeeks.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(defaultExpiryWeeks), defaultExpiryWeeks, "Expiry must be positive.");
            }

            Id = ContentId.Require(id, nameof(id));
            Kind = kind;
            DefaultExpiryWeeks = defaultExpiryWeeks;
            Inheritable = inheritable;
            EulogyKey = eulogyKey == null ? null : ContentId.RequireLocKey(eulogyKey, nameof(eulogyKey));
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Bool or counter.</summary>
        public FlagKind Kind { get; }

        /// <summary>Weeks until the flag clears itself, or null for permanent.</summary>
        public int? DefaultExpiryWeeks { get; }

        /// <summary>True when heirs inherit it.</summary>
        public bool Inheritable { get; }

        /// <summary>Eulogy localization key, or null.</summary>
        public string EulogyKey { get; }
    }
}
