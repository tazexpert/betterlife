using System;

namespace BetterLife.Core.Saving
{
    /// <summary>
    /// A save could not be read.
    /// </summary>
    public sealed class SaveLoadException : Exception
    {
        /// <summary>
        /// Creates the exception.
        /// </summary>
        /// <param name="kind">Why.</param>
        /// <param name="message">Details for logs.</param>
        /// <param name="inner">Underlying error, or null.</param>
        public SaveLoadException(SaveErrorKind kind, string message, Exception inner = null)
            : base(message, inner)
        {
            Kind = kind;
        }

        /// <summary>Why the save could not be read.</summary>
        public SaveErrorKind Kind { get; }
    }
}
