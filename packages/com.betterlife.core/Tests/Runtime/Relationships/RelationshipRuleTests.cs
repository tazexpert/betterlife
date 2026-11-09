using System.Linq;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Genetics;
using BetterLife.Core.Relationships;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using BetterLife.Core.Tests.Rules;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Relationships
{
    /// <summary>Tests for the relationship condition and effect.</summary>
    [TestFixture]
    public class RelationshipRuleTests
    {
        private RuleFixture _f;

        [SetUp]
        public void SetUp()
        {
            _f = new RuleFixture();
            Add(RelationshipType.Friend, 70f);
            Add(RelationshipType.Friend, 20f);
            Add(RelationshipType.Parent, 40f);
            Relationship gone = Add(RelationshipType.Spouse, 90f);
            _f.Character.Relationships.Npc(gone.NpcId).Die(RuleFixture.Now);
        }

        [TestCase("{'type':'relationship','relation':'friend','count':{'op':'==','value':2}}", true)]
        [TestCase("{'type':'relationship','relation':'spouse','count':{'op':'==','value':0}}", true)]
        [TestCase("{'type':'relationship','relation':'any','count':{'op':'>=','value':4}}", false)]
        [TestCase("{'type':'relationship','relation':'friend','op':'>=','value':70}", true)]
        [TestCase("{'type':'relationship','relation':'any','op':'>','value':80}", false)]
        [TestCase("{'type':'relationship','relation':'child','op':'>=','value':-100}", false)]
        public void Condition_Evaluates_AndRoundTrips(string json, bool expected)
        {
            ICondition condition = _f.Condition(json);

            Assert.That(condition.Evaluate(_f.Context), Is.EqualTo(expected));
            Assert.That(condition.Explain(_f.Context).Passed, Is.EqualTo(expected));
            Assert.That(JToken.DeepEquals(_f.Registry.WriteCondition(condition), JObject.Parse(json)), Is.True);
        }

        [TestCase("{'type':'relationship','relation':'cousin','count':{'op':'>','value':0}}", "'cousin' is not one of")]
        [TestCase("{'type':'relationship','relation':'friend'}", "either 'count' or 'op' + 'value'")]
        [TestCase("{'type':'relationship','relation':'friend','count':{'op':'>','value':0},'op':'>','value':1}", "either 'count' or 'op' + 'value'")]
        public void InvalidCondition_IsAParseError(string json, string message)
        {
            Assert.That(Assert.Throws<RuleParseException>(() => _f.Condition(json)).Message, Does.Contain(message));
        }

        [Test]
        public void Effect_ChangesTheClosestOrEveryone_AndLeavesMemories()
        {
            _f.Effects("[{'type':'relationship','relation':'friend','amount':10,'memory':'memory.party'},"
                + "{'type':'relationship','relation':'any','amount':-5,'target':'all'}]").Apply(_f.Context);

            var friends = _f.Character.Relationships.Living(RelationshipType.Friend).OrderByDescending(r => r.Value).ToList();
            Assert.That(friends.Select(r => r.Value), Is.EqualTo(new[] { 75f, 15f }));
            Assert.That(friends[0].Memories.Single().Key, Is.EqualTo("memory.party"));
            Assert.That(friends[1].Memories, Is.Empty);
            Assert.That(_f.Character.Relationships.Living(RelationshipType.Parent).Single().Value, Is.EqualTo(35f));
            Assert.That(_f.Character.Relationships.All.Single(r => r.Type == RelationshipType.Spouse).Value, Is.EqualTo(90f), "the dead are unchanged");
        }

        private Relationship Add(RelationshipType type, float value)
        {
            return _f.Character.Relationships.Add(id => new Npc(id, new PersonName("N" + id, "T"), BiologicalSex.Female, RuleFixture.Now.AddWeeks(-1500),
                new Genome(), null), type, value, RuleFixture.Now);
        }
    }
}
