using BetterLife.Core.Persistence;
using BetterLife.Core.Saving;
using BetterLife.Core.Sessions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Saving
{
    /// <summary>Tests for <see cref="SaveSerializer"/> and migrations.</summary>
    [TestFixture]
    public class SaveSerializerTests
    {
        private static LifeSaveDto Save()
        {
            using (LifeSession life = SaveFixture.NewLife())
            {
                return LifeSaveMapper.ToDto(life, SaveFixture.Content.ContentVersion);
            }
        }

        [Test]
        public void Envelope_HasFormatVersionAndChecksum()
        {
            JObject envelope = JObject.Parse(new SaveSerializer().Serialize(Save()));

            Assert.That((string)envelope["format"], Is.EqualTo(SaveFormat.FormatId));
            Assert.That((int)envelope["saveVersion"], Is.EqualTo(LifeSaveDto.CurrentVersion));
            Assert.That((string)envelope["checksum"], Has.Length.EqualTo(16));
            Assert.That(envelope["payload"]["Character"], Is.Not.Null);
        }

        [TestCase("")]
        [TestCase("{ not json")]
        [TestCase("{ \"format\": \"other.game\", \"saveVersion\": 1, \"payload\": {} }")]
        public void Garbage_IsReported(string text)
        {
            var ex = Assert.Throws<SaveLoadException>(() => new SaveSerializer().Deserialize(text));

            Assert.That(ex.Kind, Is.AnyOf(SaveErrorKind.Missing, SaveErrorKind.Corrupt));
        }

        [Test]
        public void ADamagedPayload_FailsTheChecksum()
        {
            string text = new SaveSerializer().Serialize(Save());
            JObject envelope = JObject.Parse(text);
            envelope["payload"]["PendingSocialHours"] = 999;

            var ex = Assert.Throws<SaveLoadException>(() => new SaveSerializer().Deserialize(envelope.ToString()));

            Assert.That(ex.Kind, Is.EqualTo(SaveErrorKind.Corrupt));
        }

        [Test]
        public void OlderSaves_AreMigratedStepByStep()
        {
            string v1 = new SaveSerializer().Serialize(Save());
            var migrations = new SaveMigrationRegistry().Register(new SetSocialHours(1, 7)).Register(new SetSocialHours(2, 11));

            SaveReadResult read = new SaveSerializer(migrations, 3).Deserialize(v1);

            Assert.That((read.OriginalVersion, read.WasMigrated, read.Save.SaveVersion), Is.EqualTo((1, true, 3)));
            Assert.That(read.Save.PendingSocialHours, Is.EqualTo(11), "both migrations ran in order");
        }

        [Test]
        public void MissingMigrations_AndNewerSaves_AreRefused()
        {
            string v1 = new SaveSerializer().Serialize(Save());
            string v2 = new SaveSerializer(new SaveMigrationRegistry(), 2).Serialize(Save());

            Assert.That(Assert.Throws<SaveLoadException>(() => new SaveSerializer(new SaveMigrationRegistry(), 2).Deserialize(v1)).Kind,
                Is.EqualTo(SaveErrorKind.NoMigration));
            Assert.That(Assert.Throws<SaveLoadException>(() => new SaveSerializer().Deserialize(v2)).Kind, Is.EqualTo(SaveErrorKind.TooNew));
            Assert.Throws<System.ArgumentException>(() => new SaveMigrationRegistry().Register(new SetSocialHours(1, 0)).Register(new SetSocialHours(1, 0)));
        }

        private sealed class SetSocialHours : ISaveMigration
        {
            private readonly int _hours;

            public SetSocialHours(int fromVersion, int hours)
            {
                FromVersion = fromVersion;
                _hours = hours;
            }

            public int FromVersion { get; }

            public void Migrate(JObject payload) => payload["PendingSocialHours"] = _hours;
        }
    }
}
