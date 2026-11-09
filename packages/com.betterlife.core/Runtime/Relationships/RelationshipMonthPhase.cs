using System;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Monthly relationship update in the NPC step of the tick.
    /// </summary>
    public sealed class RelationshipMonthPhase : ITickPhase
    {
        private readonly RelationshipService _relationships;

        /// <summary>
        /// Creates the phase.
        /// </summary>
        /// <param name="relationships">Relationship service.</param>
        public RelationshipMonthPhase(RelationshipService relationships)
        {
            _relationships = relationships ?? throw new ArgumentNullException(nameof(relationships));
        }

        /// <inheritdoc />
        public string Name => "relationships_month";

        /// <inheritdoc />
        public int Order => TickOrder.NpcUpdate;

        /// <inheritdoc />
        public TickCadence Cadence => TickCadence.Monthly;

        /// <inheritdoc />
        public void Execute(TickContext context) => _relationships.ProcessMonth(context);
    }
}
