using BetterLife.Core.Luck;
using BetterLife.Core.Rules;
using BetterLife.Core.Time;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// The result of answering (or auto-answering) an event.
    /// </summary>
    public sealed class EventResolution
    {
        /// <summary>
        /// Creates a resolution.
        /// </summary>
        /// <param name="pending">The answered occurrence.</param>
        /// <param name="definition">The event.</param>
        /// <param name="choice">The chosen option, or null when dismissed.</param>
        /// <param name="roll">The chance roll, or null for a guaranteed outcome.</param>
        /// <param name="outcome">The applied outcome, or null when dismissed.</param>
        /// <param name="result">What the outcome's effects did.</param>
        /// <param name="week">Week of the answer.</param>
        /// <param name="wasAutomatic">True when the default choice was applied after the deadline.</param>
        public EventResolution(PendingEvent pending, LifeEventDefinition definition, EventChoice choice, ChanceRoll roll, EventOutcome outcome,
            EffectResult result, GameDate week, bool wasAutomatic)
        {
            Pending = pending;
            Definition = definition;
            Choice = choice;
            Roll = roll;
            Outcome = outcome;
            Result = result;
            Week = week;
            WasAutomatic = wasAutomatic;
        }

        /// <summary>The answered occurrence.</summary>
        public PendingEvent Pending { get; }

        /// <summary>The event.</summary>
        public LifeEventDefinition Definition { get; }

        /// <summary>The chosen option, or null.</summary>
        public EventChoice Choice { get; }

        /// <summary>The chance roll, or null.</summary>
        public ChanceRoll Roll { get; }

        /// <summary>The applied outcome, or null.</summary>
        public EventOutcome Outcome { get; }

        /// <summary>What the effects did.</summary>
        public EffectResult Result { get; }

        /// <summary>Week of the answer.</summary>
        public GameDate Week { get; }

        /// <summary>True when applied automatically.</summary>
        public bool WasAutomatic { get; }

        /// <summary>True unless a chance roll failed.</summary>
        public bool Succeeded => Roll == null || Roll.Success;
    }
}
