using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Domain.Life
{
    /// <summary>
    /// Life stages ordered by age. The first starts at age 0 and start ages are unique, so every age has one stage.
    /// </summary>
    public sealed class LifeStageCatalog
    {
        private readonly List<LifeStageDefinition> _byAge;
        private readonly DefinitionCatalog<LifeStageDefinition> _byId;

        /// <summary>
        /// Creates the catalog.
        /// </summary>
        /// <param name="stages">Stages with unique ids and start ages, one starting at 0.</param>
        public LifeStageCatalog(IEnumerable<LifeStageDefinition> stages)
        {
            _byId = new DefinitionCatalog<LifeStageDefinition>(stages ?? throw new ArgumentNullException(nameof(stages)));
            _byAge = _byId.All.OrderBy(s => s.MinAge).ToList();
            if (_byAge.Count == 0 || _byAge[0].MinAge != 0)
            {
                throw new ArgumentException("A stage starting at age 0 is required.", nameof(stages));
            }
            if (_byAge.Select(s => s.MinAge).Distinct().Count() != _byAge.Count)
            {
                throw new ArgumentException("Stages must start at different ages.", nameof(stages));
            }
        }

        /// <summary>Stages ordered by start age.</summary>
        public IReadOnlyList<LifeStageDefinition> All => _byAge;

        /// <summary>
        /// Returns true when the stage exists.
        /// </summary>
        /// <param name="id">Stage id.</param>
        /// <returns>True when defined.</returns>
        public bool Contains(string id) => _byId.Contains(id);

        /// <summary>
        /// Returns a stage by id.
        /// </summary>
        /// <param name="id">Stage id.</param>
        /// <returns>The stage.</returns>
        public LifeStageDefinition Get(string id) => _byId.Get(id);

        /// <summary>
        /// Returns the stage of an age.
        /// </summary>
        /// <param name="age">Age in years; non-negative.</param>
        /// <returns>The stage.</returns>
        public LifeStageDefinition StageFor(int age)
        {
            if (age < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(age), age, "Age must be non-negative.");
            }
            return _byAge.Last(s => s.MinAge <= age);
        }
    }
}
