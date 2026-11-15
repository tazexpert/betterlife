using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// The character used a vice.
    /// </summary>
    public sealed class ViceUsedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="result">What happened.</param>
        /// <param name="week">Week.</param>
        public ViceUsedEvent(ViceUseResult result, GameDate week)
        {
            Result = result;
            Week = week;
        }

        /// <summary>What happened.</summary>
        public ViceUseResult Result { get; }

        /// <summary>Week.</summary>
        public GameDate Week { get; }
    }
}
