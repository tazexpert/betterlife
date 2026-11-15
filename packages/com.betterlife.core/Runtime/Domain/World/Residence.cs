namespace BetterLife.Core.Domain.World
{
    /// <summary>
    /// Where a character currently lives.
    /// </summary>
    public sealed class Residence
    {
        /// <summary>
        /// Creates a residence.
        /// </summary>
        /// <param name="countryId">Country id.</param>
        /// <param name="cityId">City id, or null when only the country is known.</param>
        public Residence(string countryId, string cityId)
        {
            CountryId = ContentId.Require(countryId, nameof(countryId));
            CityId = cityId == null ? null : ContentId.Require(cityId, nameof(cityId));
        }

        /// <summary>Country id.</summary>
        public string CountryId { get; }

        /// <summary>City id, or null.</summary>
        public string CityId { get; }
    }
}
