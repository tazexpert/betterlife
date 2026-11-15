using System.Linq;
using BetterLife.Core.Careers;
using BetterLife.Core.Domain;
using BetterLife.Core.Education;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using BetterLife.Core.Tests.Rules;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Careers
{
    /// <summary>Tests for the education and career conditions and effects.</summary>
    [TestFixture]
    public class SchoolWorkRuleTests
    {
        private RuleFixture _f;

        [SetUp]
        public void SetUp()
        {
            _f = new RuleFixture();
            var index = new CareerTrackIndex();
            index.Add("clerk", CareerCategory.Corporate, new[] { "staff", "manager" });
            _f.Registry
                .Register(new EducationConditionConverter(SchoolWorkFixture.Content.Get<DefinitionCatalog<EducationProgramDefinition>>()))
                .Register(new CareerConditionConverter(index));
            _f.Character.Education.Restore(new Enrollment("bachelor", "bachelor.open", RuleFixture.Now, RuleFixture.Now, 0, 0, 60f, false, TuitionFunding.Cash),
                new[] { new Credential("secondary", "secondary.public", RuleFixture.Now, 70f) }, null);
            _f.Character.Career.Start(new Job("clerk", 1, RuleFixture.Now, RuleFixture.Now, 50f, 50f, 0, null));
        }

        [TestCase("{'type':'education','completed':'primary'}", true)]
        [TestCase("{'type':'education','completed':'secondary'}", true)]
        [TestCase("{'type':'education','completed':'bachelor'}", false)]
        [TestCase("{'type':'education','enrolled':true}", true)]
        [TestCase("{'type':'education','enrolled':false}", false)]
        [TestCase("{'type':'education','enrolledIn':'bachelor'}", true)]
        [TestCase("{'type':'career','employed':true}", true)]
        [TestCase("{'type':'career','category':'corporate'}", true)]
        [TestCase("{'type':'career','category':'illegal'}", false)]
        [TestCase("{'type':'career','track':'clerk'}", true)]
        [TestCase("{'type':'career','track':'clerk','minLevel':'manager'}", true)]
        public void Conditions_Evaluate_AndRoundTrip(string json, bool expected)
        {
            ICondition condition = _f.Condition(json);

            Assert.That(condition.Evaluate(_f.Context), Is.EqualTo(expected));
            Assert.That(condition.Explain(_f.Context).Passed, Is.EqualTo(expected));
            Assert.That(JToken.DeepEquals(_f.Registry.WriteCondition(condition), JObject.Parse(json)), Is.True);
        }

        [TestCase("{'type':'education','completed':'astronaut_school'}", "unknown education program")]
        [TestCase("{'type':'education','completed':'primary','enrolled':true}", "exactly one")]
        [TestCase("{'type':'career','track':'pilot'}", "unknown career track")]
        [TestCase("{'type':'career','track':'clerk','minLevel':'ceo'}", "has no level 'ceo'")]
        [TestCase("{'type':'career','category':'pirate'}", "'pirate' is not one of")]
        [TestCase("{'type':'career','employed':true,'minLevel':'staff'}", "exactly one")]
        public void InvalidConditions_AreParseErrors(string json, string message)
        {
            var ex = Assert.Throws<RuleParseException>(() => _f.Condition(json));

            Assert.That(ex.Message, Does.Contain(message));
        }

        [Test]
        public void Effects_ChangePerformanceAndBoss_AndFire()
        {
            EffectResult result = _f.Effects(@"[{'type':'education.performance','amount':10},{'type':'career.performance','amount':-20},
                {'type':'career.boss','amount':15},{'type':'career.fire'}]").Apply(_f.Context);

            Assert.That(_f.Character.Education.Current.Performance, Is.EqualTo(70f));
            Assert.That(_f.Character.Career.IsEmployed, Is.False);
            PastJob fired = _f.Character.Career.History.Single();
            Assert.That((fired.Reason, fired.LevelIndex), Is.EqualTo((JobEndReason.Fired, 1)));
            Assert.That(result.Messages.OfType<JobEndedEvent>().Single().Job, Is.SameAs(fired));
            Assert.That(result.Lines.Select(l => l.Key), Does.Contain(RuleTextKeys.EffectCareerFired));
            Assert.That(_f.Character.Flags.IsSet(CareerService.FiredFlag), Is.True);
        }

        [Test]
        public void JobEffects_DoNothingWhenUnemployed()
        {
            _f.Character.Career.End(JobEndReason.Quit, RuleFixture.Now);

            EffectResult result = _f.Effects("[{'type':'career.boss','amount':15},{'type':'career.fire'}]").Apply(_f.Context);

            Assert.That(result.Messages, Is.Empty);
            Assert.That(_f.Character.Career.History, Has.Count.EqualTo(1));
        }
    }
}
