using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/stats.json into a <see cref="StatCatalog"/>. Cross-checks (parents, formula inputs,
    /// weight sums) are done by the catalog itself.
    /// </summary>
    public sealed class StatsLoader : IContentTypeLoader
    {
        private const string StatsProperty = "stats";
        private const string FormulasProperty = "formulas";
        private static readonly string[] NoDependencies = new string[0];

        private static readonly Dictionary<string, StatLayer> Layers = new Dictionary<string, StatLayer>
        {
            { "core", StatLayer.Core },
            { "sub", StatLayer.Sub },
            { "hidden", StatLayer.Hidden },
        };

        private static readonly Dictionary<string, StatPolarity> Polarities = new Dictionary<string, StatPolarity>
        {
            { "higherIsBetter", StatPolarity.HigherIsBetter },
            { "lowerIsBetter", StatPolarity.LowerIsBetter },
        };

        private readonly Dictionary<string, IStatFormulaReader> _formulaReaders;

        /// <summary>
        /// Creates the loader.
        /// </summary>
        /// <param name="formulaReaders">Supported formula types.</param>
        public StatsLoader(IEnumerable<IStatFormulaReader> formulaReaders)
        {
            if (formulaReaders == null)
            {
                throw new ArgumentNullException(nameof(formulaReaders));
            }
            _formulaReaders = formulaReaders.ToDictionary(r => r.Type, StringComparer.Ordinal);
        }

        /// <inheritdoc />
        public string TypeName => ContentTypes.Stats;

        /// <inheritdoc />
        public IReadOnlyList<string> DependsOn => NoDependencies;

        /// <inheritdoc />
        public bool AcceptsFiles => true;

        /// <inheritdoc />
        public void Load(IReadOnlyList<ContentDocument> documents, ContentLoadContext context)
        {
            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents));
            }
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var definitions = new List<StatDefinition>();
            var formulas = new List<IStatFormula>();
            int errorsBefore = context.ErrorCount;
            foreach (ContentDocument document in documents)
            {
                RuleReadContext root = context.ReadContext();
                if (!context.Guard(document, root.Path, () => RuleJson.RequireOnly(document.Root, root, StatsProperty, FormulasProperty, ContentReading.SchemaProperty)))
                {
                    continue;
                }
                ReadArray(document, root, StatsProperty, context, (item, itemContext) => definitions.Add(ReadDefinition(item, itemContext)));
                ReadArray(document, root, FormulasProperty, context, (item, itemContext) => formulas.Add(ReadFormula(item, itemContext)));
            }

            if (context.ErrorCount > errorsBefore)
            {
                return;
            }

            ContentDocument first = documents.FirstOrDefault();
            context.Guard(first, RuleReadContext.RootPath, () => context.Add(new StatCatalog(definitions, formulas)));
        }

        private static void ReadArray(ContentDocument document, RuleReadContext root, string property, ContentLoadContext context, Action<JObject, RuleReadContext> read)
        {
            JArray items = null;
            if (!context.Guard(document, root.Path, () => items = RuleJson.RequireArrayProperty(document.Root, property, root).Array))
            {
                return;
            }

            RuleReadContext arrayContext = root.Property(property);
            for (int i = 0; i < items.Count; i++)
            {
                RuleReadContext itemContext = arrayContext.Index(i);
                JToken token = items[i];
                context.Guard(document, itemContext.Path, () => read(RuleJson.RequireObject(token, itemContext), itemContext));
            }
        }

        private static StatDefinition ReadDefinition(JObject item, RuleReadContext context)
        {
            RuleJson.RequireOnly(item, context, "id", "layer", "parent", "polarity", "default", "nameKey", "descKey", "decay");
            string parent = RuleJson.OptionalId(item, "parent", context);
            StatDecayRule decay = null;
            if (item["decay"] != null)
            {
                var (decayToken, decayContext) = RuleJson.RequireProperty(item, "decay", context);
                JObject decayObject = RuleJson.RequireObject(decayToken, decayContext);
                RuleJson.RequireOnly(decayObject, decayContext, "target", "ratePerWeek");
                decay = new StatDecayRule(RuleJson.RequireFloat(decayObject, "target", decayContext), RuleJson.RequireFloat(decayObject, "ratePerWeek", decayContext));
            }

            return new StatDefinition(
                new StatId(RuleJson.RequireId(item, "id", context)),
                ContentReading.RequireEnum(item, "layer", context, Layers),
                ContentReading.RequireEnum(item, "polarity", context, Polarities),
                RuleJson.RequireFloat(item, "default", context),
                RuleJson.RequireId(item, "nameKey", context),
                RuleJson.RequireId(item, "descKey", context),
                parent == null ? (StatId?)null : new StatId(parent),
                decay);
        }

        private IStatFormula ReadFormula(JObject item, RuleReadContext context)
        {
            string type = RuleJson.RequireString(item, RuleJson.TypeProperty, context);
            if (!_formulaReaders.TryGetValue(type, out IStatFormulaReader reader))
            {
                throw context.Property(RuleJson.TypeProperty).Error($"unknown formula type '{type}'.");
            }
            return reader.Read(item, context);
        }
    }
}
