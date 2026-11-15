using BetterLife.Core.Time;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// The tone of a recent event, for anti-frustration.
    /// </summary>
    public sealed class ToneEntry
    {
        /// <summary>
        /// Creates an entry.
        /// </summary>
        /// <param name="week">Week of the event.</param>
        /// <param name="tone">Its tone.</param>
        public ToneEntry(GameDate week, EventTone tone)
        {
            Week = week;
            Tone = tone;
        }

        /// <summary>Week of the event.</summary>
        public GameDate Week { get; }

        /// <summary>Its tone.</summary>
        public EventTone Tone { get; }
    }
}
