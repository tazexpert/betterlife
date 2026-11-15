using System;
using System.Text.RegularExpressions;

namespace BetterLife.Core.Domain
{
    /// <summary>
    /// Validation rules shared by every content identifier (lowercase, dot-separated segments).
    /// </summary>
    public static class ContentId
    {
        /// <summary>Regex every content id must match, for example "health.fitness".</summary>
        public const string Pattern = @"^[a-z][a-z0-9_]*(\.[a-z][a-z0-9_]*)*$";

        private static readonly Regex IdRegex = new Regex(Pattern, RegexOptions.CultureInvariant);

        /// <summary>
        /// Returns true when <paramref name="id"/> is a well-formed content id.
        /// </summary>
        /// <param name="id">Candidate id.</param>
        /// <returns>True when valid.</returns>
        public static bool IsValid(string id)
        {
            return !string.IsNullOrEmpty(id) && IdRegex.IsMatch(id);
        }

        /// <summary>
        /// Throws when <paramref name="id"/> is not a well-formed content id.
        /// </summary>
        /// <param name="id">Candidate id.</param>
        /// <param name="paramName">Parameter name used in the exception.</param>
        /// <returns>The validated id.</returns>
        public static string Require(string id, string paramName)
        {
            if (!IsValid(id))
            {
                throw new ArgumentException($"'{id}' is not a valid content id (expected {Pattern}).", paramName);
            }
            return id;
        }

        /// <summary>
        /// Throws when a localization key is missing. Keys follow the content id format.
        /// </summary>
        /// <param name="key">Localization key.</param>
        /// <param name="paramName">Parameter name used in the exception.</param>
        /// <returns>The validated key.</returns>
        public static string RequireLocKey(string key, string paramName)
        {
            return Require(key, paramName);
        }
    }
}
