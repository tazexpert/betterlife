using System.Linq;
using BetterLife.Core.Domain.World;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/countries.json. City ids must be unique across countries.
    /// </summary>
    public sealed class CountriesLoader : DefinitionLoader<CountryDefinition>
    {
        private readonly System.Collections.Generic.HashSet<string> _cityIds = new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal);

        /// <inheritdoc />
        public override string TypeName => ContentTypes.Countries;

        /// <inheritdoc />
        protected override string ItemsProperty => "countries";

        /// <inheritdoc />
        protected override void BeforeItems(System.Collections.Generic.IReadOnlyList<ContentDocument> documents, ContentLoadContext context)
        {
            _cityIds.Clear();
        }

        /// <inheritdoc />
        protected override CountryDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "weight", "modifiers", "cities");
            var (cities, citiesContext) = RuleJson.RequireArrayProperty(item, "cities", context);
            var country = new CountryDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                RuleJson.RequireFloat(item, "weight", context),
                ReadModifiers(item, context),
                cities.Select((token, i) =>
                {
                    RuleReadContext cityContext = citiesContext.Index(i);
                    JObject city = RuleJson.RequireObject(token, cityContext);
                    RuleJson.RequireOnly(city, cityContext, "id", "nameKey", "weight", "modifiers");
                    string cityId = RuleJson.RequireId(city, "id", cityContext);
                    if (!_cityIds.Add(cityId))
                    {
                        throw cityContext.Property("id").Error($"duplicate city id '{cityId}'.");
                    }
                    return new CityDefinition(cityId, RuleJson.RequireId(city, "nameKey", cityContext), RuleJson.RequireFloat(city, "weight", cityContext),
                        city["modifiers"] == null ? null : ReadModifiers(city, cityContext));
                }).ToList());
            return country;
        }

        private static LocationModifiers ReadModifiers(JObject parent, RuleReadContext context)
        {
            var (token, modifiersContext) = RuleJson.RequireProperty(parent, "modifiers", context);
            JObject modifiers = RuleJson.RequireObject(token, modifiersContext);
            RuleJson.RequireOnly(modifiers, modifiersContext, "costOfLiving", "education", "healthcare", "taxRate");
            float? Read(string name) => modifiers[name] == null ? (float?)null : RuleJson.RequireFloat(modifiers, name, modifiersContext);
            return new LocationModifiers(Read("costOfLiving"), Read("education"), Read("healthcare"), Read("taxRate"));
        }
    }
}
