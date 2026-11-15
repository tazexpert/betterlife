namespace BetterLife.Core.Creation
{
    /// <summary>
    /// A problem with a creation request, for the character creation screen.
    /// </summary>
    public sealed class CreationIssue
    {
        /// <summary>Overrides are not allowed in Random and Challenge modes.</summary>
        public const string OverridesNotAllowed = "creation.issue.overrides_not_allowed";
        /// <summary>Birth year outside the allowed range.</summary>
        public const string BirthYearOutOfRange = "creation.issue.birth_year_out_of_range";
        /// <summary>Birth hour outside 0-23.</summary>
        public const string BirthHourInvalid = "creation.issue.birth_hour_invalid";
        /// <summary>Unknown id.</summary>
        public const string UnknownId = "creation.issue.unknown_id";
        /// <summary>The city is not in the chosen country.</summary>
        public const string CityNotInCountry = "creation.issue.city_not_in_country";
        /// <summary>The name is empty or too long.</summary>
        public const string NameInvalid = "creation.issue.name_invalid";

        /// <summary>
        /// Creates an issue.
        /// </summary>
        /// <param name="field">Request field, for example "CityId".</param>
        /// <param name="messageKey">Localization key of the message.</param>
        public CreationIssue(string field, string messageKey)
        {
            Field = field;
            MessageKey = messageKey;
        }

        /// <summary>Request field.</summary>
        public string Field { get; }

        /// <summary>Localization key of the message.</summary>
        public string MessageKey { get; }
    }
}
