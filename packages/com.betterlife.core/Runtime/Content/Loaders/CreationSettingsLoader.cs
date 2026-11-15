using System.Collections.Generic;
using BetterLife.Core.Creation;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads the single content/creation_settings.json.
    /// </summary>
    public sealed class CreationSettingsLoader : SingleDocumentLoader
    {
        private static readonly string[] NoDependencies = new string[0];

        /// <inheritdoc />
        public override string TypeName => ContentTypes.CreationSettings;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => NoDependencies;

        /// <inheritdoc />
        protected override void Read(JObject root, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(root, context, "birthYear", "parentAgeAtBirth", ContentReading.SchemaProperty);
            var (years, yearsContext) = Range(root, "birthYear", context);
            var (ages, agesContext) = Range(root, "parentAgeAtBirth", context);
            content.Add(new CreationSettings(
                RuleJson.RequireInt(years, "min", yearsContext),
                RuleJson.RequireInt(years, "max", yearsContext),
                RuleJson.RequireInt(ages, "min", agesContext),
                RuleJson.RequireInt(ages, "max", agesContext)));
        }

        private static (JObject Range, RuleReadContext Context) Range(JObject root, string name, RuleReadContext context)
        {
            var (token, rangeContext) = RuleJson.RequireProperty(root, name, context);
            JObject range = RuleJson.RequireObject(token, rangeContext);
            RuleJson.RequireOnly(range, rangeContext, "min", "max");
            return (range, rangeContext);
        }
    }
}
