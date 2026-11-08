using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Content
{
    /// <summary>
    /// Thrown by <see cref="ContentLoadResult.RequireRepository"/> when content failed to load; lists every error.
    /// </summary>
    public sealed class ContentLoadException : Exception
    {
        /// <summary>
        /// Creates the exception.
        /// </summary>
        /// <param name="errors">All errors found.</param>
        public ContentLoadException(IReadOnlyList<ContentError> errors)
            : base("Content failed to load:" + Environment.NewLine + string.Join(Environment.NewLine, (errors ?? new ContentError[0]).Select(e => "  " + e)))
        {
            Errors = errors ?? new ContentError[0];
        }

        /// <summary>All errors found.</summary>
        public IReadOnlyList<ContentError> Errors { get; }
    }
}
