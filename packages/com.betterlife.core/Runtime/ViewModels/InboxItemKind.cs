namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Kind of inbox item.
    /// </summary>
    public enum InboxItemKind
    {
        /// <summary>An event waiting for the player's answer.</summary>
        Decision = 0,
        /// <summary>News of something that already happened.</summary>
        Notice = 1,
    }
}
