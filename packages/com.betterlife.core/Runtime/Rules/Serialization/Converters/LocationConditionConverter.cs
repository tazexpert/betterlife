using BetterLife.Core.Domain.World;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"location","country":id?,"city":id?}; the city must belong to the country when both are given.
    /// </summary>
    public sealed class LocationConditionConverter : ConditionConverter<LocationCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "location";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override LocationCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "country", "city");
            string country = RuleJson.OptionalId(json, "country", context);
            string city = RuleJson.OptionalId(json, "city", context);
            var countries = context.RequireCatalogs().Countries;
            if (country != null && !countries.Contains(country))
            {
                throw context.Property("country").Error($"unknown country '{country}'.");
            }
            if (city != null)
            {
                bool cityExists = country != null
                    ? countries.Get(country).FindCity(city) != null
                    : System.Linq.Enumerable.Any(countries.All, c => c.FindCity(city) != null);
                if (!cityExists)
                {
                    throw context.Property("city").Error(country != null ? $"city '{city}' is not in '{country}'." : $"unknown city '{city}'.");
                }
            }
            return new LocationCondition(country, city);
        }

        /// <inheritdoc />
        protected override void WriteTyped(LocationCondition condition, JObject json, RuleRegistry registry)
        {
            if (condition.CountryId != null)
            {
                json["country"] = condition.CountryId;
            }
            if (condition.CityId != null)
            {
                json["city"] = condition.CityId;
            }
        }
    }
}
