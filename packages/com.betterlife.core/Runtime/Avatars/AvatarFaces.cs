using System;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Relationships;

namespace BetterLife.Core.Avatars
{
    /// <summary>
    /// Which face a person has: the player's chosen face, a person's fixed face, or one derived from a stable key.
    /// </summary>
    public static class AvatarFaces
    {
        /// <summary>Returned when no avatar set is loaded.</summary>
        public const int None = -1;

        /// <summary>
        /// Face number of the player character (chosen or derived).
        /// </summary>
        /// <param name="character">Player character.</param>
        /// <returns>The face, or <see cref="None"/>.</returns>
        public static int Of(Character character)
        {
            if (character == null)
            {
                throw new ArgumentNullException(nameof(character));
            }
            AvatarCatalog avatars = character.Catalogs.Avatars;
            return avatars == null ? None : avatars.FaceFor(character.Gender.Sex, character.AvatarFace, character.Id.Value);
        }

        /// <summary>
        /// Face number of a person in the character's life (fixed or derived from the character and NPC ids).
        /// </summary>
        /// <param name="owner">Player character.</param>
        /// <param name="npc">The person.</param>
        /// <returns>The face, or <see cref="None"/>.</returns>
        public static int Of(Character owner, Npc npc)
        {
            if (owner == null || npc == null)
            {
                throw new ArgumentNullException(owner == null ? nameof(owner) : nameof(npc));
            }
            AvatarCatalog avatars = owner.Catalogs.Avatars;
            return avatars == null ? None : avatars.FaceFor(npc.Sex, npc.AvatarFace, AvatarCatalog.NpcKey(owner, npc.Id));
        }

        /// <summary>
        /// A face number as an optional value (null for <see cref="None"/>).
        /// </summary>
        /// <param name="face">Face number.</param>
        /// <returns>The face, or null.</returns>
        public static int? Optional(int face) => face >= 0 ? face : (int?)null;
    }
}
