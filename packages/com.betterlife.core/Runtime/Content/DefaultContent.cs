using System.Collections.Generic;
using BetterLife.Core.Content.Loaders;
using BetterLife.Core.Rules.Serialization;

namespace BetterLife.Core.Content
{
    /// <summary>
    /// The standard set of content loaders. Later systems append their own loaders to this list.
    /// </summary>
    public static class DefaultContent
    {
        /// <summary>
        /// Creates the built-in loaders.
        /// </summary>
        /// <returns>Loaders for every built-in content type.</returns>
        public static List<IContentTypeLoader> CreateLoaders()
        {
            return new List<IContentTypeLoader>
            {
                new StatsLoader(new IStatFormulaReader[] { new WeightedStatFormulaReader() }),
                new TraitsLoader(),
                new SkillsLoader(),
                new FlagsLoader(),
                new ReputationDomainsLoader(),
                new HealthConditionsLoader(),
                new CharacterCatalogsBuilder(),
            };
        }

        /// <summary>
        /// Creates a pipeline with the built-in loaders and rule types.
        /// </summary>
        /// <param name="source">Where files are read from.</param>
        /// <returns>The pipeline.</returns>
        public static ContentPipeline CreatePipeline(IContentSource source)
        {
            return new ContentPipeline(source, DefaultRules.CreateRegistry(), CreateLoaders());
        }
    }
}
