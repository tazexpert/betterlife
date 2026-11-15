using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Education;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/education_programs.json into a program catalog and registers the "education" condition.
    /// Runs before rule types are final so activities, events and careers can use the condition.
    /// </summary>
    public sealed class EducationProgramsLoader : IContentTypeLoader
    {
        private static readonly string[] Dependencies = { ContentTypes.CharacterCatalogs };

        private readonly ItemsLoader _items = new ItemsLoader();

        /// <inheritdoc />
        public string TypeName => ContentTypes.EducationPrograms;

        /// <inheritdoc />
        public IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        public bool AcceptsFiles => true;

        /// <inheritdoc />
        public void Load(IReadOnlyList<ContentDocument> documents, ContentLoadContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            _items.Load(documents, context);
            var programs = context.Require<DefinitionCatalog<EducationProgramDefinition>>();
            if (!context.Rules.ConditionTypes.Contains(EducationConditionConverter.TypeName))
            {
                context.Rules.Register(new EducationConditionConverter(programs));
            }
        }

        private sealed class ItemsLoader : DefinitionLoader<EducationProgramDefinition>
        {
            public override string TypeName => ContentTypes.EducationPrograms;

            protected override string ItemsProperty => "programs";

            protected override EducationProgramDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
            {
                RuleJson.RequireOnly(item, context, "id", "nameKey", "rank", "years", "entryAge", "requiresRank", "compulsory", "autoEnroll",
                    "weeklyHours", "skills", "institutions");
                var (ageToken, ageContext) = RuleJson.RequireProperty(item, "entryAge", context);
                JObject age = RuleJson.RequireObject(ageToken, ageContext);
                RuleJson.RequireOnly(age, ageContext, "min", "max");
                var (institutions, institutionsContext) = RuleJson.RequireArrayProperty(item, "institutions", context);
                return new EducationProgramDefinition(
                    RuleJson.RequireId(item, "id", context),
                    RuleJson.RequireId(item, "nameKey", context),
                    RuleJson.RequireInt(item, "rank", context),
                    RuleJson.RequireInt(item, "years", context),
                    RuleJson.RequireInt(age, "min", ageContext),
                    ContentReading.RequireNullableInt(age, "max", ageContext),
                    RuleJson.RequireInt(item, "requiresRank", context),
                    ContentReading.RequireBool(item, "compulsory", context),
                    ContentReading.RequireBool(item, "autoEnroll", context),
                    RuleJson.RequireInt(item, "weeklyHours", context),
                    ContentReading.ReadSkillEffects(item, "skills", context),
                    institutions.Select((token, i) => ReadInstitution(RuleJson.RequireObject(token, institutionsContext.Index(i)), institutionsContext.Index(i))).ToList());
            }

            private static InstitutionDefinition ReadInstitution(JObject item, RuleReadContext context)
            {
                RuleJson.RequireOnly(item, context, "id", "nameKey", "prestige", "tuitionPerYear", "difficulty", "quality", "familyMinIncome");
                return new InstitutionDefinition(
                    RuleJson.RequireId(item, "id", context),
                    RuleJson.RequireId(item, "nameKey", context),
                    RuleJson.RequireFloat(item, "prestige", context),
                    RuleJson.RequireDecimal(item, "tuitionPerYear", context),
                    ContentReading.RequireNullableFloat(item, "difficulty", context),
                    RuleJson.RequireFloat(item, "quality", context),
                    ContentReading.RequireNullableInt(item, "familyMinIncome", context));
            }
        }
    }
}
