using BetterLife.Core.Events;
using BetterLife.Core.Rules;
using BetterLife.Core.Time;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// Published when a life event occurs (for the inbox and notifications).
    /// </summary>
    public sealed class LifeEventFiredEvent : IGameEvent
    {
        /// <summary>
        /// Creates the message.
        /// </summary>
        /// <param name="instanceId">Occurrence id.</param>
        /// <param name="definition">The event.</param>
        /// <param name="origin">Why it occurred.</param>
        /// <param name="week">Week it occurred.</param>
        /// <param name="onFire">What its immediate effects did.</param>
        public LifeEventFiredEvent(int instanceId, LifeEventDefinition definition, EventOrigin origin, GameDate week, EffectResult onFire)
        {
            InstanceId = instanceId;
            Definition = definition;
            Origin = origin;
            Week = week;
            OnFire = onFire;
        }

        /// <summary>Occurrence id.</summary>
        public int InstanceId { get; }

        /// <summary>The event.</summary>
        public LifeEventDefinition Definition { get; }

        /// <summary>Why it occurred.</summary>
        public EventOrigin Origin { get; }

        /// <summary>Week it occurred.</summary>
        public GameDate Week { get; }

        /// <summary>What its immediate effects did.</summary>
        public EffectResult OnFire { get; }
    }
}
