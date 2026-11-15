namespace BetterLife.Core.Careers
{
    /// <summary>
    /// The character's professional network, which helps job applications.
    /// </summary>
    public interface ICareerNetwork
    {
        /// <summary>
        /// Returns the hiring chance points the network adds for a track.
        /// </summary>
        /// <param name="track">Career track.</param>
        /// <returns>Chance points (0 or more).</returns>
        float HiringBonus(CareerTrackDefinition track);
    }
}
