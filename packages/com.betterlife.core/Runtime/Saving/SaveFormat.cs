namespace BetterLife.Core.Saving
{
    /// <summary>
    /// Property names and markers of the save file envelope.
    /// </summary>
    public static class SaveFormat
    {
        /// <summary>Value of the "format" property.</summary>
        public const string FormatId = "betterlife.save";

        /// <summary>Envelope property: format marker.</summary>
        public const string FormatProperty = "format";

        /// <summary>Envelope property: save version of the payload.</summary>
        public const string VersionProperty = "saveVersion";

        /// <summary>Envelope property: checksum of the payload.</summary>
        public const string ChecksumProperty = "checksum";

        /// <summary>Envelope property: the saved life.</summary>
        public const string PayloadProperty = "payload";
    }
}
