using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Time;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// The character's tie with one NPC: a role, a value from −100 to 100, and memories.
    /// </summary>
    public sealed class Relationship
    {
        /// <summary>Lowest value.</summary>
        public const float Min = -100f;

        /// <summary>Highest value.</summary>
        public const float Max = 100f;

        /// <summary>Memories kept per relationship (oldest dropped first).</summary>
        public const int MaxMemories = 8;

        private readonly List<RelationshipMemory> _memories = new List<RelationshipMemory>();
        private readonly Dictionary<string, GameDate> _cooldowns = new Dictionary<string, GameDate>(StringComparer.Ordinal);

        /// <summary>
        /// Creates or restores a relationship.
        /// </summary>
        /// <param name="npcId">NPC id.</param>
        /// <param name="type">Role.</param>
        /// <param name="value">Value, clamped to −100..100.</param>
        /// <param name="since">First week.</param>
        /// <param name="lastContact">Week of the last interaction.</param>
        /// <param name="lowMonths">Consecutive months below the break-up line.</param>
        public Relationship(int npcId, RelationshipType type, float value, GameDate since, GameDate lastContact, int lowMonths = 0)
        {
            if (npcId <= 0 || lowMonths < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(npcId), "Id must be positive and months non-negative.");
            }
            NpcId = npcId;
            Type = type;
            Value = Clamp(value);
            Since = since;
            LastContact = lastContact;
            LowMonths = lowMonths;
        }

        /// <summary>NPC id.</summary>
        public int NpcId { get; }

        /// <summary>Role.</summary>
        public RelationshipType Type { get; private set; }

        /// <summary>Value −100..100.</summary>
        public float Value { get; private set; }

        /// <summary>First week in the current role.</summary>
        public GameDate Since { get; private set; }

        /// <summary>Week of the last interaction.</summary>
        public GameDate LastContact { get; private set; }

        /// <summary>Consecutive months below the break-up line.</summary>
        public int LowMonths { get; private set; }

        /// <summary>Memories, oldest first.</summary>
        public IReadOnlyList<RelationshipMemory> Memories => _memories;

        /// <summary>Last use week per interaction id.</summary>
        public IReadOnlyDictionary<string, GameDate> Cooldowns => _cooldowns;

        /// <summary>
        /// Changes the value, clamped to −100..100, and returns the applied change.
        /// </summary>
        /// <param name="delta">Requested change.</param>
        /// <returns>Applied change.</returns>
        public float Change(float delta)
        {
            float before = Value;
            Value = Clamp(Value + delta);
            return Value - before;
        }

        /// <summary>
        /// Records a memory (keeps the newest <see cref="MaxMemories"/>).
        /// </summary>
        /// <param name="memory">Memory.</param>
        public void Remember(RelationshipMemory memory)
        {
            _memories.Add(memory ?? throw new ArgumentNullException(nameof(memory)));
            if (_memories.Count > MaxMemories)
            {
                _memories.RemoveAt(0);
            }
        }

        /// <summary>
        /// Changes the role (dating, marriage, break-up...).
        /// </summary>
        /// <param name="type">New role.</param>
        /// <param name="week">Week.</param>
        public void Become(RelationshipType type, GameDate week)
        {
            if (type != Type)
            {
                Type = type;
                Since = week;
                LowMonths = 0;
            }
        }

        /// <summary>
        /// Records an interaction.
        /// </summary>
        /// <param name="interactionId">Interaction id, or null for general contact.</param>
        /// <param name="week">Week.</param>
        public void Touch(string interactionId, GameDate week)
        {
            LastContact = week;
            if (interactionId != null)
            {
                _cooldowns[ContentId.Require(interactionId, nameof(interactionId))] = week;
            }
        }

        /// <summary>
        /// Restores saved memories and cooldowns.
        /// </summary>
        /// <param name="memories">Memories.</param>
        /// <param name="cooldowns">Cooldowns.</param>
        public void Restore(IEnumerable<RelationshipMemory> memories, IEnumerable<KeyValuePair<string, GameDate>> cooldowns)
        {
            _memories.Clear();
            _memories.AddRange((memories ?? Enumerable.Empty<RelationshipMemory>()).Take(MaxMemories));
            _cooldowns.Clear();
            foreach (KeyValuePair<string, GameDate> pair in cooldowns ?? Enumerable.Empty<KeyValuePair<string, GameDate>>())
            {
                _cooldowns[pair.Key] = pair.Value;
            }
        }

        internal void CountLowMonth(bool low) => LowMonths = low ? LowMonths + 1 : 0;

        private static float Clamp(float value) => Math.Max(Min, Math.Min(Max, value));
    }
}
