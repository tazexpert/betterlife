using System;
using BetterLife.Core.Domain.Characters;

namespace BetterLife.Core.Avatars
{
    /// <summary>
    /// One face of the avatar set. The same face number is drawn at every age band, so a person keeps their face while ageing.
    /// </summary>
    public sealed class AvatarFace
    {
        /// <summary>
        /// Creates the face.
        /// </summary>
        /// <param name="id">Face number used in the file names (e.g. 3 for "..._03").</param>
        /// <param name="sex">Sex the face is drawn as.</param>
        public AvatarFace(int id, BiologicalSex sex)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), id, "Face numbers are non-negative.");
            }
            Id = id;
            Sex = sex;
        }

        /// <summary>Face number used in the file names.</summary>
        public int Id { get; }

        /// <summary>Sex the face is drawn as.</summary>
        public BiologicalSex Sex { get; }
    }
}
