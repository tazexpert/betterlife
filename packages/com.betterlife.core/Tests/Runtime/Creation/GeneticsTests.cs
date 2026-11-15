using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Genetics;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Randomness;
using BetterLife.Core.Tests.Domain;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Creation
{
    /// <summary>Tests for <see cref="GeneticsService"/> and <see cref="RandomExtensions"/>.</summary>
    [TestFixture]
    public class GeneticsTests
    {
        private const int Samples = 20000;

        [Test]
        public void Gaussian_HasRequestedMeanAndDeviation()
        {
            var random = new Pcg32(1UL, 1UL);
            float[] values = Enumerable.Range(0, Samples).Select(_ => random.NextGaussian(50f, 15f)).ToArray();
            double mean = values.Average();
            double sd = System.Math.Sqrt(values.Average(v => (v - mean) * (v - mean)));

            Assert.That(mean, Is.EqualTo(50).Within(0.5));
            Assert.That(sd, Is.EqualTo(15).Within(0.5));
        }

        [Test]
        public void PickWeighted_FollowsWeights_AndHandlesAllZero()
        {
            var random = new Pcg32(2UL, 2UL);
            var items = new[] { ("a", 1f), ("b", 3f), ("c", 0f) };

            var picks = Enumerable.Range(0, Samples).Select(_ => random.PickWeighted(items, i => i.Item2).Item1).ToList();

            Assert.That(picks.Count(p => p == "b") / (double)Samples, Is.EqualTo(0.75).Within(0.02));
            Assert.That(picks, Has.None.EqualTo("c"));
            Assert.That(random.PickWeighted(new[] { "x" }, _ => 0f), Is.Null);
        }

        [Test]
        public void Inheritance_RegressesTowardTheMean()
        {
            GeneticsService service = Service(heritability: 0.6f, noise: 0f);
            var random = new Pcg32(3UL, 3UL);
            var tall = new Genome();
            tall.Set(StatIds.Iq, 90f);
            var average = new Genome();
            average.Set(StatIds.Iq, 70f);

            Genome child = service.Inherit(new[] { tall, average }, random);

            Assert.That(child.Get(StatIds.Iq), Is.EqualTo(0.6f * 80f + 0.4f * 50f).Within(0.001f));
        }

        [Test]
        public void Inheritance_NoiseSpreadsChildrenAroundTheExpectation()
        {
            GeneticsService service = Service(heritability: 0.5f, noise: 8f);
            var random = new Pcg32(4UL, 4UL);
            var parent = new Genome();
            parent.Set(StatIds.Iq, 80f);

            float[] children = Enumerable.Range(0, Samples).Select(_ => service.Inherit(new[] { parent, parent }, random).Get(StatIds.Iq).Value).ToArray();

            Assert.That(children.Average(), Is.EqualTo(65).Within(0.3));
            Assert.That(children, Is.All.InRange(0f, 100f));
        }

        [TestCase(0, 0.1f)]
        [TestCase(1, 0.5f)]
        [TestCase(2, 0.75f)]
        public void TraitChance_GrowsWithCarryingParents_ButNeverBelowPrevalence(int carriers, float expected)
        {
            Assert.That(new HeritableTrait("anxious", 0.1f, 0.5f).ChildChance(carriers), Is.EqualTo(expected).Within(0.0001f));
            Assert.That(new HeritableTrait("anxious", 0.6f, 0.1f).ChildChance(carriers), Is.GreaterThanOrEqualTo(0.6f));
        }

        [Test]
        public void TraitRolls_RespectExclusiveGroups()
        {
            var settings = new GeneticsSettings(50f, 15f, null, new[] { new HeritableTrait("anxious", 1f, 1f), new HeritableTrait("calm", 1f, 1f) });
            var service = new GeneticsService(settings, TestCatalogs.Traits());

            IReadOnlyList<string> traits = service.RollFounderTraits(new Pcg32(5UL, 5UL));

            Assert.That(traits, Is.EqualTo(new[] { "anxious" }), "anxious and calm share a group");
        }

        private static GeneticsService Service(float heritability, float noise)
        {
            var settings = new GeneticsSettings(50f, 15f, new[] { new HeritableStat(StatIds.Iq, heritability, noise) }, null);
            return new GeneticsService(settings, TestCatalogs.Traits());
        }
    }
}
