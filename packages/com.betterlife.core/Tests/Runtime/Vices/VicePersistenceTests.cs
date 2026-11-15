using System;
using System.Linq;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Persistence;
using BetterLife.Core.Tests.Domain;
using BetterLife.Core.Time;
using BetterLife.Core.Vices;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Vices
{
    /// <summary>Tests for the vices save mapper.</summary>
    [TestFixture]
    public class VicePersistenceTests
    {
        private static readonly GameDate Week = GameDate.FromYearWeek(2031, 3);

        [Test]
        public void Vices_RoundTripThroughTheCharacterSave()
        {
            Character original = TestCatalogs.NewCharacter();
            original.Vices.Restore(new[]
            {
                new ViceState("gambling", 44.5f, 0f, 30, Week, 30, -12345.67m, true, 2, 1, new QuitAttempt("rehab", Week, Week.AddWeeks(6))),
                new ViceState("smoking", 3f),
            }, Week);

            Character copy = CharacterDtoMapper.FromDto(CharacterDtoMapper.ToDto(original), TestCatalogs.All()).Character;

            ViceState g = copy.Vices.Find("gambling");
            Assert.That((g.Addiction, g.Uses, g.LastUsedOn, g.Bets, g.NetWinnings, g.Discovered, g.Relapses, g.Quits),
                Is.EqualTo((44.5f, 30, (GameDate?)Week, 30, -12345.67m, true, 2, 1)));
            Assert.That((g.Quitting.MethodId, g.Quitting.StartedOn, g.Quitting.EndsOn), Is.EqualTo(("rehab", Week, Week.AddWeeks(6))));
            Assert.That(copy.Vices.Find("smoking").LastUsedOn, Is.Null);
            Assert.That(copy.Vices.LastIrreversibleWeek, Is.EqualTo(Week));
        }

        [Test]
        public void OldSaves_LoadEmpty_AndUnknownVicesCanBeDropped()
        {
            CharacterDto dto = CharacterDtoMapper.ToDto(TestCatalogs.NewCharacter());
            dto.Vices = null;
            Character copy = CharacterDtoMapper.FromDto(dto, TestCatalogs.All()).Character;
            var vices = new CharacterVices();
            vices.Get("drinking");
            vices.Get("old_vice");

            var warnings = vices.DropUnknown(id => id == "drinking");

            Assert.That(copy.Vices.States, Is.Empty);
            Assert.That(warnings.Single(), Does.Contain("old_vice"));
            Assert.That(vices.States.Select(s => s.ViceId), Is.EqualTo(new[] { "drinking" }));
        }
    }
}
