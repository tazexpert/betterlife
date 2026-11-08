using System.Collections.Generic;

namespace BetterLife.Core.Content
{
    /// <summary>
    /// Outcome of loading content: the repository when everything is valid, otherwise all errors.
    /// </summary>
    public sealed class ContentLoadResult
    {
        /// <summary>
        /// Creates a result.
        /// </summary>
        /// <param name="repository">The repository, or null when there are errors.</param>
        /// <param name="errors">All errors.</param>
        public ContentLoadResult(IContentRepository repository, IReadOnlyList<ContentError> errors)
        {
            Repository = repository;
            Errors = errors ?? new ContentError[0];
        }

        /// <summary>The repository, or null when there are errors.</summary>
        public IContentRepository Repository { get; }

        /// <summary>All errors.</summary>
        public IReadOnlyList<ContentError> Errors { get; }

        /// <summary>True when content loaded without errors.</summary>
        public bool Succeeded => Repository != null && Errors.Count == 0;

        /// <summary>
        /// Returns the repository or throws a <see cref="ContentLoadException"/> listing every error.
        /// </summary>
        /// <returns>The repository.</returns>
        public IContentRepository RequireRepository()
        {
            if (!Succeeded)
            {
                throw new ContentLoadException(Errors);
            }
            return Repository;
        }
    }
}
