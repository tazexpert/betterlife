namespace BetterLife.Core.Saving
{
    /// <summary>
    /// Why a save could not be read.
    /// </summary>
    public enum SaveErrorKind
    {
        /// <summary>The slot is empty.</summary>
        Missing = 0,

        /// <summary>Not a save file, unreadable JSON, or a failed checksum.</summary>
        Corrupt = 1,

        /// <summary>Saved by a newer game or newer content.</summary>
        TooNew = 2,

        /// <summary>No migration path from the saved version.</summary>
        NoMigration = 3,

        /// <summary>The data is readable but invalid.</summary>
        Invalid = 4,
    }
}
