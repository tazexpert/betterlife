using System.Linq;
using BetterLife.Core.Events;
using BetterLife.Core.Persistence;
using BetterLife.Core.Saving;
using BetterLife.Core.Sessions;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Saving
{
    /// <summary>Tests that a saved and loaded life continues exactly like the original.</summary>
    [TestFixture]
    public class SaveRoundTripTests
    {
        [Test]
        public void LoadedLife_ContinuesExactlyLikeTheOriginal()
        {
            using (LifeSession original = SaveFixture.NewLife())
            {
                SaveFixture.Advance(original, 22);
                original.Character.Wallet.Deposit(50000m);
                original.Vices.Indulge("betting");
                var storage = new InMemorySaveStorage();
                var saves = new SaveService(storage, new SaveSerializer(), SaveFixture.Content);
                saves.Save(original, "slot_1");

                LifeSessionLoadResult loaded = saves.Load("slot_1", new EventBus());
                using (LifeSession copy = loaded.Session)
                {
                    Assert.That(loaded.Warnings, Is.Empty);
                    Assert.That(SaveFixture.Snapshot(copy), Is.EqualTo(SaveFixture.Snapshot(original)), "identical right after loading");

                    SaveFixture.Advance(original, 8);
                    SaveFixture.Advance(copy, 8);

                    Assert.That(SaveFixture.Snapshot(copy), Is.EqualTo(SaveFixture.Snapshot(original)), "identical after eight more years");
                    Assert.That(copy.Character.Relationships.Npcs.Count, Is.EqualTo(original.Character.Relationships.Npcs.Count));
                }
            }
        }

        [Test]
        public void ModeAndIronman_AreSaved()
        {
            using (LifeSession original = SaveFixture.NewLife(ironman: true))
            {
                var saves = new SaveService(new InMemorySaveStorage(), new SaveSerializer(), SaveFixture.Content);
                saves.Autosave(original);

                using (LifeSession copy = saves.Load(SlotNames.Autosave, new EventBus()).Session)
                {
                    Assert.That(copy.IsIronman, Is.True);
                    Assert.That(copy.Mode, Is.EqualTo(original.Mode));
                    Assert.That(copy.Character.Relationships.Living(BetterLife.Core.Relationships.RelationshipType.Parent).Count(), Is.EqualTo(2),
                        "loading does not create a second family");
                }
            }
        }

        [Test]
        public void RemovedContent_IsDropped_WithWarnings()
        {
            using (LifeSession original = SaveFixture.NewLife())
            {
                LifeSaveDto dto = LifeSaveMapper.ToDto(original, SaveFixture.Content.ContentVersion);
                dto.Character.Finances.Holdings.Add(new HoldingDto { Id = 9, AssetId = "yacht", Value = "100", CostBasis = "100", AcquiredWeek = 0 });
                dto.Character.Vices.States.Add(new ViceStateDto { ViceId = "old_vice", NetWinnings = "0" });

                LifeSessionLoadResult loaded = LifeSessionFactory.Load(SaveFixture.Content, dto, new EventBus());
                using (loaded.Session)
                {
                    Assert.That(loaded.Warnings, Has.Some.Contains("yacht"));
                    Assert.That(loaded.Warnings, Has.Some.Contains("old_vice"));
                    Assert.That(loaded.Session.Character.Finances.Portfolio.Holdings, Is.Empty);
                }
            }
        }

        [Test]
        public void SummaryShowsWhoAndWhen()
        {
            using (LifeSession life = SaveFixture.NewLife())
            {
                SaveFixture.Advance(life, 3);
                SaveSummaryDto summary = new SaveService(new InMemorySaveStorage(), new SaveSerializer(), SaveFixture.Content).Save(life, "kid");

                Assert.That((summary.GivenName, summary.Week, summary.AgeYears, summary.Alive),
                    Is.EqualTo((life.Character.Name.GivenName, life.TimeState.Clock.Now.TotalWeeks, life.Character.AgeAt(life.TimeState.Clock.Now), true)));
            }
        }
    }
}
