using System.Collections.Generic;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// One person in detail: closeness, compatibility, memories and possible interactions.
    /// </summary>
    public sealed class PersonState
    {
        /// <summary>
        /// Creates the state.
        /// </summary>
        /// <param name="npcId">NPC id.</param>
        /// <param name="name">Full name.</param>
        /// <param name="detail">Relationship type, age, since when.</param>
        /// <param name="valueText">Closeness text.</param>
        /// <param name="compatibility">Compatibility breakdown (total first).</param>
        /// <param name="memories">Shared memories, newest first.</param>
        /// <param name="interactions">Interactions for this relationship type.</param>
        public PersonState(int npcId, string name, string detail, string valueText, IReadOnlyList<ValueRow> compatibility,
            IReadOnlyList<ValueRow> memories, IReadOnlyList<InteractionOption> interactions)
        {
            NpcId = npcId;
            Name = name;
            Detail = detail;
            ValueText = valueText;
            Compatibility = compatibility;
            Memories = memories;
            Interactions = interactions;
        }

        /// <summary>NPC id.</summary>
        public int NpcId { get; }

        /// <summary>Full name.</summary>
        public string Name { get; }

        /// <summary>Relationship type, age, since when.</summary>
        public string Detail { get; }

        /// <summary>Closeness text.</summary>
        public string ValueText { get; }

        /// <summary>Compatibility breakdown.</summary>
        public IReadOnlyList<ValueRow> Compatibility { get; }

        /// <summary>Shared memories, newest first.</summary>
        public IReadOnlyList<ValueRow> Memories { get; }

        /// <summary>Interactions for this relationship type.</summary>
        public IReadOnlyList<InteractionOption> Interactions { get; }
    }
}
