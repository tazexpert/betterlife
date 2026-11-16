using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Avatars;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// The avatar picker: the faces of the character's sex, drawn at the character's current age.
    /// </summary>
    public static class AvatarPickerViewModel
    {
        /// <summary>Screen title.</summary>
        public const string TitleKey = "avatar.title";
        /// <summary>Hint under the title.</summary>
        public const string HintKey = "avatar.hint";

        /// <summary>
        /// Builds the options (empty when no avatar set is loaded).
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <returns>The faces, in number order.</returns>
        public static IReadOnlyList<AvatarOption> Build(LifeSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            Character character = session.Character;
            AvatarCatalog avatars = character.Catalogs.Avatars;
            if (avatars == null)
            {
                return new AvatarOption[0];
            }
            GameDate now = session.TimeState.Clock.Now;
            int age = character.AgeAt(now);
            int current = AvatarFaces.Of(character);
            return avatars.FacesFor(character.Gender.Sex).Select(f => new AvatarOption(f.Id, avatars.PathFor(f.Id, age), f.Id == current)).ToList();
        }

        /// <summary>
        /// Chooses a face (only faces of the character's sex are accepted; it is cosmetic, so it needs no undo point).
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="faceId">Face number.</param>
        /// <returns>True when the face was set.</returns>
        public static bool Choose(LifeSession session, int faceId)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            Character character = session.Character;
            AvatarCatalog avatars = character.Catalogs.Avatars;
            if (avatars == null || !avatars.FacesFor(character.Gender.Sex).Any(f => f.Id == faceId))
            {
                return false;
            }
            character.SetAvatarFace(faceId);
            return true;
        }
    }
}
