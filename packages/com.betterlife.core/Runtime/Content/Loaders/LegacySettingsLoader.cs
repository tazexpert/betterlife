using System.Collections.Generic;
using BetterLife.Core.Legacy;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads the single <c>legacy_settings</c> file (inheritance tax and what an heir inherits).
    /// </summary>
    public sealed class LegacySettingsLoader : SingleDocumentLoader
    {
        private static readonly string[] Dependencies = new string[0];

        /// <inheritdoc />
        public override string TypeName => ContentTypes.LegacySettings;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override void Read(JObject root, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(root, context, "taxExemption", "taxRate", "reputationShare", "karmaShare", "heirEducationPerformance", ContentReading.SchemaProperty);
            content.Add(new LegacySettings(
                RuleJson.RequireDecimal(root, "taxExemption", context),
                RuleJson.RequireDecimal(root, "taxRate", context),
                RuleJson.RequireFloat(root, "reputationShare", context),
                RuleJson.RequireFloat(root, "karmaShare", context),
                RuleJson.RequireFloat(root, "heirEducationPerformance", context)));
        }
    }
}
