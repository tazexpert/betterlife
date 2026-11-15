using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time;

namespace BetterLife.Core.Rules
{
    /// <summary>
    /// Everything a condition or effect may read or change. Systems that need more data provide it
    /// to their own condition types through constructor injection, keeping this context small.
    /// </summary>
    public sealed class RuleContext
    {
        /// <summary>
        /// Creates a context.
        /// </summary>
        /// <param name="character">The character being evaluated or changed.</param>
        /// <param name="now">Current week.</param>
        /// <param name="random">Random stream used to roll ranges.</param>
        /// <param name="sourceKey">Default cause recorded on stat changes, for example "event.lost_wallet".</param>
        public RuleContext(Character character, GameDate now, IRandomProvider random, string sourceKey)
        {
            Character = character ?? throw new ArgumentNullException(nameof(character));
            Random = random ?? throw new ArgumentNullException(nameof(random));
            Now = now;
            SourceKey = ContentId.RequireLocKey(sourceKey, nameof(sourceKey));
        }

        /// <summary>The character.</summary>
        public Character Character { get; }

        /// <summary>Current week.</summary>
        public GameDate Now { get; }

        /// <summary>Random stream.</summary>
        public IRandomProvider Random { get; }

        /// <summary>Default cause for stat changes.</summary>
        public string SourceKey { get; }
    }
}
