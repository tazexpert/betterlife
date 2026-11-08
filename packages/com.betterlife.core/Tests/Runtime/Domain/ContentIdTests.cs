using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Skills;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Domain
{
    /// <summary>Tests for <see cref="ContentId"/> and <see cref="DefinitionCatalog{T}"/>.</summary>
    [TestFixture]
    public class ContentIdTests
    {
        [TestCase("health")]
        [TestCase("health.fitness")]
        [TestCase("a1_b.c2")]
        public void IsValid_AcceptsWellFormedIds(string id)
        {
            Assert.That(ContentId.IsValid(id), Is.True);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("Health")]
        [TestCase("1health")]
        [TestCase("health.")]
        [TestCase("health..fitness")]
        [TestCase("health fitness")]
        [TestCase("สุขภาพ")]
        public void IsValid_RejectsMalformedIds(string id)
        {
            Assert.That(ContentId.IsValid(id), Is.False);
            Assert.Throws<ArgumentException>(() => ContentId.Require(id, "id"));
        }

        [Test]
        public void Catalog_RejectsDuplicates_AndUnknownLookups()
        {
            var skill = new SkillDefinition("cooking", "skill.cooking.name", "life", 1f, new SkillDecayRule(1, 1f, 0f));

            Assert.Throws<ArgumentException>(() => new DefinitionCatalog<SkillDefinition>(new[] { skill, skill }));
            var catalog = new DefinitionCatalog<SkillDefinition>(new[] { skill });
            Assert.That(catalog.Get("cooking"), Is.SameAs(skill));
            Assert.That(catalog.Contains("missing"), Is.False);
            Assert.Throws<KeyNotFoundException>(() => catalog.Get("missing"));
        }
    }
}
