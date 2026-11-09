using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Luck;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Whether the character can do an interaction with an NPC now, and the odds.
    /// </summary>
    public sealed class InteractionQuote
    {
        /// <summary>The NPC has died.</summary>
        public const string ReasonDead = "interaction.reason.dead";
        /// <summary>Not available for this kind of relationship.</summary>
        public const string ReasonType = "interaction.reason.type";
        /// <summary>The relationship is not close enough.</summary>
        public const string ReasonTooLow = "interaction.reason.too_low";
        /// <summary>The relationship is too good for it.</summary>
        public const string ReasonTooHigh = "interaction.reason.too_high";
        /// <summary>Used too recently with this NPC.</summary>
        public const string ReasonCooldown = "interaction.reason.cooldown";
        /// <summary>The character's situation does not allow it.</summary>
        public const string ReasonCondition = "interaction.reason.condition";
        /// <summary>Not enough cash.</summary>
        public const string ReasonCash = "interaction.reason.cash";
        /// <summary>Either partner cannot have this child now.</summary>
        public const string ReasonFertility = "interaction.reason.fertility";

        /// <summary>
        /// Creates the quote.
        /// </summary>
        /// <param name="reasonKeys">Why not; empty when available.</param>
        /// <param name="chance">Success chance, or null when it always succeeds.</param>
        /// <param name="compatibility">Compatibility with the NPC.</param>
        /// <param name="cost">Cash cost.</param>
        public InteractionQuote(IReadOnlyList<string> reasonKeys, ChanceBreakdown chance, CompatibilityBreakdown compatibility, decimal cost)
        {
            ReasonKeys = (reasonKeys ?? throw new ArgumentNullException(nameof(reasonKeys))).ToList();
            Chance = chance;
            Compatibility = compatibility ?? throw new ArgumentNullException(nameof(compatibility));
            Cost = cost;
        }

        /// <summary>True when the interaction is available.</summary>
        public bool Available => ReasonKeys.Count == 0;

        /// <summary>Why not, as localization keys.</summary>
        public IReadOnlyList<string> ReasonKeys { get; }

        /// <summary>Success chance, or null when certain.</summary>
        public ChanceBreakdown Chance { get; }

        /// <summary>Compatibility with the NPC.</summary>
        public CompatibilityBreakdown Compatibility { get; }

        /// <summary>Cash cost.</summary>
        public decimal Cost { get; }
    }
}
