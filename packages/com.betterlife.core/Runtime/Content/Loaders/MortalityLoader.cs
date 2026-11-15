using System.Collections.Generic;
using BetterLife.Core.Life;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads the single content/mortality.json into <see cref="MortalitySettings"/> and <see cref="MortalityModel"/>.
    /// </summary>
    public sealed class MortalityLoader : SingleDocumentLoader
    {
        private static readonly string[] NoDependencies = new string[0];

        /// <inheritdoc />
        public override string TypeName => ContentTypes.Mortality;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => NoDependencies;

        /// <inheritdoc />
        protected override void Read(JObject root, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(root, context, "gompertz", "maxAge", "health", "healthcare", "maxWeeklyProbability", ContentReading.SchemaProperty);
            var (gompertz, gc) = Section(root, "gompertz", context, "referenceAge", "annualAtReference", "growthPerYear");
            var (health, hc) = Section(root, "health", context, "multiplierAtZero", "multiplierAtFull");
            var (care, cc) = Section(root, "healthcare", context, "multiplierAtZero", "multiplierAtFull");
            var settings = new MortalitySettings(
                RuleJson.RequireInt(gompertz, "referenceAge", gc),
                RuleJson.RequireFloat(gompertz, "annualAtReference", gc),
                RuleJson.RequireFloat(gompertz, "growthPerYear", gc),
                RuleJson.RequireInt(root, "maxAge", context),
                RuleJson.RequireFloat(health, "multiplierAtZero", hc),
                RuleJson.RequireFloat(health, "multiplierAtFull", hc),
                RuleJson.RequireFloat(care, "multiplierAtZero", cc),
                RuleJson.RequireFloat(care, "multiplierAtFull", cc),
                RuleJson.RequireFloat(root, "maxWeeklyProbability", context));
            content.Add(settings);
            content.Add(new MortalityModel(settings));
        }

        private static (JObject Section, RuleReadContext Context) Section(JObject parent, string name, RuleReadContext context, params string[] allowed)
        {
            var (token, sectionContext) = RuleJson.RequireProperty(parent, name, context);
            JObject section = RuleJson.RequireObject(token, sectionContext);
            RuleJson.RequireOnly(section, sectionContext, allowed);
            return (section, sectionContext);
        }
    }
}
