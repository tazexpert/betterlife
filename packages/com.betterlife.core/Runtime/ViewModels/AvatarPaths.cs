using System;
using BetterLife.Core.Avatars;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Relationships;
using BetterLife.Core.Time;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Resources paths of the avatar pictures shown on screens (null when the avatar set is not loaded).
    /// </summary>
    public static class AvatarPaths
    {
        /// <summary>
        /// Picture of the player character at a week.
        /// </summary>
        /// <param name="character">Player character.</param>
        /// <param name="now">Current week.</param>
        /// <returns>The path, or null.</returns>
        public static string Of(Character character, GameDate now)
        {
            int face = AvatarFaces.Of(character);
            return face == AvatarFaces.None ? null : character.Catalogs.Avatars.PathFor(face, character.AgeAt(now));
        }

        /// <summary>
        /// Picture of a person in the character's life at a week (a person who died keeps the age they died at).
        /// </summary>
        /// <param name="owner">Player character.</param>
        /// <param name="npc">The person.</param>
        /// <param name="now">Current week.</param>
        /// <returns>The path, or null.</returns>
        public static string Of(Character owner, Npc npc, GameDate now)
        {
            int face = AvatarFaces.Of(owner, npc);
            if (face == AvatarFaces.None)
            {
                return null;
            }
            GameDate at = npc.DiedOn.HasValue && npc.DiedOn.Value < now ? npc.DiedOn.Value : now;
            return owner.Catalogs.Avatars.PathFor(face, npc.AgeAt(at));
        }
    }
}
