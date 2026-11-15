using System.Collections.Generic;
using BetterLife.Core.Achievements;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads <c>achievements</c> files: <c>{ "achievements": [ { id, nameKey, descKey, hidden, condition } ] }</c>.
    /// </summary>
    public sealed class AchievementsLoader : DefinitionLoader<AchievementDefinition>
    {
        private static readonly string[] Dependencies = { ContentTypes.RuleTypes };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.Achievements;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override string ItemsProperty => "achievements";

        /// <inheritdoc />
        protected override AchievementDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "descKey", "hidden", "condition");
            return new AchievementDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                RuleJson.RequireId(item, "descKey", context),
                RuleJson.OptionalBool(item, "hidden", context) ?? false,
                content.Rules.ParseCondition(item["condition"], context.Property("condition")));
        }
    }
}
