namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// What a timeline entry is about (drives its icon and colour).
    /// </summary>
    public enum TimelineEntryKind
    {
        /// <summary>Birth.</summary>
        Birth = 0,
        /// <summary>A life event and the answer given.</summary>
        Event = 1,
        /// <summary>Graduation.</summary>
        Education = 2,
        /// <summary>A job started or ended.</summary>
        Career = 3,
        /// <summary>Love, marriage, break-up, a child.</summary>
        Relationship = 4,
        /// <summary>Someone close passed away.</summary>
        Loss = 5,
        /// <summary>Scammed.</summary>
        Scam = 6,
        /// <summary>The character's death.</summary>
        Death = 7,
    }
}
