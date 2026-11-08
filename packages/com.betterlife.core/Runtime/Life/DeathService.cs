using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Life;
using BetterLife.Core.Events;
using BetterLife.Core.Randomness;
using BetterLife.Core.Rules;
using BetterLife.Core.Time;

namespace BetterLife.Core.Life
{
    /// <summary>
    /// Ends lives: picks natural causes by weight and handles death requests from effects.
    /// </summary>
    public sealed class DeathService : IDisposable
    {
        private const string SourceKey = "source.death";

        private readonly DefinitionCatalog<DeathCauseDefinition> _causes;
        private readonly IEventBus _bus;
        private readonly IDisposable _subscription;

        /// <summary>
        /// Creates the service and listens for death requests.
        /// </summary>
        /// <param name="causes">Death causes (at least one).</param>
        /// <param name="bus">Event bus.</param>
        public DeathService(DefinitionCatalog<DeathCauseDefinition> causes, IEventBus bus)
        {
            _causes = causes ?? throw new ArgumentNullException(nameof(causes));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            if (_causes.All.Count == 0)
            {
                throw new ArgumentException("At least one death cause is required.", nameof(causes));
            }
            _subscription = _bus.Subscribe<DeathRequest>(r => Kill(r.Character, r.CauseId, r.Week));
        }

        /// <summary>
        /// Picks a natural cause for a death this week by weight among eligible causes (the first cause when none qualify).
        /// </summary>
        /// <param name="character">Character.</param>
        /// <param name="week">Week of death.</param>
        /// <param name="random">Random stream.</param>
        /// <returns>The cause.</returns>
        public DeathCauseDefinition PickCause(Character character, GameDate week, IRandomProvider random)
        {
            var context = new RuleContext(character, week, random, SourceKey);
            List<(DeathCauseDefinition Cause, float Weight)> candidates = _causes.All
                .Where(c => c.Condition == null || c.Condition.Evaluate(context))
                .Select(c => (c, c.Weight.Evaluate(context)))
                .ToList();
            DeathCauseDefinition picked = random.PickWeighted(candidates, c => c.Weight).Cause;
            return picked ?? _causes.All[0];
        }

        /// <summary>
        /// Ends a life (ignored when already dead) and publishes <see cref="CharacterDiedEvent"/>.
        /// </summary>
        /// <param name="character">Character.</param>
        /// <param name="causeId">Declared cause id.</param>
        /// <param name="week">Week of death.</param>
        /// <returns>The death record, or null when the character was already dead.</returns>
        public DeathRecord Kill(Character character, string causeId, GameDate week)
        {
            if (character == null)
            {
                throw new ArgumentNullException(nameof(character));
            }
            _causes.Get(causeId);
            if (!character.IsAlive)
            {
                return null;
            }
            DeathRecord record = character.Die(causeId, week);
            _bus.Publish(new CharacterDiedEvent(character, record));
            return record;
        }

        /// <summary>
        /// Stops listening for death requests.
        /// </summary>
        public void Dispose() => _subscription.Dispose();
    }
}
