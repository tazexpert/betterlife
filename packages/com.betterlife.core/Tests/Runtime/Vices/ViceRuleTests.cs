using System.Collections.Generic;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using BetterLife.Core.Tests.Rules;
using BetterLife.Core.Vices;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Vices
{
    /// <summary>Tests for the vice condition and effect.</summary>
    [TestFixture]
    public class ViceRuleTests
    {
        private RuleFixture _f;

        [SetUp]
        public void SetUp()
        {
            _f = new RuleFixture();
            var known = new HashSet<string> { "drinking", "gambling" };
            _f.Registry.Register(new ViceConditionConverter(known)).Register(new ViceAddictionEffectConverter(known));
            ViceState drinking = _f.Character.Vices.Get("drinking");
            drinking.ChangeAddiction(35f);
            drinking.RecordUse(RuleFixture.Now.AddWeeks(-6), 0f, 12f);
        }

        [TestCase("{'type':'vice','vice':'drinking','measure':'addiction','op':'>=','value':35}", true)]
        [TestCase("{'type':'vice','vice':'drinking','measure':'tolerance','op':'==','value':12}", true)]
        [TestCase("{'type':'vice','vice':'drinking','measure':'uses','op':'==','value':1}", true)]
        [TestCase("{'type':'vice','vice':'drinking','measure':'cleanWeeks','op':'>=','value':6}", true)]
        [TestCase("{'type':'vice','vice':'gambling','measure':'addiction','op':'>','value':0}", false)]
        public void Condition_Evaluates_AndRoundTrips(string json, bool expected)
        {
            ICondition condition = _f.Condition(json);

            Assert.That(condition.Evaluate(_f.Context), Is.EqualTo(expected));
            Assert.That(condition.Explain(_f.Context).IsVisibleToPlayer, Is.False, "habits stay hidden in condition text");
            Assert.That(JToken.DeepEquals(_f.Registry.WriteCondition(condition), JObject.Parse(json)), Is.True);
        }

        [TestCase("{'type':'vice','vice':'chess','op':'>','value':1}", "unknown vice 'chess'")]
        [TestCase("{'type':'vice','vice':'drinking','measure':'joy','op':'>','value':1}", "'joy' is not one of")]
        public void InvalidCondition_IsAParseError(string json, string message)
        {
            Assert.That(Assert.Throws<RuleParseException>(() => _f.Condition(json)).Message, Does.Contain(message));
        }

        [Test]
        public void AddictionEffect_ChangesAddiction_ForMetAndUnmetVices()
        {
            _f.Effects("[{'type':'vice.addiction','vice':'drinking','amount':-10},{'type':'vice.addiction','vice':'gambling','amount':7}]").Apply(_f.Context);

            Assert.That(_f.Character.Vices.Find("drinking").Addiction, Is.EqualTo(25f));
            Assert.That(_f.Character.Vices.Find("gambling").Addiction, Is.EqualTo(7f));
        }
    }
}
