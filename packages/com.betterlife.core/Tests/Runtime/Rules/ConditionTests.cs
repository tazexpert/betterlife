using System.Linq;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Rules;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Rules
{
    /// <summary>Tests for the built-in conditions.</summary>
    [TestFixture]
    public class ConditionTests
    {
        private RuleFixture _f;

        [SetUp]
        public void SetUp()
        {
            _f = new RuleFixture();
        }

        [Test]
        public void Stat_ReadsStoredAndDerivedStats()
        {
            _f.Character.Stats.SetStored(StatIds.Stress, 75f);

            Assert.That(_f.Condition("{'type':'stat','stat':'mental.stress','op':'>=','value':70}").Evaluate(_f.Context), Is.True);
            Assert.That(_f.Condition("{'type':'stat','stat':'health','op':'<','value':10}").Evaluate(_f.Context), Is.False);
        }

        [Test]
        public void Flag_SetOrCount()
        {
            ICondition isSet = _f.Condition("{'type':'flag','flag':'career.fired_once'}");
            ICondition count = _f.Condition("{'type':'flag','flag':'vice.gambling_wins','op':'>=','value':3}");
            Assert.That(isSet.Evaluate(_f.Context), Is.False);
            Assert.That(count.Evaluate(_f.Context), Is.False);

            _f.Character.Flags.Set("career.fired_once", RuleFixture.Now);
            _f.Character.Flags.Increment("vice.gambling_wins", RuleFixture.Now, 3);

            Assert.That(isSet.Evaluate(_f.Context), Is.True);
            Assert.That(count.Evaluate(_f.Context), Is.True);
        }

        [Test]
        public void Trait_OptionallyChecksRevealedState()
        {
            _f.Character.Traits.Add("anxious", RuleFixture.Now);

            Assert.That(_f.Condition("{'type':'trait','trait':'anxious'}").Evaluate(_f.Context), Is.True);
            Assert.That(_f.Condition("{'type':'trait','trait':'anxious','revealed':true}").Evaluate(_f.Context), Is.False);
            Assert.That(_f.Condition("{'type':'trait','trait':'anxious','revealed':false}").Evaluate(_f.Context), Is.True);
            Assert.That(_f.Condition("{'type':'trait','trait':'calm'}").Evaluate(_f.Context), Is.False);
        }

        [Test]
        public void Skill_Age_Year_Cash_Reputation_HealthCondition()
        {
            _f.Character.Skills.Practice("cooking", 40f, RuleFixture.Now);
            _f.Character.Wallet.Deposit(5000m);
            _f.Character.Reputation.Change("business", 60f);
            _f.Character.SetCondition(new HealthCondition("flu", 20f, RuleFixture.Now));

            Assert.That(_f.Condition("{'type':'skill','skill':'cooking','op':'>=','value':40}").Evaluate(_f.Context), Is.True);
            Assert.That(_f.Condition("{'type':'age','op':'==','value':25}").Evaluate(_f.Context), Is.True);
            Assert.That(_f.Condition("{'type':'year','op':'>=','value':2027}").Evaluate(_f.Context), Is.False);
            Assert.That(_f.Condition("{'type':'cash','op':'>=','amount':5000}").Evaluate(_f.Context), Is.True);
            Assert.That(_f.Condition("{'type':'cash','op':'>','amount':5000.01}").Evaluate(_f.Context), Is.False);
            Assert.That(_f.Condition("{'type':'reputation','domain':'business','op':'>','value':50}").Evaluate(_f.Context), Is.True);
            Assert.That(_f.Condition("{'type':'healthCondition','condition':'flu'}").Evaluate(_f.Context), Is.True);
        }

        [Test]
        public void Composites_CombineChildren()
        {
            ICondition condition = _f.Condition(@"{'type':'all','conditions':[
                {'type':'age','op':'>=','value':18},
                {'type':'not','condition':{'type':'flag','flag':'career.fired_once'}},
                {'type':'any','conditions':[{'type':'trait','trait':'calm'},{'type':'cash','op':'>=','amount':0}]}]}");

            Assert.That(condition.Evaluate(_f.Context), Is.True);
            _f.Character.Flags.Set("career.fired_once", RuleFixture.Now);
            Assert.That(condition.Evaluate(_f.Context), Is.False);
        }

        [Test]
        public void Explain_BuildsTraceTree_WithVisibility()
        {
            ICondition condition = _f.Condition(@"{'type':'all','conditions':[
                {'type':'stat','stat':'mental.stress','op':'>=','value':70},
                {'type':'flag','flag':'career.fired_once'},
                {'type':'stat','stat':'hidden.willpower','op':'>','value':10}]}");

            ConditionTrace trace = condition.Explain(_f.Context);

            Assert.That(trace.Passed, Is.False);
            Assert.That(trace.Children.Select(c => c.Passed), Is.EqualTo(new[] { false, false, true }));
            Assert.That(trace.Children.Select(c => c.IsVisibleToPlayer), Is.EqualTo(new[] { true, false, false }));
            RuleText stress = trace.Children[0].Description;
            Assert.That(stress.Key, Is.EqualTo(RuleTextKeys.CondStat));
            Assert.That(stress.Args[RuleTextKeys.ArgSubjectKey], Is.EqualTo("stat.mental.stress.name"));
            Assert.That(stress.Args[RuleTextKeys.ArgOp], Is.EqualTo(">="));
            Assert.That(stress.Args[RuleTextKeys.ArgValue], Is.EqualTo("70"));
        }

        [Test]
        public void HiddenTrait_IsNotVisibleInTrace()
        {
            Assert.That(_f.Condition("{'type':'trait','trait':'anxious'}").Explain(_f.Context).IsVisibleToPlayer, Is.False);
            Assert.That(_f.Condition("{'type':'trait','trait':'athletic'}").Explain(_f.Context).IsVisibleToPlayer, Is.True);
        }
    }
}
