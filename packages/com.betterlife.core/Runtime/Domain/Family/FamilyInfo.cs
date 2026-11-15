using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Domain.Family
{
    /// <summary>
    /// The family a character was born into.
    /// </summary>
    public sealed class FamilyInfo
    {
        /// <summary>
        /// Creates family info.
        /// </summary>
        /// <param name="backgroundId">Family background id.</param>
        /// <param name="parents">Parents (usually two).</param>
        public FamilyInfo(string backgroundId, IEnumerable<ParentRecord> parents)
        {
            BackgroundId = ContentId.Require(backgroundId, nameof(backgroundId));
            Parents = (parents ?? Enumerable.Empty<ParentRecord>()).ToList();
            if (Parents.Any(p => p == null))
            {
                throw new ArgumentException("Parents must not contain null.", nameof(parents));
            }
        }

        /// <summary>Family background id.</summary>
        public string BackgroundId { get; }

        /// <summary>Parents.</summary>
        public IReadOnlyList<ParentRecord> Parents { get; }
    }
}
