using System;

namespace BetterLife.Core.Domain.Characters
{
    /// <summary>
    /// A character's name. Names are data (player-entered or generated), not UI text, so they are not localization keys.
    /// </summary>
    public sealed class PersonName
    {
        /// <summary>Maximum length of each name part.</summary>
        public const int MaxPartLength = 40;

        /// <summary>
        /// Creates a name.
        /// </summary>
        /// <param name="givenName">Given name; trimmed, non-empty.</param>
        /// <param name="familyName">Family name; trimmed, non-empty.</param>
        public PersonName(string givenName, string familyName)
        {
            GivenName = RequirePart(givenName, nameof(givenName));
            FamilyName = RequirePart(familyName, nameof(familyName));
        }

        /// <summary>Given name.</summary>
        public string GivenName { get; }

        /// <summary>Family name.</summary>
        public string FamilyName { get; }

        private static string RequirePart(string part, string paramName)
        {
            string trimmed = part?.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.Length > MaxPartLength)
            {
                throw new ArgumentException($"Name part must be 1-{MaxPartLength} characters.", paramName);
            }
            return trimmed;
        }
    }
}
