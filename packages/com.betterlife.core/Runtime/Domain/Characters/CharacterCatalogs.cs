using System;
using BetterLife.Core.Domain.Family;
using BetterLife.Core.Domain.Flags;
using BetterLife.Core.Domain.Life;
using BetterLife.Core.Domain.Skills;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Domain.Traits;
using BetterLife.Core.Domain.World;

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
        /// <param name="countries">Country catalog.</param>
        /// <param name="familyBackgrounds">Family background catalog.</param>
        /// <param name="lifeStages">Life stage catalog.</param>
        public CharacterCatalogs(
            StatCatalog stats,
            DefinitionCatalog<TraitDefinition> traits,
            DefinitionCatalog<SkillDefinition> skills,
            DefinitionCatalog<FlagDefinition> flags,
            DefinitionCatalog<ReputationDomainDefinition> reputationDomains,
            DefinitionCatalog<HealthConditionDefinition> healthConditions,
            DefinitionCatalog<CountryDefinition> countries,
            DefinitionCatalog<FamilyBackgroundDefinition> familyBackgrounds,
            LifeStageCatalog lifeStages)
        {
            Stats = stats ?? throw new ArgumentNullException(nameof(stats));
            Traits = traits ?? throw new ArgumentNullException(nameof(traits));
            Skills = skills ?? throw new ArgumentNullException(nameof(skills));
            Flags = flags ?? throw new ArgumentNullException(nameof(flags));
            ReputationDomains = reputationDomains ?? throw new ArgumentNullException(nameof(reputationDomains));
            HealthConditions = healthConditions ?? throw new ArgumentNullException(nameof(healthConditions));
            Countries = countries ?? throw new ArgumentNullException(nameof(countries));
            FamilyBackgrounds = familyBackgrounds ?? throw new ArgumentNullException(nameof(familyBackgrounds));
            LifeStages = lifeStages ?? throw new ArgumentNullException(nameof(lifeStages));
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

        /// <summary>Country catalog.</summary>
        public DefinitionCatalog<CountryDefinition> Countries { get; }

        /// <summary>Family background catalog.</summary>
        public DefinitionCatalog<FamilyBackgroundDefinition> FamilyBackgrounds { get; }

        /// <summary>Life stage catalog.</summary>
        public LifeStageCatalog LifeStages { get; }
    }
}
