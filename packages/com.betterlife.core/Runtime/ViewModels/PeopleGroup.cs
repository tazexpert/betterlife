using System.Collections.Generic;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// A titled group of people (family, love, friends, others).
    /// </summary>
    public sealed class PeopleGroup
    {
        /// <summary>
        /// Creates a group.
        /// </summary>
        /// <param name="title">Group title.</param>
        /// <param name="people">People, closest first, the departed last.</param>
        public PeopleGroup(string title, IReadOnlyList<PersonRow> people)
        {
            Title = title;
            People = people;
        }

        /// <summary>Group title.</summary>
        public string Title { get; }

        /// <summary>People.</summary>
        public IReadOnlyList<PersonRow> People { get; }
    }
}
