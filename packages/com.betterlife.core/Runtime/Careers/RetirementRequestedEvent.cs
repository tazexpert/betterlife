using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// An event effect asks the career service to retire the character now, with a pension.
    /// </summary>
    public sealed class RetirementRequestedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="week">Week of the retirement.</param>
        public RetirementRequestedEvent(GameDate week)
        {
            Week = week;
        }

        /// <summary>Week of the retirement.</summary>
        public GameDate Week { get; }
    }
}
