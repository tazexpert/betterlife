using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Events;
using BetterLife.Core.Persistence;
using BetterLife.Core.Saving;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time.Ticking;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Saves real-content lives in the middle and checks that the loaded copy plays out exactly like the original.
    /// </summary>
    [TestFixture]
    public class SaveSimulationTests
    {
        private const int Lives = 5;
        private const int MonthsBeforeSave = 25 * 12;
        private const int MonthsAfterSave = 10 * 12;

        [Test]
        public void RealLives_ContinueIdenticallyAfterSaveAndLoad()
        {
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            var serializer = new SaveSerializer();
            int largest = 0;
            for (int i = 0; i < Lives; i++)
            {
                CreationResult creation = new CharacterCreator(content).Create(new CreationRequest((ulong)(6600 + i * 19), CreationMode.Random));
                using (LifeSession original = LifeSessionFactory.CreateNew(content, creation, new EventBus()))
                {
                    Play(original, MonthsBeforeSave);
                    var storage = new InMemorySaveStorage();
                    var saves = new SaveService(storage, serializer, content);
                    saves.Save(original, "test");
                    largest = System.Math.Max(largest, storage.Read("test").Length);
                    LifeSessionLoadResult loaded = saves.Load("test", new EventBus());
                    using (LifeSession copy = loaded.Session)
                    {
                        Assert.That(loaded.Warnings, Is.Empty);
                        Play(original, MonthsAfterSave);
                        Play(copy, MonthsAfterSave);
                        Assert.That(Text(copy, content, serializer), Is.EqualTo(Text(original, content, serializer)), $"life {i} diverged after loading");
                    }
                }
            }
            TestContext.WriteLine($"Largest save at age 25: {largest / 1024} KB");
        }

        private static void Play(LifeSession life, int months)
        {
            for (int month = 0; month < months && !life.IsOver; month++)
            {
                life.Advance(TimeSkip.Month);
                if (!life.IsOver)
                {
                    CareerSimulationTests.Decide(life);
                }
            }
        }

        private static string Text(LifeSession life, IContentRepository content, SaveSerializer serializer)
        {
            return serializer.Serialize(LifeSaveMapper.ToDto(life, content.ContentVersion));
        }
    }
}
