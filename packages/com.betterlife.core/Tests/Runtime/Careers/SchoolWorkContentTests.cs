using System.Linq;
using BetterLife.Core.Careers;
using BetterLife.Core.Content;
using BetterLife.Core.Domain;
using BetterLife.Core.Education;
using BetterLife.Core.Tests.Content;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Careers
{
    /// <summary>Tests for the education and career loaders.</summary>
    [TestFixture]
    public class SchoolWorkContentTests
    {
        [Test]
        public void TestContent_LoadsEveryProduct()
        {
            IContentRepository content = SchoolWorkFixture.Content;
            var programs = content.Get<DefinitionCatalog<EducationProgramDefinition>>();
            var tracks = content.Get<DefinitionCatalog<CareerTrackDefinition>>();

            Assert.That(programs.Get("bachelor").Institutions.Get("bachelor.top").Difficulty, Is.EqualTo(75f));
            Assert.That(programs.Get("bachelor").MaxEntryAge, Is.Null);
            Assert.That(tracks.Get("clerk").Requirement, Is.Not.Null);
            Assert.That(tracks.Get("courier").Risk.Effects, Is.Not.Null);
            Assert.That(tracks.Get("courier").HasVariableIncome, Is.True);
            Assert.That(content.Get<EducationSettings>().Admission.RetryWeeks, Is.EqualTo(52));
            Assert.That(content.Get<CareerSettings>().RetirementAge, Is.EqualTo(60));
        }

        [TestCase("education_programs.json", "'requiresRank': 2,", "'requiresRank': 9,", "requiresRank must be below rank")]
        [TestCase("education_programs.json", "'skill': 'cooking', 'gainPerHour': 0.01", "'skill': 'juggling', 'gainPerHour': 0.01", "juggling")]
        [TestCase("education_settings.json", "'schoolActivity': 'school'", "'schoolActivity': 'classes'", "unknown activity 'classes'")]
        [TestCase("education_settings.json", "'studentLoanProduct': 'student_loan'", "'studentLoanProduct': 'credit_card'", "must be an installment loan")]
        [TestCase("education_settings.json", "'attendanceWeight': 0.45", "'attendanceWeight': 0.9", "sum to 1")]
        [TestCase("careers.json", "'completed': 'bachelor'", "'completed': 'wizardry'", "unknown education program 'wizardry'")]
        [TestCase("careers.json", "'category': 'illegal'", "'category': 'piracy'", "'piracy' is not one of")]
        [TestCase("careers.json", "'type': 'cash', 'amount': -5000", "'type': 'cashh', 'amount': -5000", "cashh")]
        [TestCase("career_settings.json", "'phase': 'recession'", "'phase': 'depression'", "unknown economy phase 'depression'")]
        [TestCase("career_settings.json", "'retirementAge': 60", "'retirementAge': 10", "out of range")]
        public void InvalidContent_IsReported(string file, string find, string replace, string message)
        {
            string original = Original(file);
            Assert.That(original, Does.Contain(find));
            InMemoryContentSource source = TestContent.Source().Add(file, original.Replace(find, replace));

            ContentLoadResult result = DefaultContent.CreatePipeline(source).Load();

            Assert.That(result.Errors.Select(e => e.Message), Has.Some.Contains(message), string.Join("\n", result.Errors));
        }

        [Test]
        public void Activities_CanUseCareerConditions_FromTracksLoadedEarlier()
        {
            InMemoryContentSource source = TestContent.Source().Add("activities.json",
                TestContent.Activities.Replace("{ 'type': 'career', 'employed': true }", "{ 'type': 'career', 'track': 'clerk', 'minLevel': 'manager' }"));

            ContentLoadResult result = DefaultContent.CreatePipeline(source).Load();

            Assert.That(result.Errors, Is.Empty, string.Join("\n", result.Errors));
        }

        private static string Original(string file)
        {
            switch (file)
            {
                case "education_programs.json": return TestContent.EducationPrograms;
                case "education_settings.json": return TestContent.EducationSettings;
                case "careers.json": return TestContent.Careers;
                default: return TestContent.CareerSettings;
            }
        }
    }
}
