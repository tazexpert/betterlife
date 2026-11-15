using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Careers;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Education;
using BetterLife.Core.Persistence;
using BetterLife.Core.Tests.Domain;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Careers
{
    /// <summary>Tests for the education and career save mappers.</summary>
    [TestFixture]
    public class SchoolWorkPersistenceTests
    {
        private static readonly GameDate Week = GameDate.FromYearWeek(2030, 5);

        [Test]
        public void EducationAndCareer_RoundTripThroughTheCharacterSave()
        {
            Character original = TestCatalogs.NewCharacter();
            original.Education.Restore(
                new Enrollment("master", "master.private", Week, Week.AddWeeks(52), 1, 1, 77.5f, true, TuitionFunding.StudentLoan),
                new[] { new Credential("bachelor", "bachelor.top", Week.AddWeeks(-10), 81f) },
                new[] { new KeyValuePair<string, GameDate>("master", Week) });
            original.Career.Start(new Job("clerk", 1, Week, Week.AddWeeks(30), 66f, 44f, 2, Week.AddWeeks(100)));
            original.Career.Restore(original.Career.Current,
                new[] { new PastJob("courier", 0, Week.AddWeeks(-100), Week.AddWeeks(-50), JobEndReason.Risk) }, true, 12345.5m,
                new[] { new KeyValuePair<string, GameDate>("clerk", Week) });

            Character copy = CharacterDtoMapper.FromDto(CharacterDtoMapper.ToDto(original), TestCatalogs.All()).Character;

            Enrollment e = copy.Education.Current;
            Assert.That((e.ProgramId, e.InstitutionId, e.StartedOn, e.YearStartedOn, e.YearsCompleted, e.Repeats, e.Performance, e.Scholarship, e.Funding),
                Is.EqualTo(("master", "master.private", Week, Week.AddWeeks(52), 1, 1, 77.5f, true, TuitionFunding.StudentLoan)));
            Credential c = copy.Education.Credentials.Single();
            Assert.That((c.ProgramId, c.InstitutionId, c.GraduatedOn, c.FinalPerformance), Is.EqualTo(("bachelor", "bachelor.top", Week.AddWeeks(-10), 81f)));
            Assert.That(copy.Education.Attempts["master"], Is.EqualTo(Week));
            Job j = copy.Career.Current;
            Assert.That((j.TrackId, j.LevelIndex, j.StartedOn, j.LevelSince, j.Performance, j.BossSatisfaction, j.LowBossMonths, j.NextReviewOn),
                Is.EqualTo(("clerk", 1, Week, Week.AddWeeks(30), 66f, 44f, 2, (GameDate?)Week.AddWeeks(100))));
            PastJob p = copy.Career.History.Single();
            Assert.That((p.TrackId, p.Reason, p.EndedOn), Is.EqualTo(("courier", JobEndReason.Risk, Week.AddWeeks(-50))));
            Assert.That((copy.Career.IsRetired, copy.Career.PensionMonthly), Is.EqualTo((true, 12345.5m)));
            Assert.That(copy.Career.Attempts["clerk"], Is.EqualTo(Week));
        }

        [Test]
        public void OldSaves_LoadEmpty_AndBadEnumsAreFormatErrors()
        {
            CharacterDto dto = CharacterDtoMapper.ToDto(TestCatalogs.NewCharacter());
            dto.Education = null;
            dto.Career = null;
            Character copy = CharacterDtoMapper.FromDto(dto, TestCatalogs.All()).Character;

            Character original = TestCatalogs.NewCharacter();
            original.Career.Start(new Job("clerk", 0, Week, Week, 50f, 50f, 0, null));
            original.Career.End(JobEndReason.Quit, Week);
            CharacterDto bad = CharacterDtoMapper.ToDto(original);
            bad.Career.History[0].Reason = "Abducted";

            Assert.That(copy.Education.IsEnrolled || copy.Career.IsEmployed, Is.False);
            Assert.Throws<FormatException>(() => CharacterDtoMapper.FromDto(bad, TestCatalogs.All()));
        }

        [Test]
        public void DropUnknown_RemovesProgramsAndTracksThatNoLongerExist()
        {
            var education = new CharacterEducation();
            education.Restore(new Enrollment("old_school", "old_school.a", Week, Week, 0, 0, 50f, false, TuitionFunding.Cash),
                new[] { new Credential("primary", "primary.public", Week, 50f), new Credential("old_school", "old_school.a", Week, 50f) }, null);
            var career = new CharacterCareer();
            career.Start(new Job("clerk", 5, Week, Week, 50f, 50f, 0, null));
            career.Restore(career.Current, new[] { new PastJob("pirate", 0, Week, Week, JobEndReason.Quit) }, false, 0m, null);

            var educationWarnings = education.DropUnknown(id => id == "primary", (p, i) => true);
            var careerWarnings = career.DropUnknown(id => id == "clerk" ? 2 : (int?)null);

            Assert.That(educationWarnings, Has.Count.EqualTo(2));
            Assert.That(education.IsEnrolled, Is.False);
            Assert.That(education.Credentials.Single().ProgramId, Is.EqualTo("primary"));
            Assert.That(careerWarnings, Has.Count.EqualTo(2), "level 5 does not exist and 'pirate' is unknown");
            Assert.That(career.IsEmployed || career.History.Count > 0, Is.False);
        }
    }
}
