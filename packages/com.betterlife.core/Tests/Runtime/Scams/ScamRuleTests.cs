using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using BetterLife.Core.Scams;
using BetterLife.Core.Tests.Rules;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Scams
{
    /// <summary>Tests for the scam effect and condition.</summary>
    [TestFixture]
    public class ScamRuleTests
    {
        private RuleFixture _f;

        [SetUp]
        public void SetUp()
        {
            _f = new RuleFixture();
            _f.Registry.Register(new ScamLossEffectConverter(new HashSet<string> { "ponzi" }));
        }

        [Test]
        public void LossEffect_TakesAShareOfCash_AndRecordsAReportableIncident()
        {
            _f.Character.Wallet.Deposit(40000m);
            const string json = "[{'type':'scam.loss','scam':'ponzi','cashShare':0.5,'min':5000,'max':15000}]";

            EffectResult result = _f.Effects(json).Apply(_f.Context);

            Assert.That(_f.Character.Wallet.Cash, Is.EqualTo(25000m), "half is 20,000 but the cap is 15,000");
            ScamIncident incident = _f.Character.Scams.Incidents.Single();
            Assert.That((incident.ScamId, incident.Lost), Is.EqualTo(("ponzi", 15000m)));
            Assert.That(result.Messages.OfType<ScammedEvent>().Single().Incident, Is.SameAs(incident));
            Assert.That(JToken.DeepEquals(_f.Registry.WriteEffect(_f.Effects(json).Effects.Single()), JArray.Parse(json)[0]), Is.True);
        }

        [TestCase("{'type':'scam','measure':'victims','op':'==','value':1}", true)]
        [TestCase("{'type':'scam','measure':'lost','op':'>=','value':3000}", true)]
        [TestCase("{'type':'scam','measure':'detected','op':'>','value':0}", false)]
        public void Condition_ReadsTheHistory(string json, bool expected)
        {
            _f.Character.Scams.AddIncident("ponzi", RuleFixture.Now, 3000m, 20);

            ICondition condition = _f.Condition(json);

            Assert.That(condition.Evaluate(_f.Context), Is.EqualTo(expected));
            Assert.That(JToken.DeepEquals(_f.Registry.WriteCondition(condition), JObject.Parse(json)), Is.True);
        }

        [Test]
        public void UnknownScam_IsAParseError()
        {
            var ex = Assert.Throws<RuleParseException>(() => _f.Effects("[{'type':'scam.loss','scam':'nigerian_prince','cashShare':0.1,'min':0,'max':1}]"));

            Assert.That(ex.Message, Does.Contain("unknown scam 'nigerian_prince'"));
        }
    }
}
