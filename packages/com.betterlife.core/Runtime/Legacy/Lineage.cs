using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Legacy
{
    /// <summary>
    /// The ancestors of the current character, oldest first (the family tree above the current life).
    /// </summary>
    public sealed class Lineage
    {
        private readonly List<AncestorRecord> _ancestors = new List<AncestorRecord>();

        /// <summary>Ancestors, oldest first.</summary>
        public IReadOnlyList<AncestorRecord> Ancestors => _ancestors;

        /// <summary>Generation of the next life (ancestors + 1).</summary>
        public int NextGeneration => _ancestors.Count == 0 ? 1 : _ancestors.Max(a => a.Generation) + 1;

        /// <summary>
        /// Replaces all ancestors (loading a save or starting an heir).
        /// </summary>
        /// <param name="ancestors">Ancestors in any order.</param>
        public void Restore(IEnumerable<AncestorRecord> ancestors)
        {
            if (ancestors == null)
            {
                throw new ArgumentNullException(nameof(ancestors));
            }
            _ancestors.Clear();
            _ancestors.AddRange(ancestors.Where(a => a != null).OrderBy(a => a.Generation));
        }
    }
}
