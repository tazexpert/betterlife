using BetterLife.Core.Rules;
using BetterLife.Core.Time;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// A life event without choices that has happened (a "story"), with the effects it applied, kept so the UI can show it.
    /// </summary>
    public sealed class StoryEntry
    {
        /// <summary>
        /// Creates the entry.
        /// </summary>
        /// <param name="instanceId">Event instance id.</param>
        /// <param name="definition">Event definition.</param>
        /// <param name="week">Week it happened.</param>
        /// <param name="effects">Effects applied when it happened.</param>
        public StoryEntry(int instanceId, LifeEventDefinition definition, GameDate week, EffectResult effects)
        {
            InstanceId = instanceId;
            Definition = definition;
            Week = week;
            Effects = effects;
        }

        /// <summary>Event instance id.</summary>
        public int InstanceId { get; }

        /// <summary>Event definition.</summary>
        public LifeEventDefinition Definition { get; }

        /// <summary>Week it happened.</summary>
        public GameDate Week { get; }

        /// <summary>Effects applied when it happened.</summary>
        public EffectResult Effects { get; }
    }
}
