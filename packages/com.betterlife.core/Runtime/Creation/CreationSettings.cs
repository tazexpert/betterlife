using System;
using BetterLife.Core.Time;

namespace BetterLife.Core.Creation
{
    /// <summary>
    /// Ranges used when creating a life, from content/creation_settings.json.
    /// </summary>
    public sealed class CreationSettings
    {
        /// <summary>
        /// Creates settings.
        /// </summary>
        /// <param name="minBirthYear">Earliest birth year (not before the game epoch).</param>
        /// <param name="maxBirthYear">Latest birth year.</param>
        /// <param name="minParentAge">Youngest parent age at the birth; positive.</param>
        /// <param name="maxParentAge">Oldest parent age at the birth.</param>
        public CreationSettings(int minBirthYear, int maxBirthYear, int minParentAge, int maxParentAge)
        {
            if (minBirthYear < GameCalendar.EpochYear || maxBirthYear < minBirthYear)
            {
                throw new ArgumentOutOfRangeException(nameof(maxBirthYear), maxBirthYear, $"Birth years must satisfy {GameCalendar.EpochYear} <= min <= max.");
            }
            if (minParentAge <= 0 || maxParentAge < minParentAge)
            {
                throw new ArgumentOutOfRangeException(nameof(maxParentAge), maxParentAge, "Parent ages must satisfy 0 < min <= max.");
            }
            MinBirthYear = minBirthYear;
            MaxBirthYear = maxBirthYear;
            MinParentAge = minParentAge;
            MaxParentAge = maxParentAge;
        }

        /// <summary>Earliest birth year.</summary>
        public int MinBirthYear { get; }

        /// <summary>Latest birth year.</summary>
        public int MaxBirthYear { get; }

        /// <summary>Youngest parent age at the birth.</summary>
        public int MinParentAge { get; }

        /// <summary>Oldest parent age at the birth.</summary>
        public int MaxParentAge { get; }
    }
}
