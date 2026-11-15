using System.Collections.Generic;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Family;
using BetterLife.Core.Domain.Flags;
using BetterLife.Core.Domain.Life;
using BetterLife.Core.Domain.Skills;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Domain.Traits;
using BetterLife.Core.Domain.World;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Combines the base catalogs into <see cref="CharacterCatalogs"/>. Takes no files; later content types
    /// (events, careers...) depend on it so their rules can validate referenced ids.
    /// </summary>
    public sealed class CharacterCatalogsBuilder : IContentTypeLoader
    {
        private static readonly string[] Dependencies =
        {
            ContentTypes.Stats, ContentTypes.Traits, ContentTypes.Skills, ContentTypes.Flags,
            ContentTypes.ReputationDomains, ContentTypes.HealthConditions, ContentTypes.Countries, ContentTypes.FamilyBackgrounds, ContentTypes.LifeStages,
        };

        /// <inheritdoc />
        public string TypeName => ContentTypes.CharacterCatalogs;

        /// <inheritdoc />
        public IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        public bool AcceptsFiles => false;

        /// <inheritdoc />
        public void Load(IReadOnlyList<ContentDocument> documents, ContentLoadContext context)
        {
            context.Add(new CharacterCatalogs(
                context.Require<StatCatalog>(),
                context.Require<DefinitionCatalog<TraitDefinition>>(),
                context.Require<DefinitionCatalog<SkillDefinition>>(),
                context.Require<DefinitionCatalog<FlagDefinition>>(),
                context.Require<DefinitionCatalog<ReputationDomainDefinition>>(),
                context.Require<DefinitionCatalog<HealthConditionDefinition>>(),
                context.Require<DefinitionCatalog<CountryDefinition>>(),
                context.Require<DefinitionCatalog<FamilyBackgroundDefinition>>(),
                context.Require<LifeStageCatalog>()));
        }
    }
}
