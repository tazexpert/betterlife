using System;
using BetterLife.Core.Persistence;

namespace BetterLife.Core.Saving
{
    /// <summary>
    /// A save read from text, upgraded to the current version.
    /// </summary>
    public sealed class SaveReadResult
    {
        /// <summary>
        /// Creates the result.
        /// </summary>
        /// <param name="save">The saved life.</param>
        /// <param name="originalVersion">Version the file was written with.</param>
        public SaveReadResult(LifeSaveDto save, int originalVersion)
        {
            Save = save ?? throw new ArgumentNullException(nameof(save));
            OriginalVersion = originalVersion;
        }

        /// <summary>The saved life (current version).</summary>
        public LifeSaveDto Save { get; }

        /// <summary>Version the file was written with.</summary>
        public int OriginalVersion { get; }

        /// <summary>True when migrations ran.</summary>
        public bool WasMigrated => OriginalVersion != Save.SaveVersion;
    }
}
