using System;
using System.Linq;
using BetterLife.Core.Domain.Stats;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Domain
{
    /// <summary>Tests for <see cref="WeightedStatFormula"/>, <see cref="StatCatalog"/> and <see cref="StatBlock"/>.</summary>
    [TestFixture]
    public class StatFormulaTests
    {
        [Test]
        public void Health_IsWeightedSumOfSubStats_WithInversion()
        {
            var block = new StatBlock(TestCatalogs.Stats());
            block.SetStored(StatIds.Fitness, 70f);
            block.SetStored(StatIds.Nutrition, 60f);
            block.SetStored(StatIds.Fatigue, 40f);
            block.SetStored(StatIds.DiseaseBurden, 10f);

            // 70*.35 + 60*.25 + (100-40)*.15 + (100-10)*.25 = 24.5 + 15 + 9 + 22.5
            Assert.That(block.GetValue(StatIds.Health), Is.EqualTo(71f).Within(0.001f));
        }

        [Test]
        public void Explain_LinesSumToValue_AndCarryInputs()
        {
            var block = new StatBlock(TestCatalogs.Stats());

            StatBreakdown breakdown = block.ExplainFormula(StatIds.Mental);

            Assert.That(breakdown.Kind, Is.EqualTo(StatBreakdownKind.Formula));
            Assert.That(breakdown.Lines, Has.Count.EqualTo(4));
            Assert.That(breakdown.Lines.Sum(l => l.Contribution), Is.EqualTo(breakdown.Value).Within(0.001f));
            StatBreakdownLine stress = breakdown.Lines.Single(l => l.SourceStat == StatIds.Stress);
            Assert.That(stress.LabelKey, Is.EqualTo("stat.mental.stress.name"));
            Assert.That(stress.InputValue, Is.EqualTo(20f));
            Assert.That(stress.Contribution, Is.EqualTo(24f).Within(0.001f));
        }

        [Test]
        public void Formula_RejectsBadWeights_DuplicatesAndSelfReference()
        {
            Assert.Throws<ArgumentException>(() => new WeightedStatFormula(StatIds.Health, new[] { new StatFormulaTerm(StatIds.Fitness, 0.5f) }));
            Assert.Throws<ArgumentException>(() => new WeightedStatFormula(StatIds.Health, new[]
            {
                new StatFormulaTerm(StatIds.Fitness, 0.5f), new StatFormulaTerm(StatIds.Fitness, 0.5f),
            }));
            Assert.Throws<ArgumentException>(() => new WeightedStatFormula(StatIds.Health, new[] { new StatFormulaTerm(StatIds.Health, 1f) }));
            Assert.Throws<ArgumentException>(() => new WeightedStatFormula(StatIds.Health, new StatFormulaTerm[0]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new StatFormulaTerm(StatIds.Fitness, 0f));
        }

        [Test]
        public void Catalog_RejectsInconsistentContent()
        {
            StatDefinition core = Def(StatIds.Health, StatLayer.Core);
            StatDefinition sub = Def(StatIds.Fitness, StatLayer.Sub, StatIds.Health);
            StatDefinition otherCore = Def(StatIds.Mental, StatLayer.Core);
            var fitnessOnly = new WeightedStatFormula(StatIds.Health, new[] { new StatFormulaTerm(StatIds.Fitness, 1f) });

            Assert.Throws<ArgumentException>(() => new StatCatalog(new[] { core, core }, new IStatFormula[0]), "duplicate");
            Assert.Throws<ArgumentException>(() => new StatCatalog(new[] { sub }, new IStatFormula[0]), "missing parent");
            Assert.Throws<ArgumentException>(() => new StatCatalog(new[] { core }, new[] { fitnessOnly }), "unknown input");
            Assert.Throws<ArgumentException>(() => new StatCatalog(new[] { core, sub }, new[] { fitnessOnly, fitnessOnly }), "two formulas");
            Assert.Throws<ArgumentException>(
                () => new StatCatalog(new[] { core, sub }, new[] { new WeightedStatFormula(StatIds.Fitness, new[] { new StatFormulaTerm(StatIds.Health, 1f) }) }),
                "non-core target");
            Assert.Throws<ArgumentException>(
                () => new StatCatalog(new[] { core, sub, otherCore }, new IStatFormula[]
                {
                    fitnessOnly,
                    new WeightedStatFormula(StatIds.Mental, new[] { new StatFormulaTerm(StatIds.Health, 1f) }),
                }),
                "derived input");
        }

        [Test]
        public void StatBlock_StartsAtDefaults_AndClamps()
        {
            var block = new StatBlock(TestCatalogs.Stats());

            Assert.That(block.GetValue(StatIds.Nutrition), Is.EqualTo(60f));
            Assert.That(block.SetStored(StatIds.Stress, 150f), Is.EqualTo(100f));
            Assert.That(block.SetStored(StatIds.Stress, -5f), Is.EqualTo(0f));
            Assert.That(block.StoredValues.ContainsKey(StatIds.Health), Is.False, "derived stats are not stored");
        }

        [Test]
        public void StatBlock_RejectsDerivedUnknownAndNonFinite()
        {
            var block = new StatBlock(TestCatalogs.Stats());

            Assert.Throws<InvalidOperationException>(() => block.SetStored(StatIds.Health, 10f));
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => block.GetValue(new StatId("nope")));
            Assert.Throws<ArgumentException>(() => block.SetStored(StatIds.Stress, float.NaN));
            Assert.Throws<InvalidOperationException>(() => block.ExplainFormula(StatIds.Stress));
        }

        [Test]
        public void StatId_ValidatesAndComparesByValue()
        {
            Assert.Throws<ArgumentException>(() => new StatId("Bad Id"));
            Assert.That(new StatId("health") == StatIds.Health, Is.True);
            Assert.That(default(StatId).Value, Is.Empty);
        }

        [TestCase(0f, 300)]
        [TestCase(50f, 600)]
        [TestCase(100f, 900)]
        [TestCase(150f, 900)]
        public void CreditScore_MapsToThaiBureauRange(float stat, int display)
        {
            Assert.That(CreditScoreScale.ToDisplay(stat), Is.EqualTo(display));
            Assert.That(CreditScoreScale.FromDisplay(display), Is.EqualTo(Math.Min(stat, 100f)).Within(0.001f));
        }

        private static StatDefinition Def(StatId id, StatLayer layer, StatId? parent = null)
        {
            return new StatDefinition(id, layer, StatPolarity.HigherIsBetter, 50f, "stat.x.name", "stat.x.desc", parent);
        }
    }
}
