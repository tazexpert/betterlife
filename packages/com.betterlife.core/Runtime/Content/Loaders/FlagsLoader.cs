using System.Collections.Generic;
using BetterLife.Core.Domain.Flags;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/flags.json into a flag catalog.
    /// </summary>
    public sealed class FlagsLoader : DefinitionLoader<FlagDefinition>
    {
        private static readonly Dictionary<string, FlagKind> Kinds = new Dictionary<string, FlagKind>
        {
            { "bool", FlagKind.Bool },
            { "counter", FlagKind.Counter },
        };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.Flags;

        /// <inheritdoc />
        protected override string ItemsProperty => "flags";

        /// <inheritdoc />
        protected override FlagDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "kind", "defaultExpiryWeeks", "inheritable", "eulogyKey");
            return new FlagDefinition(
                RuleJson.RequireId(item, "id", context),
                ContentReading.RequireEnum(item, "kind", context, Kinds),
                ContentReading.RequireNullableInt(item, "defaultExpiryWeeks", context),
                ContentReading.RequireBool(item, "inheritable", context),
                ContentReading.RequireNullableId(item, "eulogyKey", context));
        }
    }
}
