using System;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Navigation
{
    /// <summary>
    /// A screen to show and an optional argument (for example an NPC id).
    /// </summary>
    public sealed class ScreenRequest : IEquatable<ScreenRequest>
    {
        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="screenId">Screen id.</param>
        /// <param name="argument">Argument, or null.</param>
        public ScreenRequest(string screenId, string argument = null)
        {
            ScreenId = ContentId.Require(screenId, nameof(screenId));
            Argument = argument;
        }

        /// <summary>Screen id.</summary>
        public string ScreenId { get; }

        /// <summary>Argument, or null.</summary>
        public string Argument { get; }

        /// <inheritdoc />
        public bool Equals(ScreenRequest other) => other != null && ScreenId == other.ScreenId && Argument == other.Argument;

        /// <inheritdoc />
        public override bool Equals(object obj) => Equals(obj as ScreenRequest);

        /// <inheritdoc />
        public override int GetHashCode() => (ScreenId.GetHashCode() * 397) ^ (Argument?.GetHashCode() ?? 0);

        /// <inheritdoc />
        public override string ToString() => Argument == null ? ScreenId : ScreenId + "(" + Argument + ")";
    }
}
