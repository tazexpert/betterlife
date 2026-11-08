using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Genetics;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Domain.Traits;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads the single content/genetics.json into <see cref="GeneticsSettings"/> and <see cref="GeneticsService"/>.
    /// </summary>
    public sealed class GeneticsLoader : SingleDocumentLoader
    {
        private static readonly string[] Dependencies = { ContentTypes.CharacterCatalogs };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.Genetics;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override void Read(JObject root, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(root, context, "population", "stats", "traits", ContentReading.SchemaProperty);
            var (populationToken, populationContext) = RuleJson.RequireProperty(root, "population", context);
            JObject population = RuleJson.RequireObject(populationToken, populationContext);
            RuleJson.RequireOnly(population, populationContext, "mean", "sd");
            StatCatalog statCatalog = content.Require<StatCatalog>();

            var (stats, statsContext) = RuleJson.RequireArrayProperty(root, "stats", context);
            var (traits, traitsContext) = RuleJson.RequireArrayProperty(root, "traits", context);
            var settings = new GeneticsSettings(
                RuleJson.RequireFloat(population, "mean", populationContext),
                RuleJson.RequireFloat(population, "sd", populationContext),
                stats.Select((token, i) =>
                {
                    RuleReadContext c = statsContext.Index(i);
                    JObject stat = RuleJson.RequireObject(token, c);
                    RuleJson.RequireOnly(stat, c, "stat", "heritability", "noise");
                    return new HeritableStat(ContentReading.RequireStoredStat(stat, "stat", c, statCatalog), RuleJson.RequireFloat(stat, "heritability", c), RuleJson.RequireFloat(stat, "noise", c));
                }).ToList(),
                traits.Select((token, i) =>
                {
                    RuleReadContext c = traitsContext.Index(i);
                    JObject trait = RuleJson.RequireObject(token, c);
                    RuleJson.RequireOnly(trait, c, "trait", "prevalence", "heritability");
                    return new HeritableTrait(RuleJson.RequireTrait(trait, "trait", c), RuleJson.RequireFloat(trait, "prevalence", c), RuleJson.RequireFloat(trait, "heritability", c));
                }).ToList());

            content.Add(settings);
            content.Add(new GeneticsService(settings, content.Require<DefinitionCatalog<TraitDefinition>>()));
        }
    }
}
