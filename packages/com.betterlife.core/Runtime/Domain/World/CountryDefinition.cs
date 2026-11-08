using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Domain.World
{
    /// <summary>
    /// A country and its cities, loaded from content/countries.json.
    /// </summary>
    public sealed class CountryDefinition : IDefinition
    {
        /// <summary>
        /// Creates a country.
        /// </summary>
        /// <param name="id">Country id.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="weight">Random selection weight; non-negative.</param>
        /// <param name="modifiers">Complete country modifiers.</param>
        /// <param name="cities">At least one city.</param>
        public CountryDefinition(string id, string nameKey, float weight, LocationModifiers modifiers, IEnumerable<CityDefinition> cities)
        {
            if (!(weight >= 0f) || float.IsInfinity(weight))
            {
                throw new ArgumentOutOfRangeException(nameof(weight), weight, "Weight must be non-negative.");
            }
            if (modifiers == null || !modifiers.IsComplete)
            {
                throw new ArgumentException("Country modifiers must define every field.", nameof(modifiers));
            }
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            Weight = weight;
            Modifiers = modifiers;
            Cities = (cities ?? Enumerable.Empty<CityDefinition>()).ToList();
            if (Cities.Count == 0 || Cities.Any(c => c == null))
            {
                throw new ArgumentException("A country needs at least one city.", nameof(cities));
            }
            if (Cities.Select(c => c.Id).Distinct(StringComparer.Ordinal).Count() != Cities.Count)
            {
                throw new ArgumentException("City ids must be unique.", nameof(cities));
            }
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Random selection weight.</summary>
        public float Weight { get; }

        /// <summary>Complete country modifiers.</summary>
        public LocationModifiers Modifiers { get; }

        /// <summary>Cities.</summary>
        public IReadOnlyList<CityDefinition> Cities { get; }

        /// <summary>
        /// Returns a city by id, or null.
        /// </summary>
        /// <param name="cityId">City id.</param>
        /// <returns>The city or null.</returns>
        public CityDefinition FindCity(string cityId) => Cities.FirstOrDefault(c => c.Id == cityId);

        /// <summary>
        /// Returns the effective modifiers in a city (null for the country average).
        /// </summary>
        /// <param name="cityId">City id, or null.</param>
        /// <returns>Complete modifiers.</returns>
        public LocationModifiers ModifiersIn(string cityId)
        {
            CityDefinition city = cityId == null ? null : FindCity(cityId);
            return city == null ? Modifiers : city.Modifiers.Over(Modifiers);
        }
    }
}
