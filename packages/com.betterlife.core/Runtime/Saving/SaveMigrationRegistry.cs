using System;
using System.Collections.Generic;

namespace BetterLife.Core.Saving
{
    /// <summary>
    /// Save migrations by the version they read. Adding a save version = adding a migration class and registering it here.
    /// </summary>
    public sealed class SaveMigrationRegistry
    {
        private readonly Dictionary<int, ISaveMigration> _byVersion = new Dictionary<int, ISaveMigration>();

        /// <summary>
        /// Creates the registry with the built-in migrations (none yet: version 1 is the first).
        /// </summary>
        /// <returns>The registry.</returns>
        public static SaveMigrationRegistry CreateDefault() => new SaveMigrationRegistry();

        /// <summary>
        /// Registers a migration.
        /// </summary>
        /// <param name="migration">Migration; one per source version.</param>
        /// <returns>This registry.</returns>
        public SaveMigrationRegistry Register(ISaveMigration migration)
        {
            if (migration == null)
            {
                throw new ArgumentNullException(nameof(migration));
            }
            if (_byVersion.ContainsKey(migration.FromVersion))
            {
                throw new ArgumentException($"A migration from version {migration.FromVersion} is already registered.", nameof(migration));
            }
            _byVersion.Add(migration.FromVersion, migration);
            return this;
        }

        /// <summary>
        /// Returns the migration that reads a version, or null.
        /// </summary>
        /// <param name="fromVersion">Version.</param>
        /// <returns>The migration.</returns>
        public ISaveMigration Find(int fromVersion) => _byVersion.TryGetValue(fromVersion, out ISaveMigration migration) ? migration : null;
    }
}
