using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.World;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/names.json: one name list per declared country.
    /// </summary>
    public sealed class NamesLoader : DefinitionLoader<NameList>
    {
        private static readonly string[] Dependencies = { ContentTypes.Countries };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.Names;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override string ItemsProperty => "countries";

        /// <inheritdoc />
        protected override NameList ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "country", "given", "family");
            string country = RuleJson.RequireId(item, "country", context);
            if (!content.Require<DefinitionCatalog<CountryDefinition>>().Contains(country))
            {
                throw context.Property("country").Error($"unknown country '{country}'.");
            }
            var (givenToken, givenContext) = RuleJson.RequireProperty(item, "given", context);
            JObject given = RuleJson.RequireObject(givenToken, givenContext);
            RuleJson.RequireOnly(given, givenContext, "female", "male");
            return new NameList(country, ReadNames(given, "female", givenContext), ReadNames(given, "male", givenContext), ReadNames(item, "family", context));
        }

        private static List<NameEntry> ReadNames(JObject parent, string name, RuleReadContext context)
        {
            var (array, arrayContext) = RuleJson.RequireArrayProperty(parent, name, context);
            return array.Select((token, i) =>
            {
                RuleReadContext c = arrayContext.Index(i);
                JObject entry = RuleJson.RequireObject(token, c);
                RuleJson.RequireOnly(entry, c, "native", "latin");
                return new NameEntry(RuleJson.RequireString(entry, "native", c), RuleJson.RequireString(entry, "latin", c));
            }).ToList();
        }
    }
}
