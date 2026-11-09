using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Events;
using BetterLife.Core.Relationships;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Relationships
{
    /// <summary>Tests for <see cref="RelationshipService"/>.</summary>
    [TestFixture]
    public class RelationshipServiceTests
    {
        [Test]
        public void InitializeFamily_TurnsParentsIntoNpcs_AndAddsSiblings_Once()
        {
            var f = new RelationshipFixture();

            f.Service.InitializeFamily();
            f.Service.InitializeFamily();

            var parents = f.People.Living(RelationshipType.Parent).ToList();
            Assert.That(parents.Select(p => p.Value), Is.EquivalentTo(new[] { 70f, 40f }));
            Assert.That(parents.Select(p => f.People.Npc(p.NpcId).Name.GivenName), Is.EquivalentTo(new[] { "Malee", "Somsak" }));
            Relationship sibling = f.People.Living(RelationshipType.Sibling).Single();
            Npc brother = f.People.Npc(sibling.NpcId);
            Assert.That(brother.Name.FamilyName, Is.EqualTo(f.Character.Name.FamilyName));
            Assert.That(System.Math.Abs(brother.BornOn.TotalWeeks - f.Character.Birth.Week.TotalWeeks), Is.LessThanOrEqualTo(4 * GameCalendar.WeeksPerYear));
            Assert.That(f.People.Npcs, Has.Count.EqualTo(3));
        }

        [Test]
        public void SocialTime_FeedsTheClosest_AndNeglectFadesTowardsTheFloor()
        {
            var f = new RelationshipFixture(family: false);
            Relationship friend = f.Add(RelationshipType.Friend, 30f);
            Relationship colleague = f.Add(RelationshipType.Colleague, 3f);

            f.Month(socialHours: 20);
            float fed = friend.Value;
            for (int i = 0; i < 40; i++)
            {
                f.Month();
            }

            Assert.That(fed, Is.EqualTo(30f + 20 * 0.6f / 2 * (1f - 0.3f) - 1.5f).Within(1e-3), "diminished gain, then the monthly fade");
            Assert.That(friend.Value, Is.EqualTo(0f), "friends fade to their floor");
            Assert.That(colleague.Value, Is.EqualTo(0f));
            Assert.That(friend.LastContact, Is.EqualTo(GameDate.FromYearWeek(2030, 0)));
        }

        [Test]
        public void Family_TimeTogether_IsDiminishedForCloseTies()
        {
            var f = new RelationshipFixture(2010);
            f.Service.InitializeFamily();
            f.Dependency.Independent = false;
            Relationship mother = f.People.Living(RelationshipType.Parent).First(r => r.Value == 70f);

            f.Month();

            Assert.That(mother.Value, Is.EqualTo(70f + 1f * (1f - 0.7f) - 0.5f).Within(1e-3));
        }

        [Test]
        public void MeetingPeople_AddsFriendsAndColleagues_AndPrunesTheWeakest()
        {
            var f = new RelationshipFixture(family: false);
            f.Character.Career.Start(new BetterLife.Core.Careers.Job("clerk", 0, f.Clock.Now, f.Clock.Now, 50f, 50f, 0, null));
            var met = new List<MetPersonEvent>();
            f.Bus.Subscribe<MetPersonEvent>(met.Add);

            for (int i = 0; i < 60; i++)
            {
                f.Month(socialHours: 40);
            }

            Assert.That(met.Select(m => m.Relationship.Type).Distinct(), Is.SupersetOf(new[] { RelationshipType.Friend, RelationshipType.Colleague }));
            Assert.That(met.Select(m => System.Math.Abs(m.Npc.AgeAt(m.Week) - f.Character.AgeAt(m.Week))), Is.All.LessThanOrEqualTo(6));
            Assert.That(f.People.All.Count, Is.LessThanOrEqualTo(f.Settings.MaxRelationships));
        }

        [Test]
        public void Compatibility_ShowsEveryContribution()
        {
            var f = new RelationshipFixture(family: false);
            f.Character.Traits.Add("anxious", f.Clock.Now);
            f.Character.Traits.Add("athletic", f.Clock.Now);
            Relationship match = f.Add(RelationshipType.Friend, 50f, age: 33, traits: new[] { "athletic", "calm" });

            CompatibilityBreakdown breakdown = f.Service.Compatibility(match.NpcId);

            var lines = breakdown.Lines.ToDictionary(l => l.Key, l => l.Points);
            Assert.That(lines[CompatibilityBreakdown.SharedTraitsKey], Is.EqualTo(8f));
            Assert.That(lines[CompatibilityBreakdown.ConflictingTraitsKey], Is.EqualTo(-12f));
            Assert.That(lines[CompatibilityBreakdown.AgeGapKey], Is.EqualTo(-2f * 4));
            Assert.That(breakdown.Score, Is.EqualTo(50f + 8f - 12f - 8f).Within(1e-3));
        }

        [Test]
        public void Quote_ListsEveryReason()
        {
            var f = new RelationshipFixture(family: false);
            Relationship acquaintance = f.Add(RelationshipType.Friend, 10f);
            Relationship close = f.Add(RelationshipType.Friend, 60f);
            Relationship partner = f.Add(RelationshipType.Partner, 80f);
            Relationship gone = f.Add(RelationshipType.Friend, 50f);
            f.People.Npc(gone.NpcId).Die(f.Clock.Now);

            Assert.That(f.Service.Quote(acquaintance.NpcId, "ask_out").ReasonKeys, Is.EquivalentTo(new[] { InteractionQuote.ReasonTooLow, InteractionQuote.ReasonCondition }));
            Assert.That(f.Service.Quote(close.NpcId, "propose").ReasonKeys, Is.EquivalentTo(new[] { InteractionQuote.ReasonType }));
            Assert.That(f.Service.Quote(close.NpcId, "gift").ReasonKeys, Is.EquivalentTo(new[] { InteractionQuote.ReasonCash }));
            Assert.That(f.Service.Quote(gone.NpcId, "chat").ReasonKeys, Does.Contain(InteractionQuote.ReasonDead));
            Assert.That(f.Service.Quote(partner.NpcId, "propose").Available, Is.True);
            Assert.That(f.Service.Options(partner.NpcId).Select(o => o.Interaction.Id), Is.EquivalentTo(new[] { "chat", "gift", "propose", "try_for_baby", "adopt" }));
        }

        [Test]
        public void Gift_CostsCash_AddsAMemory_AndHasACooldown()
        {
            var f = new RelationshipFixture(family: false);
            Relationship friend = f.Add(RelationshipType.Friend, 20f);
            f.Character.Wallet.Deposit(5000m);

            InteractionResult result = f.Service.Interact(friend.NpcId, "gift");

            Assert.That((result.Performed, result.Success, result.Roll), Is.EqualTo((true, true, (BetterLife.Core.Luck.ChanceRoll)null)));
            Assert.That(friend.Value, Is.EqualTo(25f));
            Assert.That(f.Character.Wallet.Cash, Is.EqualTo(4000m));
            Assert.That(friend.Memories.Single().Key, Is.EqualTo("memory.gift"));
            Assert.That(f.Service.Quote(friend.NpcId, "gift").ReasonKeys, Is.EquivalentTo(new[] { InteractionQuote.ReasonCooldown }));
            Assert.That(f.Service.Interact(friend.NpcId, "gift").Performed, Is.False);
        }

        [Test]
        public void AskingOut_SucceedsAtAboutTheQuotedRate_AndMakesAPartner()
        {
            int partners = 0;
            float chance = 0f;
            for (ulong seed = 1; seed <= 200; seed++)
            {
                var f = new RelationshipFixture(seed: seed, family: false);
                Relationship friend = f.Add(RelationshipType.Friend, 50f);
                var changes = new List<RelationshipTypeChangedEvent>();
                f.Bus.Subscribe<RelationshipTypeChangedEvent>(changes.Add);
                chance = f.Service.Quote(friend.NpcId, "ask_out").Chance.Final;
                InteractionResult result = f.Service.Interact(friend.NpcId, "ask_out");
                if (result.Success)
                {
                    partners++;
                    Assert.That((friend.Type, friend.Value, changes.Single().To), Is.EqualTo((RelationshipType.Partner, 60f, RelationshipType.Partner)));
                }
                else
                {
                    Assert.That((friend.Type, friend.Value), Is.EqualTo((RelationshipType.Friend, 40f)));
                }
            }

            Assert.That(partners / 200f * 100f, Is.EqualTo(chance).Within(10f));
        }

        [Test]
        public void Proposing_Marries_AndAppliesTheWeddingEffects()
        {
            var f = new RelationshipFixture(family: false);
            Relationship partner = f.Add(RelationshipType.Partner, 70f);
            f.Character.Wallet.Deposit(8000m);

            f.Service.Interact(partner.NpcId, "propose");

            Assert.That(partner.Type, Is.EqualTo(RelationshipType.Spouse));
            Assert.That(partner.Memories.Single().Key, Is.EqualTo("memory.wedding"));
            Assert.That(f.Character.Wallet.Cash, Is.EqualTo(3000m));
        }

        [Test]
        public void Babies_NeedFertileOppositeSexParents_AndArriveAfterThePregnancy()
        {
            var f = new RelationshipFixture(family: false);
            Relationship same = f.Add(RelationshipType.Spouse, 60f, sex: BiologicalSex.Male);
            Relationship wife = f.Add(RelationshipType.Spouse, 60f, sex: BiologicalSex.Female, traits: "calm");
            var births = new List<ChildBornEvent>();
            f.Bus.Subscribe<ChildBornEvent>(births.Add);

            Assert.That(f.Service.Quote(same.NpcId, "try_for_baby").ReasonKeys, Is.EquivalentTo(new[] { InteractionQuote.ReasonFertility }));
            f.Service.Interact(wife.NpcId, "try_for_baby");
            Assert.That(f.Service.Quote(wife.NpcId, "try_for_baby").ReasonKeys, Does.Contain(InteractionQuote.ReasonFertility), "one pregnancy at a time");
            for (int i = 0; i < 10; i++)
            {
                f.Month();
            }
            Assert.That(births, Is.Empty, "due after 40 weeks");
            f.Month();

            Npc child = births.Single().Child;
            Assert.That(births.Single().OtherParentId, Is.EqualTo(wife.NpcId));
            Assert.That(child.Name.FamilyName, Is.EqualTo(f.Character.Name.FamilyName));
            Assert.That(f.People.With(child.Id).Type, Is.EqualTo(RelationshipType.Child));
            Assert.That(f.People.Pregnancies, Is.Empty);
        }

        [Test]
        public void Adoption_AddsAChildAtOnce_ForAnyCouple()
        {
            var f = new RelationshipFixture(family: false);
            Relationship husband = f.Add(RelationshipType.Spouse, 60f, sex: BiologicalSex.Male);

            InteractionResult result = f.Service.Interact(husband.NpcId, "adopt");

            Assert.That(result.Child, Is.Not.Null);
            Assert.That(f.People.With(result.Child.Id).Value, Is.EqualTo(70f));
            Assert.That(result.Child.AgeAt(f.Clock.Now), Is.EqualTo(0));
        }

        [Test]
        public void Children_CostMoneyUntilAdulthood()
        {
            var f = new RelationshipFixture(family: false);
            f.Add(RelationshipType.Child, 70f, age: 5);
            f.Add(RelationshipType.Child, 70f, age: 25);
            f.Character.Wallet.Deposit(10000m);
            var shortfalls = new List<CashShortfallEvent>();
            f.Bus.Subscribe<CashShortfallEvent>(shortfalls.Add);

            f.Month();
            f.Month();

            Assert.That(f.Character.Wallet.Cash, Is.EqualTo(0m));
            Assert.That(shortfalls.Single().Amount, Is.EqualTo(2000m));
        }

        [Test]
        public void UnhappyPartnersLeave_AndUnhappySpousesDivorce()
        {
            var f = new RelationshipFixture(family: false);
            Relationship partner = f.Add(RelationshipType.Partner, -50f);
            Relationship spouse = f.Add(RelationshipType.Spouse, -60f);

            f.Month();
            f.Month();
            Assert.That(partner.Type, Is.EqualTo(RelationshipType.Partner));
            f.Month();
            Assert.That(partner.Type, Is.EqualTo(RelationshipType.Ex));
            Assert.That(spouse.Type, Is.EqualTo(RelationshipType.Spouse));
            Assert.That(f.LastTick.Interrupts.Select(i => i.ReasonKey), Does.Contain(RelationshipService.LeftInterruptKey));
            for (int i = 0; i < 3; i++)
            {
                f.Month();
            }

            Assert.That(spouse.Type, Is.EqualTo(RelationshipType.Ex));
            Assert.That(spouse.Memories.Last().Key, Is.EqualTo(RelationshipService.DivorceMemoryKey));
        }

        [Test]
        public void VeryOldNpcsDie_AndCloseOnesLeaveGrief()
        {
            var f = new RelationshipFixture(family: false);
            Relationship grandparent = f.Add(RelationshipType.Parent, 100f, age: 115);
            var deaths = new List<NpcDiedEvent>();
            f.Bus.Subscribe<NpcDiedEvent>(deaths.Add);
            float stress = f.Character.Stats.GetValue(StatIds.Stress);

            f.Year();

            Assert.That(deaths.Single().Relationship, Is.SameAs(grandparent));
            Assert.That(f.People.Npc(grandparent.NpcId).IsAlive, Is.False);
            Assert.That(f.Character.Stats.GetValue(StatIds.Stress), Is.GreaterThan(stress));
            Assert.That(f.LastTick.Interrupts.Single().Severity, Is.EqualTo(BetterLife.Core.Domain.Severity.Major));
            Assert.That(f.People.Living(RelationshipType.Parent), Is.Empty);
        }

        [Test]
        public void CloseTies_RaiseTheRelationshipsStat_AndEaseLoneliness()
        {
            var popular = new RelationshipFixture(family: false);
            var alone = new RelationshipFixture(family: false);
            for (int i = 0; i < 5; i++)
            {
                popular.Add(RelationshipType.Friend, 90f);
            }
            for (int i = 0; i < 12; i++)
            {
                popular.Month(socialHours: 30);
                alone.Month();
            }

            Assert.That(popular.Character.Stats.GetValue(StatIds.Relationships), Is.InRange(70f, 95f), "close ties settle below the maximum");
            Assert.That(alone.Character.Stats.GetValue(StatIds.Relationships), Is.LessThan(30f));
            Assert.That(popular.Character.Stats.GetValue(StatIds.Loneliness), Is.LessThan(alone.Character.Stats.GetValue(StatIds.Loneliness)));
        }

        [Test]
        public void CloseWorkContacts_HelpJobApplications_UpToTheCap()
        {
            var f = new RelationshipFixture(family: false);
            for (int i = 0; i < 3; i++)
            {
                f.Add(RelationshipType.Colleague, 60f);
            }
            f.Add(RelationshipType.Contact, 10f);
            float three = f.Service.HiringBonus(null);
            for (int i = 0; i < 5; i++)
            {
                f.Add(RelationshipType.Contact, 80f);
            }

            Assert.That(three, Is.EqualTo(9f));
            Assert.That(f.Service.HiringBonus(null), Is.EqualTo(15f));
        }
    }
}
