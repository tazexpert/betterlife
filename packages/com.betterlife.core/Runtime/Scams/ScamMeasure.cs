namespace BetterLife.Core.Scams
{
    /// <summary>
    /// What a scam condition compares.
    /// </summary>
    public enum ScamMeasure
    {
        /// <summary>Times scammed.</summary>
        Victims = 0,

        /// <summary>Money lost overall.</summary>
        Lost = 1,

        /// <summary>Times a scam was spotted.</summary>
        Detected = 2,
    }
}
