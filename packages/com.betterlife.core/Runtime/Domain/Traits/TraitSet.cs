using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Time;

namespace BetterLife.Core.Domain.Traits
{
    /// <summary>
    /// The traits of one character. Enforces exclusive groups and combines stat multipliers.
    /// Hidden traits still take effect; hiding only affects what the player sees.
    /// </summary>
    public sealed class TraitSet
    {
        private readonly DefinitionCatalog<TraitDefinition> _catalog;
        private readonly List<TraitInstance> _instances = new List<TraitInstance>();

        /// <summary>
        /// Creates an empty trait set.
        /// </summary>
        /// <param name="catalog">Trait definitions.</param>
        public TraitSet(DefinitionCatalog<TraitDefinition> catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        /// <summary>Held traits in acquisition order.</summary>
        public IReadOnlyList<TraitInstance> Instances => _instances;

        /// <summary>
        /// Returns true when the character has the trait.
        /// </summary>
        /// <param name="traitId">Trait id.</param>
        /// <returns>True when held.</returns>
        public bool Has(string traitId) => Find(traitId) != null;

        /// <summary>
        /// Adds a trait unless it is already held or conflicts with an exclusive group.
        /// </summary>
        /// <param name="traitId">Known trait id.</param>
        /// <param name="date">Week of acquisition.</param>
        /// <returns>The outcome.</returns>
        public TraitAddOutcome Add(string traitId, GameDate date)
        {
            TraitDefinition definition = _catalog.Get(traitId);
            if (Has(traitId))
            {
                return TraitAddOutcome.AlreadyHas;
            }
            if (definition.ExclusiveGroup != null && _instances.Any(i => _catalog.Get(i.TraitId).ExclusiveGroup == definition.ExclusiveGroup))
            {
                return TraitAddOutcome.ExclusiveConflict;
            }

            _instances.Add(new TraitInstance(traitId, date, !definition.StartsHidden));
            return TraitAddOutcome.Added;
        }

        /// <summary>
        /// Removes a trait.
        /// </summary>
        /// <param name="traitId">Trait id.</param>
        /// <returns>True when it was held.</returns>
        public bool Remove(string traitId)
        {
            TraitInstance instance = Find(traitId);
            return instance != null && _instances.Remove(instance);
        }

        /// <summary>
        /// Reveals a held trait to the player.
        /// </summary>
        /// <param name="traitId">Trait id.</param>
        /// <returns>True when it was held and hidden before.</returns>
        public bool Reveal(string traitId)
        {
            TraitInstance instance = Find(traitId);
            return instance != null && instance.Reveal();
        }

        /// <summary>
        /// Returns the product of all held traits' multipliers that apply to this change.
        /// </summary>
        /// <param name="statId">Changed stat.</param>
        /// <param name="delta">Requested delta.</param>
        /// <returns>Combined multiplier (1 when none apply).</returns>
        public float GetMultiplier(StatId statId, float delta)
        {
            float multiplier = 1f;
            foreach (TraitInstance instance in _instances)
            {
                foreach (TraitStatModifier modifier in _catalog.Get(instance.TraitId).StatModifiers)
                {
                    if (modifier.AppliesTo(statId, delta))
                    {
                        multiplier *= modifier.Multiplier;
                    }
                }
            }
            return multiplier;
        }

        /// <summary>
        /// Restores a saved trait without exclusivity checks. Unknown ids are rejected.
        /// </summary>
        /// <param name="instance">Saved instance.</param>
        public void Restore(TraitInstance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            _catalog.Get(instance.TraitId);
            if (!Has(instance.TraitId))
            {
                _instances.Add(instance);
            }
        }

        private TraitInstance Find(string traitId)
        {
            return _instances.FirstOrDefault(i => string.Equals(i.TraitId, traitId, StringComparison.Ordinal));
        }
    }
}
