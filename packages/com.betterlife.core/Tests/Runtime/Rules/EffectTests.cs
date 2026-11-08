using System.Linq;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Rules;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Rules
{
    /// <summary>Tests for the built-in effects.</summary>
    [TestFixture]
    public class EffectTests
    {
        private RuleFixture _f;

        [SetUp]
        public void SetUp()
        {
            _f = new RuleFixture();
        }

        [Test]
        public void Stat_GoesThroughCharacter_WithTraitsAndSource()
        {
            _f.Character.Traits.Add("anxious", RuleFixture.Now);

            EffectResult result = _f.Effects("[{'type':'stat','stat':'mental.stress','amount':8}]").Apply(_f.Context);

            Assert.That(_f.Character.Stats.GetValue(StatIds.Stress), Is.EqualTo(30f).Within(0.0001f));
            Assert.That(result.StatChanges.Single().AppliedDelta, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(_f.Character.ChangeLog.Entries.Single().SourceKey, Is.EqualTo(RuleFixture.Source));
            Assert.That(result.Lines.Single().Args[RuleTextKeys.ArgValue], Is.EqualTo("10"));
        }

        [Test]
        public void Stat_SourceOverride_AndHiddenStatsAreSilent()
        {
            EffectResult result = _f.Effects("[{'type':'stat','stat':'hidden.willpower','amount':5,'source':'source.meditation'}]").Apply(_f.Context);

            Assert.That(result.Lines, Is.Empty);
            Assert.That(result.StatChanges, Has.Count.EqualTo(1));
            Assert.That(_f.Character.ChangeLog.Entries.Single().SourceKey, Is.EqualTo("source.meditation"));
        }

        [Test]
        public void Ranges_RollEachTime_AndAreDeterministicPerSeed()
        {
            EffectSequence effects = _f.Effects("[{'type':'stat','stat':'wealth','amount':{'min':1,'max':9}}]");
            var other = new RuleFixture();

            float[] first = Enumerable.Range(0, 5).Select(_ => effects.Apply(_f.Context).StatChanges[0].AppliedDelta).ToArray();
            float[] second = Enumerable.Range(0, 5).Select(_ => effects.Apply(other.Context).StatChanges[0].AppliedDelta).ToArray();

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first.Distinct().Count(), Is.GreaterThan(1), "rolled anew each time");
            Assert.That(first, Is.All.InRange(1f, 9f));
        }

        [Test]
        public void Cash_PaysWhatIsAvailable_AndReportsShortfall()
        {
            _f.Character.Wallet.Deposit(3000m);

            EffectResult result = _f.Effects("[{'type':'cash','amount':-5000}]").Apply(_f.Context);

            Assert.That(_f.Character.Wallet.Cash, Is.EqualTo(0m));
            Assert.That(result.CashShortfall, Is.EqualTo(2000m));
            Assert.That(result.Lines.Select(l => l.Key), Is.EqualTo(new[] { RuleTextKeys.EffectCashPay, RuleTextKeys.EffectCashShortfall }));
        }

        [Test]
        public void Cash_Gain_AndShortfallSumsAcrossEffects()
        {
            EffectResult result = _f.Effects("[{'type':'cash','amount':100},{'type':'cash','amount':-150},{'type':'cash','amount':-50}]").Apply(_f.Context);

            Assert.That(_f.Character.Wallet.Cash, Is.EqualTo(0m));
            Assert.That(result.CashShortfall, Is.EqualTo(100m));
        }

        [Test]
        public void Flags_AreSilent()
        {
            EffectResult result = _f.Effects(@"[
                {'type':'flag.set','flag':'relationship.recent_breakup','expiryWeeks':4},
                {'type':'flag.increment','flag':'vice.gambling_wins','amount':2},
                {'type':'flag.set','flag':'career.fired_once'},
                {'type':'flag.clear','flag':'career.fired_once'}]").Apply(_f.Context);

            Assert.That(result.Lines, Is.Empty);
            Assert.That(_f.Character.Flags.Find("relationship.recent_breakup").ExpiresOn, Is.EqualTo(RuleFixture.Now.AddWeeks(4)));
            Assert.That(_f.Character.Flags.GetCount("vice.gambling_wins"), Is.EqualTo(2));
            Assert.That(_f.Character.Flags.IsSet("career.fired_once"), Is.False);
        }

        [Test]
        public void Traits_HiddenGainIsSilent_RevealAnnouncesIt()
        {
            EffectResult gain = _f.Effects("[{'type':'trait.add','trait':'anxious'},{'type':'trait.add','trait':'athletic'}]").Apply(_f.Context);
            EffectResult reveal = _f.Effects("[{'type':'trait.reveal','trait':'anxious'}]").Apply(_f.Context);
            EffectResult again = _f.Effects("[{'type':'trait.reveal','trait':'anxious'}]").Apply(_f.Context);
            EffectResult remove = _f.Effects("[{'type':'trait.remove','trait':'anxious'}]").Apply(_f.Context);

            Assert.That(gain.Lines.Single().Args[RuleTextKeys.ArgSubjectKey], Is.EqualTo("trait.athletic.name"));
            Assert.That(reveal.Lines.Single().Key, Is.EqualTo(RuleTextKeys.EffectTraitReveal));
            Assert.That(again.Lines, Is.Empty);
            Assert.That(remove.Lines.Single().Key, Is.EqualTo(RuleTextKeys.EffectTraitRemove));
            Assert.That(_f.Character.Traits.Has("anxious"), Is.False);
        }

        [Test]
        public void Skill_Reputation_HealthConditions()
        {
            _f.Effects(@"[
                {'type':'skill.practice','skill':'cooking','gain':10},
                {'type':'reputation','domain':'business','amount':15},
                {'type':'healthCondition.add','condition':'flu','severity':{'min':20,'max':40}}]").Apply(_f.Context);

            Assert.That(_f.Character.Skills.GetLevel("cooking"), Is.EqualTo(10f));
            Assert.That(_f.Character.Reputation.Get("business"), Is.EqualTo(15f));
            Assert.That(_f.Character.Conditions.Single().Severity, Is.InRange(20f, 40f));

            EffectResult cure = _f.Effects("[{'type':'healthCondition.remove','condition':'flu'}]").Apply(_f.Context);
            Assert.That(cure.Lines, Has.Count.EqualTo(1));
            Assert.That(_f.Character.HasCondition("flu"), Is.False);
        }

        [Test]
        public void If_ChoosesBranchFromCurrentState()
        {
            EffectSequence effects = _f.Effects(@"[{'type':'if','condition':{'type':'trait','trait':'calm'},
                'then':[{'type':'stat','stat':'mental.happiness','amount':-2}],
                'else':[{'type':'stat','stat':'mental.happiness','amount':-8}]}]");

            effects.Apply(_f.Context);
            Assert.That(_f.Character.Stats.GetValue(StatIds.Happiness), Is.EqualTo(52f));

            _f.Character.Traits.Add("calm", RuleFixture.Now);
            effects.Apply(_f.Context);
            Assert.That(_f.Character.Stats.GetValue(StatIds.Happiness), Is.EqualTo(50f));
        }

        [Test]
        public void Preview_ShowsRanges_HidesSecrets_AndFollowsIfBranch()
        {
            EffectSequence effects = _f.Effects(@"[
                {'type':'stat','stat':'mental.stress','amount':{'min':3,'max':8}},
                {'type':'cash','amount':{'min':-500,'max':-100}},
                {'type':'flag.set','flag':'career.fired_once'},
                {'type':'trait.add','trait':'anxious'},
                {'type':'if','condition':{'type':'cash','op':'>','amount':1000000},'then':[{'type':'cash','amount':1}]}]");

            var preview = effects.Preview(_f.Context);

            Assert.That(preview, Has.Count.EqualTo(2));
            Assert.That(preview[0].Args[RuleTextKeys.ArgMin], Is.EqualTo("3"));
            Assert.That(preview[0].Args[RuleTextKeys.ArgMax], Is.EqualTo("8"));
            Assert.That(preview[1].Key, Is.EqualTo(RuleTextKeys.EffectCashPay));
            Assert.That(preview[1].Args[RuleTextKeys.ArgMin], Is.EqualTo("100"));
            Assert.That(preview[1].Args[RuleTextKeys.ArgMax], Is.EqualTo("500"));
            Assert.That(_f.Character.Wallet.Cash, Is.EqualTo(0m), "preview changes nothing");
        }
    }
}
