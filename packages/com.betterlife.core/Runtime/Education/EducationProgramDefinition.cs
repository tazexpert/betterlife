using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Time.Budget;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// One level of schooling (primary, bachelor...) and the institutions that offer it.
    /// </summary>
    public sealed class EducationProgramDefinition : IDefinition
    {
        /// <summary>
        /// Creates the program.
        /// </summary>
        /// <param name="id">Unique id.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="rank">Level rank; equivalent programs share a rank.</param>
        /// <param name="years">School years to graduate.</param>
        /// <param name="minEntryAge">Youngest entry age.</param>
        /// <param name="maxEntryAge">Oldest entry age, or null for none.</param>
        /// <param name="requiresRank">Rank of a credential needed to enter (0 = none).</param>
        /// <param name="compulsory">Dependents may not drop out.</param>
        /// <param name="autoEnroll">Dependents are enrolled automatically.</param>
        /// <param name="weeklyHours">School hours for full attendance.</param>
        /// <param name="skills">Skills practiced per school hour.</param>
        /// <param name="institutions">Institutions; at least one.</param>
        public EducationProgramDefinition(string id, string nameKey, int rank, int years, int minEntryAge, int? maxEntryAge, int requiresRank,
            bool compulsory, bool autoEnroll, int weeklyHours, IEnumerable<ActivitySkillEffect> skills, IEnumerable<InstitutionDefinition> institutions)
        {
            if (rank < 1 || years < 1 || minEntryAge < 0 || (maxEntryAge.HasValue && maxEntryAge.Value < minEntryAge) || requiresRank < 0 || requiresRank >= rank)
            {
                throw new ArgumentOutOfRangeException(nameof(rank), "Rank, years and ages are out of range (requiresRank must be below rank).");
            }
            if (weeklyHours < 1 || weeklyHours > TimeBudget.HoursPerWeek)
            {
                throw new ArgumentOutOfRangeException(nameof(weeklyHours), weeklyHours, "Weekly hours must be within 1-168.");
            }
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            Rank = rank;
            Years = years;
            MinEntryAge = minEntryAge;
            MaxEntryAge = maxEntryAge;
            RequiresRank = requiresRank;
            Compulsory = compulsory;
            AutoEnroll = autoEnroll;
            WeeklyHours = weeklyHours;
            Skills = (skills ?? Enumerable.Empty<ActivitySkillEffect>()).ToList();
            Institutions = new DefinitionCatalog<InstitutionDefinition>(institutions ?? throw new ArgumentNullException(nameof(institutions)));
            if (Institutions.All.Count == 0)
            {
                throw new ArgumentException("A program needs at least one institution.", nameof(institutions));
            }
        }

        /// <summary>Unique id.</summary>
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Level rank.</summary>
        public int Rank { get; }

        /// <summary>School years to graduate.</summary>
        public int Years { get; }

        /// <summary>Youngest entry age.</summary>
        public int MinEntryAge { get; }

        /// <summary>Oldest entry age, or null.</summary>
        public int? MaxEntryAge { get; }

        /// <summary>Rank of a credential needed to enter (0 = none).</summary>
        public int RequiresRank { get; }

        /// <summary>Dependents may not drop out.</summary>
        public bool Compulsory { get; }

        /// <summary>Dependents are enrolled automatically.</summary>
        public bool AutoEnroll { get; }

        /// <summary>School hours for full attendance.</summary>
        public int WeeklyHours { get; }

        /// <summary>Skills practiced per school hour.</summary>
        public IReadOnlyList<ActivitySkillEffect> Skills { get; }

        /// <summary>Institutions offering the program.</summary>
        public DefinitionCatalog<InstitutionDefinition> Institutions { get; }

        /// <summary>
        /// Returns true when the age is within the entry ages.
        /// </summary>
        /// <param name="age">Age in years.</param>
        /// <returns>True when allowed.</returns>
        public bool AcceptsAge(int age) => age >= MinEntryAge && (!MaxEntryAge.HasValue || age <= MaxEntryAge.Value);
    }
}
