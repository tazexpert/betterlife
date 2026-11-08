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
        public CharacterCatalogs(
            StatCatalog stats,
            DefinitionCatalog<TraitDefinition> traits,
            DefinitionCatalog<SkillDefinition> skills,
            DefinitionCatalog<FlagDefinition> flags)
        {
            Stats = stats ?? throw new ArgumentNullException(nameof(stats));
            Traits = traits ?? throw new ArgumentNullException(nameof(traits));
            Skills = skills ?? throw new ArgumentNullException(nameof(skills));
            Flags = flags ?? throw new ArgumentNullException(nameof(flags));
        }

        /// <summary>Stat catalog.</summary>
        public StatCatalog Stats { get; }

        /// <summary>Trait catalog.</summary>
        public DefinitionCatalog<TraitDefinition> Traits { get; }

        /// <summary>Skill catalog.</summary>
        public DefinitionCatalog<SkillDefinition> Skills { get; }

        /// <summary>Flag catalog.</summary>
        public DefinitionCatalog<FlagDefinition> Flags { get; }
    }
}
