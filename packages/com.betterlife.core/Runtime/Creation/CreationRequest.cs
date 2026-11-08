using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Time;

namespace BetterLife.Core.Creation
{
    /// <summary>
    /// What the player asked for. Unset optional fields are rolled from the seed; overrides are only allowed in Custom mode.
    /// </summary>
    public sealed class CreationRequest
    {
        /// <summary>
        /// Creates a request.
        /// </summary>
        /// <param name="seed">Life seed.</param>
        /// <param name="mode">Creation mode.</param>
        public CreationRequest(ulong seed, CreationMode mode)
        {
            Seed = seed;
            Mode = mode;
        }

        /// <summary>Life seed.</summary>
        public ulong Seed { get; }

        /// <summary>Creation mode.</summary>
        public CreationMode Mode { get; }

        /// <summary>Script of generated names.</summary>
        public NameScript NameScript { get; set; } = NameScript.Native;

        /// <summary>Given name override.</summary>
        public string GivenName { get; set; }

        /// <summary>Family name override.</summary>
        public string FamilyName { get; set; }

        /// <summary>Biological sex override.</summary>
        public BiologicalSex? Sex { get; set; }

        /// <summary>Gender identity override.</summary>
        public GenderIdentity? GenderIdentity { get; set; }

        /// <summary>Speech style override.</summary>
        public SpeechStyle? SpeechStyle { get; set; }

        /// <summary>Birth date override.</summary>
        public CalendarDate? BirthDate { get; set; }

        /// <summary>Birth hour override (0-23).</summary>
        public int? BirthHour { get; set; }

        /// <summary>Country override.</summary>
        public string CountryId { get; set; }

        /// <summary>City override (must belong to the country).</summary>
        public string CityId { get; set; }

        /// <summary>Family background override.</summary>
        public string FamilyBackgroundId { get; set; }

        /// <summary>Mother's personality override.</summary>
        public string MotherPersonalityId { get; set; }

        /// <summary>Father's personality override.</summary>
        public string FatherPersonalityId { get; set; }

        /// <summary>True when any override is set.</summary>
        public bool HasOverrides =>
            GivenName != null || FamilyName != null || Sex.HasValue || GenderIdentity.HasValue || SpeechStyle.HasValue ||
            BirthDate.HasValue || BirthHour.HasValue || CountryId != null || CityId != null || FamilyBackgroundId != null ||
            MotherPersonalityId != null || FatherPersonalityId != null;
    }
}
