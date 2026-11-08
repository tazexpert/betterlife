using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;

namespace BetterLife.Core.Creation
{
    /// <summary>
    /// Random-name pools of one country, loaded from content/names.json. The id is the country id.
    /// </summary>
    public sealed class NameList : IDefinition
    {
        /// <summary>
        /// Creates a name list.
        /// </summary>
        /// <param name="countryId">Country id.</param>
        /// <param name="female">Female given names (at least one).</param>
        /// <param name="male">Male given names (at least one).</param>
        /// <param name="family">Family names (at least one).</param>
        public NameList(string countryId, IEnumerable<NameEntry> female, IEnumerable<NameEntry> male, IEnumerable<NameEntry> family)
        {
            Id = ContentId.Require(countryId, nameof(countryId));
            Female = Require(female, nameof(female));
            Male = Require(male, nameof(male));
            Family = Require(family, nameof(family));
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Female given names.</summary>
        public IReadOnlyList<NameEntry> Female { get; }

        /// <summary>Male given names.</summary>
        public IReadOnlyList<NameEntry> Male { get; }

        /// <summary>Family names.</summary>
        public IReadOnlyList<NameEntry> Family { get; }

        /// <summary>
        /// Returns the given-name pool for a sex.
        /// </summary>
        /// <param name="sex">Biological sex.</param>
        /// <returns>The pool.</returns>
        public IReadOnlyList<NameEntry> GivenNames(BiologicalSex sex) => sex == BiologicalSex.Female ? Female : Male;

        private static IReadOnlyList<NameEntry> Require(IEnumerable<NameEntry> names, string paramName)
        {
            List<NameEntry> list = (names ?? throw new ArgumentNullException(paramName)).ToList();
            if (list.Count == 0 || list.Any(n => n == null))
            {
                throw new ArgumentException("At least one name is required.", paramName);
            }
            return list;
        }
    }
}
