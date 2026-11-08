namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// Read-only access to current stat values.
    /// </summary>
    public interface IStatReader
    {
        /// <summary>
        /// Returns the current value of a stat (stored or derived).
        /// </summary>
        /// <param name="id">Stat id.</param>
        /// <returns>Value from 0 to 100.</returns>
        float GetValue(StatId id);
    }
}
