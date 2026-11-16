using System;

namespace BetterLife.Core.Avatars
{
    /// <summary>
    /// An age band of the avatar set and where its pictures are (a path prefix to which the two-digit face number is appended).
    /// </summary>
    public sealed class AvatarBand
    {
        /// <summary>
        /// Creates the band.
        /// </summary>
        /// <param name="minAge">First age of the band.</param>
        /// <param name="maxAge">Last age of the band, or null for no upper limit.</param>
        /// <param name="filePrefix">Path below the avatar root up to the face number, e.g. "age_36_45/images/age_36_45_".</param>
        public AvatarBand(int minAge, int? maxAge, string filePrefix)
        {
            if (minAge < 0 || (maxAge.HasValue && maxAge.Value < minAge))
            {
                throw new ArgumentOutOfRangeException(nameof(minAge), "The age range is invalid.");
            }
            if (string.IsNullOrWhiteSpace(filePrefix))
            {
                throw new ArgumentException("A file prefix is required.", nameof(filePrefix));
            }
            MinAge = minAge;
            MaxAge = maxAge;
            FilePrefix = filePrefix;
        }

        /// <summary>First age of the band.</summary>
        public int MinAge { get; }

        /// <summary>Last age of the band, or null for no upper limit.</summary>
        public int? MaxAge { get; }

        /// <summary>Path below the avatar root up to the face number.</summary>
        public string FilePrefix { get; }

        /// <summary>
        /// Whether an age falls in this band.
        /// </summary>
        /// <param name="age">Age in years.</param>
        /// <returns>True when covered.</returns>
        public bool Covers(int age) => age >= MinAge && (!MaxAge.HasValue || age <= MaxAge.Value);
    }
}
