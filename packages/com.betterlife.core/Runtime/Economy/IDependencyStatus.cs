using BetterLife.Core.Time;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Tells whether the character still depends on their family (who then pays for school and covers shortfalls).
    /// </summary>
    public interface IDependencyStatus
    {
        /// <summary>
        /// Returns true once the character pays their own way.
        /// </summary>
        /// <param name="now">Week.</param>
        /// <returns>True when independent.</returns>
        bool IsIndependent(GameDate now);
    }
}
