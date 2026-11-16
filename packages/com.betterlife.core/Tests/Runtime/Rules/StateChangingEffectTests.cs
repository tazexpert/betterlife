using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Careers;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Randomness;
using BetterLife.Core.Relationships;
using BetterLife.Core.Rules;
using BetterLife.Core.Tests.Careers;
using BetterLife.Core.Tests.Relationships;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Rules
{
    /// <summary>Tests for event effects that change real state: career end/promotion, marriage, children, deaths.</summary>
    [TestFixture]
    public class StateChangingEffectTests
    {
        private static EffectResult Apply(IEffect effect, Character character, BetterLife.Core.Time.GameDate now, BetterLife.Core.Events.IEventBus bus)
        {
            EffectResult result = effect.Apply(new RuleContext(character, now, new Pcg32(1UL, 1UL), "test"));
            result.PublishTo(bus);
            return result;
        }

        [Test]
        public void CareerEnd_QuitsOrLaysOff_WithTheRightReason()
        {
            var f = new SchoolWorkFixture(2030);
            f.GiveJob("clerk");
            EffectResult result = Apply(new EndCareerEffect(JobEndReason.LaidOff), f.Character, f.Clock.Now, f.Bus);

            Assert.That(f.Character.Career.IsEmployed, Is.False);
            Assert.That(f.Character.Career.History.Last().Reason, Is.EqualTo(JobEndReason.LaidOff));
            Assert.That(result.Lines.Single().Key, Is.EqualTo(EndCareerEffect.LaidOffKey));
            Assert.That(Apply(new EndCareerEffect(JobEndReason.Quit), f.Character, f.Clock.Now, f.Bus).Lines, Is.Empty, "no job, nothing to quit");
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new EndCareerEffect(JobEndReason.Fired));
        }

        [Test]
        public void Promote_RaisesOneLevel_UntilTheTop()
        {
            var f = new SchoolWorkFixture(2030);
            Job job = f.GiveJob("clerk");
            var promotions = new List<PromotedEvent>();
            f.Bus.Subscribe<PromotedEvent>(promotions.Add);

            Apply(new PromoteEffect(), f.Character, f.Clock.Now, f.Bus);
            Assert.That(job.LevelIndex, Is.EqualTo(1));
            Assert.That(promotions, Has.Count.EqualTo(1));

            Apply(new PromoteEffect(), f.Character, f.Clock.Now, f.Bus);
            Assert.That(job.LevelIndex, Is.EqualTo(1), "manager is the top level of the test clerk track");
            Assert.That(promotions, Has.Count.EqualTo(1));
        }

        [Test]
        public void Marry_TurnsTheClosestPartnerIntoTheSpouse()
        {
            var f = new RelationshipFixture(family: false);
            Assert.That(Apply(new MarryEffect(), f.Character, f.Clock.Now, f.Bus).Lines, Is.Empty, "no partner, no wedding");
            f.Add(RelationshipType.Partner, 40f);
            Relationship closest = f.Add(RelationshipType.Partner, 80f);
            var changes = new List<RelationshipTypeChangedEvent>();
            f.Bus.Subscribe<RelationshipTypeChangedEvent>(changes.Add);

            Apply(new MarryEffect(), f.Character, f.Clock.Now, f.Bus);

            Assert.That(closest.Type, Is.EqualTo(RelationshipType.Spouse));
            Assert.That(changes.Single().To, Is.EqualTo(RelationshipType.Spouse));
            Assert.That(f.People.Living(RelationshipType.Partner).Count(), Is.EqualTo(1));
        }

        [Test]
        public void Conceive_StartsAPregnancy_OrAnAdoptionWhenThatIsImpossible()
        {
            var couple = new RelationshipFixture(family: false);
            couple.Add(RelationshipType.Spouse, 70f, sex: BiologicalSex.Female);
            Apply(new ConceiveEffect(), couple.Character, couple.Clock.Now, couple.Bus);
            Assert.That(couple.People.Pregnancies, Has.Count.EqualTo(1));

            var same = new RelationshipFixture(family: false);
            same.Add(RelationshipType.Spouse, 70f, sex: BiologicalSex.Male);
            Apply(new ConceiveEffect(), same.Character, same.Clock.Now, same.Bus);
            Assert.That(same.People.Pregnancies, Is.Empty);
            Assert.That(same.People.Living(RelationshipType.Child).Count(), Is.EqualTo(1), "adopted at once");

            var single = new RelationshipFixture(family: false);
            Assert.That(Apply(new ConceiveEffect(), single.Character, single.Clock.Now, single.Bus).Lines, Is.Empty);
        }

        [Test]
        public void Death_TakesTheClosestPersonOfThatKind()
        {
            var f = new RelationshipFixture(family: false);
            Relationship distant = f.Add(RelationshipType.Friend, 20f);
            Relationship best = f.Add(RelationshipType.Friend, 90f);
            var deaths = new List<NpcDiedEvent>();
            f.Bus.Subscribe<NpcDiedEvent>(deaths.Add);

            EffectResult result = Apply(new NpcDeathEffect(RelationshipType.Friend), f.Character, f.Clock.Now, f.Bus);

            Assert.That(f.People.Npc(best.NpcId).IsAlive, Is.False);
            Assert.That(f.People.Npc(distant.NpcId).IsAlive, Is.True);
            Assert.That(deaths.Single().Npc.Id, Is.EqualTo(best.NpcId));
            Assert.That(result.Lines.Single().Key, Is.EqualTo(NpcDeathEffect.DiedKey));
        }
    }
}
