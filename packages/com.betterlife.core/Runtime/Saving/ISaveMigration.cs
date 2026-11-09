using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Saving
{
    /// <summary>
    /// Upgrades a save payload by one version (from <see cref="FromVersion"/> to <see cref="FromVersion"/> + 1).
    /// </summary>
    public interface ISaveMigration
    {
        /// <summary>Version this migration reads.</summary>
        int FromVersion { get; }

        /// <summary>
        /// Rewrites the payload in place.
        /// </summary>
        /// <param name="payload">Payload JSON of version <see cref="FromVersion"/>.</param>
        void Migrate(JObject payload);
    }
}
