namespace BetterLife.Core.Navigation
{
    /// <summary>
    /// What the Back action did.
    /// </summary>
    public enum BackResult
    {
        /// <summary>Closed the top modal.</summary>
        ClosedModal = 0,

        /// <summary>Went back one screen in the current tab.</summary>
        Popped = 1,

        /// <summary>Switched to the home tab.</summary>
        ReturnedHome = 2,

        /// <summary>Nothing left to go back to: the app may exit.</summary>
        Exit = 3,
    }
}
