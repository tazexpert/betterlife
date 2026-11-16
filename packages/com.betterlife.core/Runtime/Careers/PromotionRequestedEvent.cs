using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// An event effect asks the career service to promote the character one level.
    /// </summary>
    public sealed class PromotionRequestedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="week">Week of the promotion.</param>
        public PromotionRequestedEvent(GameDate week)
        {
            Week = week;
        }

        /// <summary>Week of the promotion.</summary>
        public GameDate Week { get; }
    }
}
