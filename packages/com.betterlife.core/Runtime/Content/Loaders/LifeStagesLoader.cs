using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Life;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/life_stages.json into a <see cref="LifeStageCatalog"/>.
    /// </summary>
    public sealed class LifeStagesLoader : IContentTypeLoader
    {
        private static readonly string[] Dependencies = { ContentTypes.Stats, ContentTypes.HealthConditions };

        /// <inheritdoc />
        public string TypeName => ContentTypes.LifeStages;

        /// <inheritdoc />
        public IReadOnlyList<string> DependsOn => Dependencies;

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

            StatCatalog stats = context.Require<StatCatalog>();
            var conditions = context.Require<DefinitionCatalog<HealthConditionDefinition>>();
            var stages = new List<LifeStageDefinition>();
            int before = context.ErrorCount;
            foreach (ContentDocument document in documents)
            {
                RuleReadContext root = context.ReadContext();
                JArray items = null;
                context.Guard(document, root.Path, () =>
                {
                    RuleJson.RequireOnly(document.Root, root, "stages", ContentReading.SchemaProperty);
                    items = RuleJson.RequireArrayProperty(document.Root, "stages", root).Array;
                });
                if (items == null)
                {
                    continue;
                }
                RuleReadContext arrayContext = root.Property("stages");
                for (int i = 0; i < items.Count; i++)
                {
                    RuleReadContext itemContext = arrayContext.Index(i);
                    JToken token = items[i];
                    context.Guard(document, itemContext.Path, () => stages.Add(ReadStage(RuleJson.RequireObject(token, itemContext), itemContext, stats, conditions)));
                }
            }
            if (context.ErrorCount == before)
            {
                context.Guard(documents.FirstOrDefault(), RuleReadContext.RootPath, () => context.Add(new LifeStageCatalog(stages)));
            }
        }

        private static LifeStageDefinition ReadStage(JObject item, RuleReadContext context, StatCatalog stats, DefinitionCatalog<HealthConditionDefinition> conditions)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "minAge", "playerControlsTime", "extraAnnualMortality", "birthdayStats", "conditionRisks");
            var (risks, risksContext) = RuleJson.RequireArrayProperty(item, "conditionRisks", context);
            return new LifeStageDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                RuleJson.RequireInt(item, "minAge", context),
                ContentReading.RequireBool(item, "playerControlsTime", context),
                RuleJson.RequireFloat(item, "extraAnnualMortality", context),
                ContentReading.ReadStatAmounts(item, "birthdayStats", context, stats),
                risks.Select((token, i) =>
                {
                    RuleReadContext c = risksContext.Index(i);
                    JObject risk = RuleJson.RequireObject(token, c);
                    RuleJson.RequireOnly(risk, c, "condition", "annualChance");
                    string id = RuleJson.RequireId(risk, "condition", c);
                    if (!conditions.Contains(id))
                    {
                        throw c.Property("condition").Error($"unknown health condition '{id}'.");
                    }
                    return new ConditionRisk(id, RuleJson.RequireFloat(risk, "annualChance", c));
                }).ToList());
        }
    }
}
