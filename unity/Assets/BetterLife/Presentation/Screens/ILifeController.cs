using BetterLife.Core.Content;
using BetterLife.Core.Creation;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Starts lives: as an heir, a new random life, or a life the player designed. Implemented by the composition root.
    /// </summary>
    public interface ILifeController
    {
        /// <summary>Loaded content (for the creation screen).</summary>
        IContentRepository Content { get; }

        /// <summary>
        /// Continues the family line as a living child of the dead character.
        /// </summary>
        /// <param name="heirNpcId">NPC id of the child.</param>
        void ContinueAsHeir(int heirNpcId);

        /// <summary>Starts a new random life and family.</summary>
        void StartNewLife();

        /// <summary>
        /// Starts a life from the creation screen.
        /// </summary>
        /// <param name="draft">The values the player chose (must have no problems).</param>
        void StartNewLife(CreationDraft draft);

        /// <summary>
        /// A fresh random draft for the creation screen.
        /// </summary>
        /// <returns>The draft.</returns>
        CreationDraft NewDraft();
    }
}
