using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Avatars;
using BetterLife.Core.Content;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Time;

namespace BetterLife.Core.Creation
{
    /// <summary>
    /// The values on the character creation screen. Starts from a random character, the player edits any field,
    /// and it becomes a custom <see cref="CreationRequest"/>.
    /// </summary>
    public sealed class CreationDraft
    {
        /// <summary>Given name.</summary>
        public string GivenName { get; set; }

        /// <summary>Family name.</summary>
        public string FamilyName { get; set; }

        /// <summary>Sex.</summary>
        public BiologicalSex Sex { get; set; }

        /// <summary>Birth year.</summary>
        public int Year { get; set; }

        /// <summary>Birth month 1-12.</summary>
        public int Month { get; set; }

        /// <summary>Birth day of the month.</summary>
        public int Day { get; set; }

        /// <summary>Birth hour 0-23.</summary>
        public int Hour { get; set; }

        /// <summary>Country id.</summary>
        public string CountryId { get; set; }

        /// <summary>City (province) id.</summary>
        public string CityId { get; set; }

        /// <summary>Family background id.</summary>
        public string FamilyBackgroundId { get; set; }

        /// <summary>Mother's parenting style id.</summary>
        public string MotherPersonalityId { get; set; }

        /// <summary>Father's parenting style id.</summary>
        public string FatherPersonalityId { get; set; }

        /// <summary>Avatar face, or null for the derived one.</summary>
        public int? AvatarFace { get; set; }

        /// <summary>True when the day exists in the month (e.g. not 31 February).</summary>
        public bool HasValidDate => Month >= 1 && Month <= 12 && Day >= 1 && Year > 0 && Day <= CalendarDate.DaysInMonth(Year, Month);

        /// <summary>
        /// A draft filled from a random character (what "random" would have created with this seed).
        /// </summary>
        /// <param name="content">Loaded content.</param>
        /// <param name="seed">Seed.</param>
        /// <returns>The draft.</returns>
        public static CreationDraft Random(IContentRepository content, ulong seed)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            Character c = new CharacterCreator(content).Create(new CreationRequest(seed, CreationMode.Random)).Character;
            IReadOnlyList<Domain.Family.ParentRecord> parents = c.Family.Parents;
            int face = AvatarFaces.Of(c);
            return new CreationDraft
            {
                GivenName = c.Name.GivenName,
                FamilyName = c.Name.FamilyName,
                Sex = c.Gender.Sex,
                Year = c.Birth.Date.Year,
                Month = c.Birth.Date.Month,
                Day = c.Birth.Date.Day,
                Hour = c.Birth.Hour,
                CountryId = c.Residence.CountryId,
                CityId = c.Residence.CityId,
                FamilyBackgroundId = c.Family.BackgroundId,
                MotherPersonalityId = parents.FirstOrDefault(p => p.Sex == BiologicalSex.Female)?.PersonalityId,
                FatherPersonalityId = parents.FirstOrDefault(p => p.Sex == BiologicalSex.Male)?.PersonalityId,
                AvatarFace = AvatarFaces.Optional(face),
            };
        }

        /// <summary>
        /// The custom request for these values (the date must be valid).
        /// </summary>
        /// <param name="seed">Seed for everything not on the form (genes, parents' names, the id).</param>
        /// <returns>The request.</returns>
        public CreationRequest ToRequest(ulong seed)
        {
            if (!HasValidDate)
            {
                throw new InvalidOperationException("The birth date is not valid.");
            }
            return new CreationRequest(seed, CreationMode.Custom)
            {
                GivenName = GivenName,
                FamilyName = FamilyName,
                Sex = Sex,
                BirthDate = new CalendarDate(Year, Month, Day),
                BirthHour = Hour,
                CountryId = CountryId,
                CityId = CityId,
                FamilyBackgroundId = FamilyBackgroundId,
                MotherPersonalityId = MotherPersonalityId,
                FatherPersonalityId = FatherPersonalityId,
                AvatarFace = AvatarFace,
            };
        }
    }
}
