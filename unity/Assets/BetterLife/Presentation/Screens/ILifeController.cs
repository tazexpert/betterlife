namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Replaces the running life (implemented by the composition root, which owns the life).
    /// </summary>
    public interface ILifeController
    {
        /// <summary>
        /// Continues the family line as a living child of the finished life.
        /// </summary>
        /// <param name="heirNpcId">NPC id of the child.</param>
        void ContinueAsHeir(int heirNpcId);

        /// <summary>
        /// Starts a new random life (a new family).
        /// </summary>
        void StartNewLife();
    }
}
