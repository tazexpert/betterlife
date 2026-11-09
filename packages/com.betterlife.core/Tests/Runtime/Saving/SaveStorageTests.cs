using System;
using System.IO;
using System.Linq;
using BetterLife.Core.Events;
using BetterLife.Core.Persistence;
using BetterLife.Core.Saving;
using BetterLife.Core.Sessions;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Saving
{
    /// <summary>Tests for the save storages and <see cref="SaveService"/> slots.</summary>
    [TestFixture]
    public class SaveStorageTests
    {
        private string _directory;

        [SetUp]
        public void SetUp() => _directory = Path.Combine(Path.GetTempPath(), "betterlife-saves-" + Guid.NewGuid().ToString("N"));

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, true);
            }
        }

        [Test]
        public void FileStorage_WritesReplacesListsAndDeletes()
        {
            var storage = new FileSaveStorage(_directory);

            storage.Write("slot_1", "first");
            storage.Write("slot_1", "second");
            storage.Write("autosave", "auto");

            Assert.That(storage.Read("slot_1"), Is.EqualTo("second"));
            Assert.That(storage.Slots(), Is.EqualTo(new[] { "autosave", "slot_1" }));
            Assert.That(Directory.GetFiles(_directory).Select(Path.GetFileName), Is.EquivalentTo(new[] { "autosave.json", "slot_1.json" }), "no temp files left");
            Assert.That(storage.Delete("slot_1"), Is.True);
            Assert.That(storage.Read("slot_1"), Is.Null);
            Assert.That(storage.Delete("slot_1"), Is.False);
        }

        [TestCase("../evil")]
        [TestCase("Slot")]
        [TestCase("")]
        [TestCase("a_name_that_is_far_too_long_for_a_slot")]
        public void InvalidSlotNames_AreRefused(string slot)
        {
            Assert.Throws<ArgumentException>(() => new InMemorySaveStorage().Write(slot, "x"));
            Assert.Throws<ArgumentException>(() => new FileSaveStorage(_directory).Write(slot, "x"));
        }

        [Test]
        public void Slots_ListReadableAndBrokenSaves_ButNotTheProfile()
        {
            var storage = new InMemorySaveStorage();
            var saves = new SaveService(storage, new SaveSerializer(), SaveFixture.Content);
            using (LifeSession life = SaveFixture.NewLife())
            {
                saves.Save(life, "good");
            }
            storage.Write("broken", "{ nope");
            storage.Write(SlotNames.UndoQuota, "{}");

            var slots = saves.Slots();

            Assert.That(slots.Select(s => s.Slot), Is.EqualTo(new[] { "broken", "good" }));
            Assert.That(slots.Single(s => s.Slot == "good").Readable, Is.True);
            Assert.That(slots.Single(s => s.Slot == "broken").Error, Is.EqualTo(SaveErrorKind.Corrupt));
            Assert.That(Assert.Throws<SaveLoadException>(() => saves.Load("missing", new EventBus())).Kind, Is.EqualTo(SaveErrorKind.Missing));
        }

        [Test]
        public void SavesFromNewerContent_AreRefused()
        {
            var storage = new InMemorySaveStorage();
            using (LifeSession life = SaveFixture.NewLife())
            {
                LifeSaveDto dto = LifeSaveMapper.ToDto(life, SaveFixture.Content.ContentVersion + 1);
                storage.Write("future", new SaveSerializer().Serialize(dto));
            }

            var ex = Assert.Throws<SaveLoadException>(() => new SaveService(storage, new SaveSerializer(), SaveFixture.Content).Load("future", new EventBus()));

            Assert.That(ex.Kind, Is.EqualTo(SaveErrorKind.TooNew));
        }
    }
}
