using System;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Domain.World
{
    /// <summary>
    /// Passes when the character lives in the given country and/or city.
    /// </summary>
    public sealed class LocationCondition : ICondition
    {
        /// <summary>Localization key of the description.</summary>
        public const string TextKey = "rule.cond.location";

        /// <summary>
        /// Creates the condition; at least one of the ids is required.
        /// </summary>
        /// <param name="countryId">Country id, or null.</param>
        /// <param name="cityId">City id, or null.</param>
        public LocationCondition(string countryId, string cityId)
        {
            if (countryId == null && cityId == null)
            {
                throw new ArgumentException("A country or a city is required.");
            }
            CountryId = countryId == null ? null : ContentId.Require(countryId, nameof(countryId));
            CityId = cityId == null ? null : ContentId.Require(cityId, nameof(cityId));
        }

        /// <summary>Country id, or null.</summary>
        public string CountryId { get; }

        /// <summary>City id, or null.</summary>
        public string CityId { get; }

        /// <inheritdoc />
        public bool Evaluate(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            Residence residence = context.Character.Residence;
            return (CountryId == null || residence.CountryId == CountryId) && (CityId == null || residence.CityId == CityId);
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            RuleText text = RuleText.Of(TextKey, (RuleTextKeys.ArgSubject, CityId ?? CountryId));
            return new ConditionTrace(Evaluate(context), text, true);
        }
    }
}
