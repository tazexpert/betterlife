using System;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Yearly relationship update in the NPC step of the tick.
    /// </summary>
    public sealed class RelationshipYearPhase : ITickPhase
    {
        private readonly RelationshipService _relationships;

        /// <summary>
        /// Creates the phase.
        /// </summary>
        /// <param name="relationships">Relationship service.</param>
        public RelationshipYearPhase(RelationshipService relationships)
        {
            _relationships = relationships ?? throw new ArgumentNullException(nameof(relationships));
        }

        /// <inheritdoc />
        public string Name => "relationships_year";

        /// <inheritdoc />
        public int Order => TickOrder.NpcUpdate;

        /// <inheritdoc />
        public TickCadence Cadence => TickCadence.Yearly;

        /// <inheritdoc />
        public void Execute(TickContext context) => _relationships.ProcessYear(context);
    }
}
