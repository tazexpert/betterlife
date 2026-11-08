using System;
using BetterLife.Core.Domain.Flags;
using BetterLife.Core.Domain.Skills;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Domain.Traits;

namespace BetterLife.Core.Domain.Characters
{
    /// <summary>
    /// The content catalogs a character needs. Created once by the composition root and shared.
    /// </summary>
    public sealed class CharacterCatalogs
    {
        /// <summary>
        /// Groups the catalogs.
        /// </summary>
        /// <param name="stats">Stat catalog.</param>
        /// <param name="traits">Trait catalog.</param>
        /// <param name="skills">Skill catalog.</param>
        /// <param name="flags">Flag catalog.</param>
        /// <param name="reputationDomains">Reputation domain catalog.</param>
        /// <param name="healthConditions">Health condition catalog.</param>
        public CharacterCatalogs(
            StatCatalog stats,
            DefinitionCatalog<TraitDefinition> traits,
            DefinitionCatalog<SkillDefinition> skills,
            DefinitionCatalog<FlagDefinition> flags,
            DefinitionCatalog<ReputationDomainDefinition> reputationDomains,
            DefinitionCatalog<HealthConditionDefinition> healthConditions)
        {
            Stats = stats ?? throw new ArgumentNullException(nameof(stats));
            Traits = traits ?? throw new ArgumentNullException(nameof(traits));
            Skills = skills ?? throw new ArgumentNullException(nameof(skills));
            Flags = flags ?? throw new ArgumentNullException(nameof(flags));
            ReputationDomains = reputationDomains ?? throw new ArgumentNullException(nameof(reputationDomains));
            HealthConditions = healthConditions ?? throw new ArgumentNullException(nameof(healthConditions));
        }

        /// <summary>Stat catalog.</summary>
        public StatCatalog Stats { get; }

        /// <summary>Trait catalog.</summary>
        public DefinitionCatalog<TraitDefinition> Traits { get; }

        /// <summary>Skill catalog.</summary>
        public DefinitionCatalog<SkillDefinition> Skills { get; }

        /// <summary>Flag catalog.</summary>
        public DefinitionCatalog<FlagDefinition> Flags { get; }

        /// <summary>Reputation domain catalog.</summary>
        public DefinitionCatalog<ReputationDomainDefinition> ReputationDomains { get; }

        /// <summary>Health condition catalog.</summary>
        public DefinitionCatalog<HealthConditionDefinition> HealthConditions { get; }
    }
}
