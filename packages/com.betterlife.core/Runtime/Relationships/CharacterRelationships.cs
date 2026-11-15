using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Time;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Everyone in the character's life: NPCs, the character's relationship with each, and children on the way.
    /// </summary>
    public sealed class CharacterRelationships
    {
        private readonly Dictionary<int, Npc> _npcs = new Dictionary<int, Npc>();
        private readonly List<Relationship> _relationships = new List<Relationship>();
        private readonly List<Pregnancy> _pregnancies = new List<Pregnancy>();

        /// <summary>Every NPC ever met (including the dead), by id order.</summary>
        public IReadOnlyCollection<Npc> Npcs => _npcs.Values;

        /// <summary>Relationships, in the order they began.</summary>
        public IReadOnlyList<Relationship> All => _relationships;

        /// <summary>Children on the way.</summary>
        public IReadOnlyList<Pregnancy> Pregnancies => _pregnancies;

        /// <summary>Next NPC id.</summary>
        public int NextNpcId { get; private set; } = 1;

        /// <summary>
        /// Returns an NPC, or null.
        /// </summary>
        /// <param name="npcId">Id.</param>
        /// <returns>The NPC.</returns>
        public Npc Npc(int npcId) => _npcs.TryGetValue(npcId, out Npc npc) ? npc : null;

        /// <summary>
        /// Returns the relationship with an NPC, or null.
        /// </summary>
        /// <param name="npcId">Id.</param>
        /// <returns>The relationship.</returns>
        public Relationship With(int npcId) => _relationships.FirstOrDefault(r => r.NpcId == npcId);

        /// <summary>
        /// Returns the relationships of a type with living NPCs.
        /// </summary>
        /// <param name="type">Type, or null for any.</param>
        /// <returns>Relationships.</returns>
        public IEnumerable<Relationship> Living(RelationshipType? type = null)
        {
            return _relationships.Where(r => (!type.HasValue || r.Type == type.Value) && Npc(r.NpcId)?.IsAlive == true);
        }

        /// <summary>
        /// Returns the living relationship of a type with the highest value, or null.
        /// </summary>
        /// <param name="type">Type, or null for any.</param>
        /// <returns>The relationship.</returns>
        public Relationship Closest(RelationshipType? type = null) => Living(type).OrderByDescending(r => r.Value).FirstOrDefault();

        /// <summary>
        /// Adds an NPC and the character's relationship with them.
        /// </summary>
        /// <param name="create">Builds the NPC from its new id.</param>
        /// <param name="type">Role.</param>
        /// <param name="value">Starting value.</param>
        /// <param name="week">Week they met (or birth week).</param>
        /// <returns>The relationship.</returns>
        public Relationship Add(Func<int, Npc> create, RelationshipType type, float value, GameDate week)
        {
            if (create == null)
            {
                throw new ArgumentNullException(nameof(create));
            }
            Npc npc = create(NextNpcId);
            if (npc == null || npc.Id != NextNpcId)
            {
                throw new InvalidOperationException("The NPC must use the id it was given.");
            }
            NextNpcId++;
            _npcs.Add(npc.Id, npc);
            var relationship = new Relationship(npc.Id, type, value, week, week);
            _relationships.Add(relationship);
            return relationship;
        }

        /// <summary>
        /// Forgets an NPC entirely (used to keep the list short).
        /// </summary>
        /// <param name="npcId">Id.</param>
        /// <returns>True when removed.</returns>
        public bool Forget(int npcId)
        {
            _pregnancies.RemoveAll(p => p.PartnerNpcId == npcId);
            return _relationships.RemoveAll(r => r.NpcId == npcId) > 0 | _npcs.Remove(npcId);
        }

        /// <summary>
        /// Records a child on the way.
        /// </summary>
        /// <param name="pregnancy">Pregnancy.</param>
        public void AddPregnancy(Pregnancy pregnancy) => _pregnancies.Add(pregnancy ?? throw new ArgumentNullException(nameof(pregnancy)));

        /// <summary>
        /// Removes a pregnancy (after birth).
        /// </summary>
        /// <param name="pregnancy">Pregnancy.</param>
        public void RemovePregnancy(Pregnancy pregnancy) => _pregnancies.Remove(pregnancy);

        /// <summary>
        /// Restores saved state.
        /// </summary>
        /// <param name="npcs">NPCs.</param>
        /// <param name="relationships">Relationships (each must name a restored NPC).</param>
        /// <param name="pregnancies">Pregnancies.</param>
        /// <param name="nextNpcId">Next id; raised above every saved id.</param>
        public void Restore(IEnumerable<Npc> npcs, IEnumerable<Relationship> relationships, IEnumerable<Pregnancy> pregnancies, int nextNpcId)
        {
            _npcs.Clear();
            foreach (Npc npc in npcs ?? Enumerable.Empty<Npc>())
            {
                _npcs[npc.Id] = npc;
            }
            _relationships.Clear();
            foreach (Relationship relationship in relationships ?? Enumerable.Empty<Relationship>())
            {
                if (!_npcs.ContainsKey(relationship.NpcId))
                {
                    throw new ArgumentException($"Relationship with unknown NPC {relationship.NpcId}.", nameof(relationships));
                }
                _relationships.Add(relationship);
            }
            _pregnancies.Clear();
            _pregnancies.AddRange((pregnancies ?? Enumerable.Empty<Pregnancy>()).Where(p => _npcs.ContainsKey(p.PartnerNpcId)));
            NextNpcId = Math.Max(Math.Max(1, nextNpcId), _npcs.Keys.DefaultIfEmpty(0).Max() + 1);
        }
    }
}
