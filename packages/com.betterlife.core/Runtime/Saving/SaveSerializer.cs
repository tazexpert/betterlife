using System;
using System.Globalization;
using System.IO;
using BetterLife.Core.Persistence;
using BetterLife.Core.Randomness;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Saving
{
    /// <summary>
    /// Turns a <see cref="LifeSaveDto"/> into save text (envelope with version and checksum) and back, running migrations
    /// for older versions.
    /// </summary>
    public sealed class SaveSerializer
    {
        private const string ChecksumFormat = "x16";

        private readonly SaveMigrationRegistry _migrations;
        private readonly int _currentVersion;
        private readonly JsonSerializer _json;

        /// <summary>
        /// Creates the serializer.
        /// </summary>
        /// <param name="migrations">Migrations, or null for the defaults.</param>
        /// <param name="currentVersion">Version written and read back (tests use a higher one to exercise migrations).</param>
        public SaveSerializer(SaveMigrationRegistry migrations = null, int currentVersion = LifeSaveDto.CurrentVersion)
        {
            if (currentVersion < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(currentVersion), currentVersion, "Version must be positive.");
            }
            _migrations = migrations ?? SaveMigrationRegistry.CreateDefault();
            _currentVersion = currentVersion;
            _json = JsonSerializer.Create(new JsonSerializerSettings
            {
                Culture = CultureInfo.InvariantCulture,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Include,
                FloatParseHandling = FloatParseHandling.Double,
                DateParseHandling = DateParseHandling.None,
            });
        }

        /// <summary>
        /// Writes a save.
        /// </summary>
        /// <param name="save">The saved life.</param>
        /// <returns>Save text.</returns>
        public string Serialize(LifeSaveDto save)
        {
            if (save == null)
            {
                throw new ArgumentNullException(nameof(save));
            }
            save.SaveVersion = _currentVersion;
            JObject payload = JObject.FromObject(save, _json);
            var envelope = new JObject
            {
                [SaveFormat.FormatProperty] = SaveFormat.FormatId,
                [SaveFormat.VersionProperty] = _currentVersion,
                [SaveFormat.ChecksumProperty] = Checksum(payload),
                [SaveFormat.PayloadProperty] = payload,
            };
            return envelope.ToString(Formatting.None);
        }

        /// <summary>
        /// Reads a save, verifying the checksum and upgrading older versions.
        /// </summary>
        /// <param name="text">Save text.</param>
        /// <returns>The save.</returns>
        /// <exception cref="SaveLoadException">The text is not a readable save.</exception>
        public SaveReadResult Deserialize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new SaveLoadException(SaveErrorKind.Missing, "The save is empty.");
            }
            JObject envelope;
            try
            {
                using (var reader = new JsonTextReader(new StringReader(text)) { DateParseHandling = DateParseHandling.None, FloatParseHandling = FloatParseHandling.Double })
                {
                    envelope = JObject.Load(reader);
                }
            }
            catch (JsonException ex)
            {
                throw new SaveLoadException(SaveErrorKind.Corrupt, "The save is not valid JSON.", ex);
            }
            if ((string)envelope[SaveFormat.FormatProperty] != SaveFormat.FormatId || !(envelope[SaveFormat.PayloadProperty] is JObject payload)
                || envelope[SaveFormat.VersionProperty]?.Type != JTokenType.Integer)
            {
                throw new SaveLoadException(SaveErrorKind.Corrupt, "Not a BetterLife save.");
            }
            if ((string)envelope[SaveFormat.ChecksumProperty] != Checksum(payload))
            {
                throw new SaveLoadException(SaveErrorKind.Corrupt, "The save checksum does not match (the file is damaged).");
            }

            int original = (int)envelope[SaveFormat.VersionProperty];
            if (original > _currentVersion)
            {
                throw new SaveLoadException(SaveErrorKind.TooNew, $"The save is version {original}; this game reads up to {_currentVersion}.");
            }
            for (int version = original; version < _currentVersion; version++)
            {
                ISaveMigration migration = _migrations.Find(version)
                    ?? throw new SaveLoadException(SaveErrorKind.NoMigration, $"No migration from save version {version}.");
                migration.Migrate(payload);
            }
            payload["SaveVersion"] = _currentVersion;

            try
            {
                return new SaveReadResult(payload.ToObject<LifeSaveDto>(_json), original);
            }
            catch (JsonException ex)
            {
                throw new SaveLoadException(SaveErrorKind.Invalid, "The save data does not match the expected structure.", ex);
            }
        }

        private static string Checksum(JObject payload) => StableHash.Fnv1a64(payload.ToString(Formatting.None)).ToString(ChecksumFormat, CultureInfo.InvariantCulture);
    }
}
