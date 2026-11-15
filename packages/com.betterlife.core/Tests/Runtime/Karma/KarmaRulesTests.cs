using System.Linq;
using BetterLife.Core.Astrology;
using BetterLife.Core.Karma;
using BetterLife.Core.Persistence;
using BetterLife.Core.Rules;
using BetterLife.Core.Tests.Astrology;
using BetterLife.Core.Tests.Rules;
using BetterLife.Core.Time;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Karma
{
    /// <summary>Tests for the karma and fortune rule types and karma saving.</summary>
    [TestFixture]
    public class KarmaRulesTests
    {
        private RuleFixture _f;

        [SetUp]
        public void SetUp()
        {
            _f = new RuleFixture();
            KarmaService karma = TestAstrology.Karma();
            _f.Registry
                .Register(new FortuneConditionConverter(new FortuneCalculator(TestAstrology.Tables())))
                .Register(new BadKarmaEffectConverter(karma))
                .Register(new MeritEffectConverter(karma));
        }

        [Test]
        public void KarmaEffects_UpdateTheLedger_AndOnlyMeritIsShown()
        {
            EffectResult result = _f.Effects("[{'type':'karma.bad','amount':20},{'type':'karma.good','amount':5},{'type':'karma.merit','amount':10}]").Apply(_f.Context);

            Assert.That(_f.Character.Karma.Bad, Is.EqualTo(20f));
            Assert.That(_f.Character.Karma.Good, Is.EqualTo(15f));
            Assert.That(_f.Character.Karma.PendingDebt, Is.EqualTo(5f), "debt 10 minus merit offset 5");
            Assert.That(_f.Character.Karma.Debts.Single().SourceKey, Is.EqualTo(RuleFixture.Source));
            Assert.That(result.Lines.Single().Key, Is.EqualTo(KarmaTextKeys.EffectMerit));
        }

        [Test]
        public void KarmaCondition_IsHidden_FortuneCondition_IsVisible()
        {
            _f.Character.Karma.AddBad(40f);
            ICondition karma = _f.Condition("{'type':'karma','kind':'bad','op':'>','value':30}");
            ICondition fortune = _f.Condition("{'type':'fortune','area':'career','op':'>=','value':50}");

            Assert.That(karma.Evaluate(_f.Context), Is.True);
            Assert.That(karma.Explain(_f.Context).IsVisibleToPlayer, Is.False);
            Assert.That(fortune.Evaluate(_f.Context), Is.True);
            Assert.That(fortune.Explain(_f.Context).IsVisibleToPlayer, Is.True);
        }

        [Test]
        public void NewRuleTypes_RoundTrip()
        {
            const string effects = "[{'type':'karma.bad','amount':{'min':2,'max':4}},{'type':'karma.merit','amount':10},{'type':'karma.good','amount':1}]";
            const string condition = "{'type':'fortune','area':'love','op':'<','value':40}";

            Assert.That(JToken.DeepEquals(_f.Registry.WriteEffects(_f.Effects(effects)), JToken.Parse(effects)), Is.True);
            Assert.That(JToken.DeepEquals(_f.Registry.WriteCondition(_f.Condition(condition)), JToken.Parse(condition)), Is.True);
        }

        [Test]
        public void Karma_IsSavedAndRestored()
        {
            _f.Character.Karma.AddGood(12f);
            _f.Character.Karma.AddBad(7f);
            _f.Character.Karma.AddDebt(new KarmicDebt(3.5f, RuleFixture.Now.AddWeeks(30), "event.lie"));

            var copy = CharacterDtoMapper.FromDto(CharacterDtoMapper.ToDto(_f.Character), _f.Catalogs).Character;

            Assert.That(copy.Karma.Good, Is.EqualTo(12f));
            Assert.That(copy.Karma.Bad, Is.EqualTo(7f));
            KarmicDebt debt = copy.Karma.Debts.Single();
            Assert.That((debt.Amount, debt.DueOn, debt.SourceKey), Is.EqualTo((3.5f, RuleFixture.Now.AddWeeks(30), "event.lie")));
        }

        [Test]
        public void OldSaves_WithoutKarma_LoadAsZero()
        {
            CharacterDto dto = CharacterDtoMapper.ToDto(_f.Character);
            dto.Karma = null;

            var copy = CharacterDtoMapper.FromDto(dto, _f.Catalogs).Character;

            Assert.That(copy.Karma.Good, Is.EqualTo(0f));
            Assert.That(copy.Karma.Debts, Is.Empty);
        }
    }
}
