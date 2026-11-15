using System.Linq;
using BetterLife.Core.Achievements;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Genetics;
using BetterLife.Core.Economy;
using BetterLife.Core.Events;
using BetterLife.Core.Legacy;
using BetterLife.Core.Localization;
using BetterLife.Core.Persistence;
using BetterLife.Core.Relationships;
using BetterLife.Core.Saving;
using BetterLife.Core.Sessions;
using BetterLife.Core.Tests.Dashboard;
using BetterLife.Core.Tests.Saving;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.ViewModels;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Legacy
{
    /// <summary>Tests for inheritance, heirs, the family tree, the life summary and achievements.</summary>
    [TestFixture]
    public class LegacyTests
    {
        private static readonly LegacySettings Settings = new LegacySettings(100000m, 0.1m, 0.5f, 0.25f, 60f);

        [Test]
        public void Estate_PaysDebts_ThenTax_ThenSplits()
        {
            InheritanceReport rich = InheritanceReport.Settle(500000m, 100000m, 2, Settings);
            Assert.That(rich.NetEstate, Is.EqualTo(400000m));
            Assert.That(rich.Tax, Is.EqualTo(30000m), "10% of what is above the 100,000 exemption");
            Assert.That(rich.Received, Is.EqualTo(185000m));

            InheritanceReport small = InheritanceReport.Settle(80000m, 0m, 1, Settings);
            Assert.That(small.Tax, Is.EqualTo(0m));
            Assert.That(small.Received, Is.EqualTo(80000m));

            InheritanceReport broke = InheritanceReport.Settle(10000m, 50000m, 3, Settings);
            Assert.That(broke.Debts, Is.EqualTo(10000m), "only what the estate can cover is paid");
            Assert.That(broke.Received, Is.EqualTo(0m), "heirs never take on debts beyond the estate");
        }

        [Test]
        public void Heir_ContinuesTheFamilyLine()
        {
            using (LifeSession parent = FinishedLife(out int heirId, out int siblingId))
            {
                Npc heirNpc = parent.Character.Relationships.Npc(heirId);
                var legacy = new LegacyService(SaveFixture.Content);
                Assert.That(legacy.Heirs(parent).Select(r => r.NpcId), Is.EquivalentTo(new[] { heirId, siblingId }));

                using (LifeSession heir = legacy.ContinueAs(parent, heirId, new EventBus(), out InheritanceReport report))
                {
                    Character c = heir.Character;
                    GameDate died = parent.Character.Death.Week;
                    Assert.That(c.Name.GivenName, Is.EqualTo(heirNpc.Name.GivenName));
                    Assert.That(c.Generation, Is.EqualTo(2));
                    Assert.That(heir.TimeState.Clock.Now, Is.EqualTo(died), "time continues from the death");
                    Assert.That(c.AgeAt(died), Is.EqualTo(heirNpc.AgeAt(died)));
                    Assert.That(heir.Economy.State.PriceLevel, Is.EqualTo(parent.Economy.State.PriceLevel), "same economy");

                    Assert.That(report.Heirs, Is.EqualTo(2));
                    Assert.That(report.Debts, Is.EqualTo(0m));
                    Assert.That(report.Tax, Is.EqualTo(Money.Round((report.Assets - 100000m) * 0.1m)));
                    Assert.That(report.Received, Is.EqualTo(Money.Round((report.Assets - report.Tax) / 2m)));
                    Assert.That(c.Wallet.Cash, Is.EqualTo(report.Received));
                    Assert.That(c.Karma.Good, Is.EqualTo(parent.Character.Karma.Good * 0.25f).Within(0.01f), "karma share of the test settings");
                    Assert.That(c.Flags.GetCount("vice.wins"), Is.EqualTo(3), "inheritable counter");
                    Assert.That(c.Flags.IsSet("career.fired_once"), Is.False, "not inheritable");

                    Relationship mother = c.Relationships.All.Single(r => r.Type == RelationshipType.Parent && !c.Relationships.Npc(r.NpcId).IsAlive);
                    Assert.That(c.Relationships.Npc(mother.NpcId).Name.GivenName, Is.EqualTo(parent.Character.Name.GivenName));
                    Assert.That(c.Relationships.All.Count(r => r.Type == RelationshipType.Parent), Is.EqualTo(2), "the surviving spouse is the other parent");
                    Assert.That(c.Relationships.All.Count(r => r.Type == RelationshipType.Sibling), Is.EqualTo(1));
                    Assert.That(c.Education.Credentials.Select(x => x.ProgramId), Does.Contain("primary"), "a grown heir has the compulsory schooling");

                    Assert.That(heir.Lineage.Ancestors.Single().GivenName, Is.EqualTo(parent.Character.Name.GivenName));
                    Assert.That(heir.Lineage.NextGeneration, Is.EqualTo(2));
                    SaveFixture.Advance(heir, 2);
                    Assert.That(heir.TimeState.Clock.Now.TotalWeeks, Is.GreaterThan(died.TotalWeeks), "the heir's life runs");
                }
            }
        }

        [Test]
        public void Heir_IsDeterministic_AndSurvivesASave()
        {
            using (LifeSession parent = FinishedLife(out int heirId, out _))
            {
                var legacy = new LegacyService(SaveFixture.Content);
                using (LifeSession a = legacy.ContinueAs(parent, heirId, new EventBus(), out _))
                using (LifeSession b = legacy.ContinueAs(parent, heirId, new EventBus(), out _))
                {
                    Assert.That(SaveFixture.Snapshot(a), Is.EqualTo(SaveFixture.Snapshot(b)));

                    var serializer = new SaveSerializer();
                    string text = serializer.Serialize(LifeSaveMapper.ToDto(a, SaveFixture.Content.ContentVersion));
                    using (LifeSession loaded = LifeSessionFactory.Load(SaveFixture.Content, serializer.Deserialize(text).Save, new EventBus()).Session)
                    {
                        Assert.That(loaded.Character.Generation, Is.EqualTo(2));
                        Assert.That(loaded.Lineage.Ancestors.Single().NetWorth, Is.EqualTo(a.Lineage.Ancestors.Single().NetWorth));
                        Assert.That(SaveFixture.Snapshot(loaded), Is.EqualTo(SaveFixture.Snapshot(a)));
                    }
                }
            }
        }

        [Test]
        public void OnlyLivingChildrenOfAFinishedLife_AreHeirs()
        {
            using (LifeSession life = SaveFixture.NewLife())
            {
                Assert.That(HeirCreator.Heirs(life), Is.Empty, "still alive");
                Assert.Throws<System.InvalidOperationException>(() => new LegacyService(SaveFixture.Content).ContinueAs(life, 1, new EventBus(), out _));
            }
        }

        [Test]
        public void Achievements_UnlockOnce_AndPersistAcrossLives()
        {
            var storage = new InMemorySaveStorage();
            var catalog = SaveFixture.Content.Get<DefinitionCatalog<AchievementDefinition>>();
            var tracker = new AchievementTracker(catalog, new AchievementStore(storage), new SystemRealTimeClock());
            using (LifeSession life = SaveFixture.NewLife())
            {
                Assert.That(tracker.Evaluate(life), Is.Empty, "a newborn has nothing yet");
                while (life.Character.AgeAt(life.TimeState.Clock.Now) < 1)
                {
                    life.Advance(TimeSkip.Month);
                }
                Assert.That(tracker.Evaluate(life).Select(a => a.Id), Is.EqualTo(new[] { "first_birthday" }));
                Assert.That(tracker.Evaluate(life), Is.Empty, "never twice");
            }
            var again = new AchievementTracker(catalog, new AchievementStore(storage), new SystemRealTimeClock());
            Assert.That(again.Profile.Has("first_birthday"), Is.True, "kept in the profile slot");
            Assert.That(new SaveService(storage, new SaveSerializer(), SaveFixture.Content).Slots(), Is.Empty, "profile slots are not lives");

            using (LifeSession parent = FinishedLife(out int heirId, out _))
            using (LifeSession heir = new LegacyService(SaveFixture.Content).ContinueAs(parent, heirId, new EventBus(), out _))
            {
                Assert.That(again.Evaluate(heir).Select(a => a.Id), Does.Contain("second_generation"));
            }

            ILocalizer keys = DashboardFixture.Keys();
            var rows = AchievementsViewModel.Build(again, keys, out string progress);
            Assert.That(progress, Is.EqualTo(AchievementsViewModel.ProgressKey));
            Assert.That(rows.All(r => r.IsUnlocked), Is.True, "both test achievements are unlocked now");
            var fresh = AchievementsViewModel.Build(new AchievementTracker(catalog, new AchievementStore(new InMemorySaveStorage()), new SystemRealTimeClock()), keys, out _);
            Assert.That(fresh.Single(r => r.Id == "second_generation").Name, Is.EqualTo(AchievementsViewModel.SecretNameKey), "hidden until unlocked");
        }

        [Test]
        public void Summary_TellsTheStory_AndOffersTheHeirs()
        {
            ILocalizer keys = DashboardFixture.Keys();
            var money = new MoneyFormatter(keys);
            using (LifeSession alive = SaveFixture.NewLife())
            {
                Assert.That(LifeSummaryViewModel.Build(alive, null, Settings, keys, money), Is.Null);
            }
            using (LifeSession parent = FinishedLife(out int heirId, out _))
            {
                LifeSummaryState summary = LifeSummaryViewModel.Build(parent, null, Settings, keys, money);
                Assert.That(summary.Eulogy.First(), Is.EqualTo(LifeSummaryViewModel.OpeningKey));
                Assert.That(summary.Eulogy, Does.Contain(LifeSummaryViewModel.MarriedKey));
                Assert.That(summary.Eulogy, Does.Contain(LifeSummaryViewModel.ChildrenKey));
                Assert.That(summary.Eulogy, Does.Contain("eulogy.fired"), "flags with a eulogy line");
                Assert.That(summary.Heirs.Select(h => h.NpcId), Does.Contain(heirId));
                Assert.That(summary.ShareText, Does.StartWith(LifeSummaryViewModel.ShareKey));

                using (LifeSession heir = new LegacyService(SaveFixture.Content).ContinueAs(parent, heirId, new EventBus(), out _))
                {
                    var tree = FamilyTreeViewModel.Build(heir, keys, money);
                    Assert.That(tree.Count, Is.EqualTo(2));
                    Assert.That(tree.Last().Detail, Is.EqualTo(FamilyTreeViewModel.YouKey));
                }
            }
        }

        /// <summary>A 40-year-old who dies leaving a spouse, two adult children, money and some flags.</summary>
        private static LifeSession FinishedLife(out int heirId, out int siblingId)
        {
            LifeSession life = SaveFixture.NewLife();
            SaveFixture.Advance(life, 40);
            Character c = life.Character;
            GameDate now = life.TimeState.Clock.Now;
            foreach (int debt in c.Finances.Debts.Debts.Select(d => d.Id).ToList())
            {
                c.Finances.Debts.Remove(debt);
            }
            c.Wallet.Deposit(600000m);
            c.Flags.Increment("vice.wins", now, 3);
            c.Flags.Set("career.fired_once", now);
            c.Karma.AddGood(40f);
            c.Relationships.Add(id => new Npc(id, new PersonName("Partner", c.Name.FamilyName), BiologicalSex.Male, now.AddWeeks(-40 * 52), new Genome(), new string[0]),
                RelationshipType.Spouse, 70f, now);
            heirId = c.Relationships.Add(id => new Npc(id, new PersonName("Heir", c.Name.FamilyName), BiologicalSex.Female, now.AddWeeks(-20 * 52), new Genome(), new string[0]),
                RelationshipType.Child, 80f, now).NpcId;
            siblingId = c.Relationships.Add(id => new Npc(id, new PersonName("Sibling", c.Name.FamilyName), BiologicalSex.Male, now.AddWeeks(-18 * 52), new Genome(), new string[0]),
                RelationshipType.Child, 50f, now).NpcId;
            c.Die("accident", now);
            return life;
        }
    }
}
