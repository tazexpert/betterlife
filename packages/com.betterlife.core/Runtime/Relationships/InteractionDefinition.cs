using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Luck;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Something the character can do with an NPC (chat, gift, ask out, propose, break up, have a child...).
    /// </summary>
    public sealed class InteractionDefinition : IDefinition
    {
        /// <summary>
        /// Creates the interaction.
        /// </summary>
        /// <param name="id">Unique id.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="types">Relationship types it applies to.</param>
        /// <param name="minValue">Lowest relationship value, or null.</param>
        /// <param name="maxValue">Highest relationship value, or null.</param>
        /// <param name="condition">Condition on the character, or null.</param>
        /// <param name="costCash">Cash cost; non-negative.</param>
        /// <param name="cooldownWeeks">Weeks before using it again with the same NPC.</param>
        /// <param name="chance">Success chance, or null for always.</param>
        /// <param name="compatibilityPerPoint">Chance points per compatibility point above 50.</param>
        /// <param name="success">Outcome on success.</param>
        /// <param name="failure">Outcome on failure.</param>
        public InteractionDefinition(string id, string nameKey, IEnumerable<RelationshipType> types, float? minValue, float? maxValue, ICondition condition,
            decimal costCash, int cooldownWeeks, ChanceSpec chance, float compatibilityPerPoint, InteractionOutcome success, InteractionOutcome failure)
        {
            Types = new HashSet<RelationshipType>(types ?? throw new ArgumentNullException(nameof(types)));
            if (Types.Count == 0 || costCash < 0m || cooldownWeeks < 0 || compatibilityPerPoint < 0f
                || (minValue.HasValue && maxValue.HasValue && maxValue.Value < minValue.Value))
            {
                throw new ArgumentOutOfRangeException(nameof(types), "Types, cost, cooldown, compatibility or value range out of range.");
            }
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            MinValue = minValue;
            MaxValue = maxValue;
            Condition = condition;
            CostCash = costCash;
            CooldownWeeks = cooldownWeeks;
            Chance = chance;
            CompatibilityPerPoint = compatibilityPerPoint;
            Success = success ?? throw new ArgumentNullException(nameof(success));
            Failure = failure ?? throw new ArgumentNullException(nameof(failure));
        }

        /// <summary>Unique id.</summary>
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Relationship types it applies to.</summary>
        public IReadOnlyCollection<RelationshipType> Types { get; }

        /// <summary>Lowest relationship value, or null.</summary>
        public float? MinValue { get; }

        /// <summary>Highest relationship value, or null.</summary>
        public float? MaxValue { get; }

        /// <summary>Condition on the character, or null.</summary>
        public ICondition Condition { get; }

        /// <summary>Cash cost.</summary>
        public decimal CostCash { get; }

        /// <summary>Weeks before reuse with the same NPC.</summary>
        public int CooldownWeeks { get; }

        /// <summary>Success chance, or null for always.</summary>
        public ChanceSpec Chance { get; }

        /// <summary>Chance points per compatibility point above 50.</summary>
        public float CompatibilityPerPoint { get; }

        /// <summary>Outcome on success.</summary>
        public InteractionOutcome Success { get; }

        /// <summary>Outcome on failure.</summary>
        public InteractionOutcome Failure { get; }

        /// <summary>True when either outcome may bring a child.</summary>
        public bool InvolvesChild => Success.Child != ChildMode.None || Failure.Child != ChildMode.None;

        /// <summary>
        /// Returns true when it applies to a relationship type.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>True when it applies.</returns>
        public bool AppliesTo(RelationshipType type) => Types.Contains(type);
    }
}
