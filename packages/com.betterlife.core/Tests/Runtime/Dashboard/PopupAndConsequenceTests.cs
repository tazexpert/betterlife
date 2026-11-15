using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Genetics;
using BetterLife.Core.Events;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Localization;
using BetterLife.Core.Relationships;
using BetterLife.Core.Sessions;
using BetterLife.Core.Tests.Content;
using BetterLife.Core.Tests.LifeEvents;
using BetterLife.Core.Tests.Saving;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.ViewModels;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Dashboard
{
    /// <summary>Tests for task 16b: decisions stop time, consequences (meeting people), family status.</summary>
    [TestFixture]
    public class PopupAndConsequenceTests
    {
        private const string MinorChoice = @"{ 'id': 'test.party', 'titleKey': 'event.test.party.title', 'bodyKey': 'event.test.party.body',
            'category': 'test', 'trigger': 'random', 'severity': 'minor', 'tone': 'neutral', 'cooldownWeeks': 1000, 'defaultChoice': 'stay',
            'choices': [
              { 'id': 'go', 'textKey': 'c.go', 'success': { 'resultKey': 'r.go', 'effects': [ { 'type': 'meet', 'relation': 'friend', 'chance': 1 } ] } },
              { 'id': 'stay', 'textKey': 'c.stay', 'success': { 'resultKey': 'r.stay', 'effects': [ { 'type': 'stat', 'stat': 'health.fitness', 'amount': 1 } ] } } ] }";

        private static IContentRepository _content;

        private static IContentRepository Content => _content ?? (_content = TestContent.Load(
            TestContent.Source("{ 'type': 'events', 'path': 'events/test.json' }", 1f).Add("events/test.json", EventFixture.Events(MinorChoice))));

        [Test]
        public void ADecision_StopsTime_EvenWhenMinor()
        {
            using (LifeSession life = NewLife(new InterruptPolicy()))
            {
                AdvanceResult result = life.Advance(TimeSkip.Year);
                Assert.That(result.WasInterrupted, Is.True);
                Assert.That(result.StoppedBy.RequiresAnswer, Is.True);
                Assert.That(result.WeeksAdvanced, Is.EqualTo(1), "the popup comes in the first week of the year skip");
                Assert.That(Notices.NoticeFeed.StoppingDecision(result, life.Events.Inbox), Is.Not.Null, "the shell opens the dialog");
            }
        }

        [Test]
        public void Babies_AreNotAskedToDecide()
        {
            string template = TestContent.EventSettings(1f);
            string settings = template.Substring(0, template.LastIndexOf('}')) + ", 'minDecisionAge': 6 }";
            IContentRepository content = TestContent.Load(TestContent.Source("{ 'type': 'events', 'path': 'events/test.json' }", 1f)
                .Add("events/test.json", EventFixture.Events(MinorChoice))
                .Add("event_settings.json", settings));
            CreationResult creation = new CharacterCreator(content).Create(new CreationRequest(5UL, CreationMode.Random));
            using (LifeSession life = LifeSessionFactory.CreateNew(content, creation, new EventBus(), new InterruptPolicy()))
            {
                while (life.Character.AgeAt(life.TimeState.Clock.Now.AddWeeks(1)) < 6)
                {
                    AdvanceResult result = life.Advance(TimeSkip.Week);
                    Assert.That(result.StoppedBy == null || !result.StoppedBy.RequiresAnswer, Is.True, "no decision before 6");
                    Assert.That(life.Events.Inbox, Is.Empty);
                }
                life.Advance(TimeSkip.Year);
                Assert.That(life.Events.State.History.Any(h => h.EventId == "test.party") || life.Events.Inbox.Any(), Is.True, "from 6 the event can happen");
            }
        }

        [Test]
        public void Simulations_CanLetDecisionsExpire()
        {
            using (LifeSession life = NewLife(new InterruptPolicy { StopForDecisions = false }))
            {
                AdvanceResult result = life.Advance(TimeSkip.Year);
                Assert.That(result.StoppedBy == null || !result.StoppedBy.RequiresAnswer, Is.True);
                Assert.That(life.Events.State.History.Any(h => h.EventId == "test.party"), Is.True, "it expired and resolved by itself");
            }
        }

        [Test]
        public void GoingToTheParty_CanMeetSomeoneNew()
        {
            ILocalizer keys = DashboardFixture.Keys();
            using (LifeSession life = NewLife(new InterruptPolicy()))
            {
                life.Advance(TimeSkip.Year);
                int friends = life.Character.Relationships.All.Count(r => r.Type == RelationshipType.Friend);
                int id = life.Events.Inbox.Single().InstanceId;

                EventDialogState dialog = EventDialogViewModel.Build(life, id, keys, new MoneyFormatter(keys));
                Assert.That(dialog.Choices[0].SuccessPreview, Is.EqualTo(new[] { MeetEffect.ChanceKey }));

                EventResolution resolution = new DecisionService(null, 1).Answer(life, id, "go");
                Assert.That(life.Character.Relationships.All.Count(r => r.Type == RelationshipType.Friend), Is.EqualTo(friends + 1));
                Assert.That(resolution.Result.Lines.Single().Key, Is.EqualTo(MeetEffect.MetKey));
            }
        }

        [Test]
        public void Home_ShowsRelationshipStatusAndChildren()
        {
            ILocalizer keys = DashboardFixture.Keys();
            using (LifeSession life = SaveFixture.NewLife())
            {
                SaveFixture.Advance(life, 25);
                Character c = life.Character;
                GameDate now = life.TimeState.Clock.Now;
                foreach (Relationship r in c.Relationships.All.Where(r => RelationshipNames.IsRomantic(r.Type) || r.Type == RelationshipType.Ex || r.Type == RelationshipType.Child).ToList())
                {
                    c.Relationships.Forget(r.NpcId);
                }
                Assert.That(Status(life, keys), Is.EqualTo((DashboardViewModel.SingleKey, DashboardViewModel.NoChildrenKey)));

                Relationship partner = c.Relationships.Add(id => Person(id, "Ploy", now, 25), RelationshipType.Partner, 60f, now);
                Assert.That(Status(life, keys).Status, Is.EqualTo(DashboardViewModel.DatingKey));

                partner.Become(RelationshipType.Spouse, now);
                c.Relationships.Add(id => Person(id, "Kid", now, 1), RelationshipType.Child, 80f, now);
                Assert.That(Status(life, keys), Is.EqualTo((DashboardViewModel.MarriedKey, DashboardViewModel.ChildrenCountKey)));

                c.Relationships.Npc(partner.NpcId).Die(now);
                Assert.That(Status(life, keys).Status, Is.EqualTo(DashboardViewModel.WidowedKey));
            }
        }

        private static (string Status, string Children) Status(LifeSession life, ILocalizer keys)
        {
            var rows = DashboardViewModel.Build(life, keys).Family;
            return (rows[0].Value, rows[1].Value);
        }

        private static Npc Person(int id, string name, GameDate now, int age)
        {
            return new Npc(id, new PersonName(name, "Test"), BiologicalSex.Female, now.AddWeeks(-age * 52), new Genome(), new string[0]);
        }

        private static LifeSession NewLife(InterruptPolicy policy)
        {
            CreationResult creation = new CharacterCreator(Content).Create(new CreationRequest(5UL, CreationMode.Random));
            return LifeSessionFactory.CreateNew(Content, creation, new EventBus(), policy);
        }
    }
}
