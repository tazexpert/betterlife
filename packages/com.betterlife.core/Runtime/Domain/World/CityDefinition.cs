using System;

namespace BetterLife.Core.Domain.World
{
    /// <summary>
    /// A city within a country. Its modifiers override the country's.
    /// </summary>
    public sealed class CityDefinition
    {
        /// <summary>
        /// Creates a city.
        /// </summary>
        /// <param name="id">City id, unique across all countries (for example "thailand.bangkok").</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="weight">Random selection weight; non-negative.</param>
        /// <param name="modifiers">Overrides of the country modifiers.</param>
        public CityDefinition(string id, string nameKey, float weight, LocationModifiers modifiers)
        {
            if (!(weight >= 0f) || float.IsInfinity(weight))
            {
                throw new ArgumentOutOfRangeException(nameof(weight), weight, "Weight must be non-negative.");
            }
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            Weight = weight;
            Modifiers = modifiers ?? new LocationModifiers(null, null, null, null);
        }

        /// <summary>City id.</summary>
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Random selection weight.</summary>
        public float Weight { get; }

        /// <summary>Overrides of the country modifiers.</summary>
        public LocationModifiers Modifiers { get; }
    }
}
