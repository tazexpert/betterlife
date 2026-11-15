using System.Collections.Generic;
using BetterLife.Core.Persistence;
using BetterLife.Core.Tests.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Persistence
{
    /// <summary>Tests for <see cref="TimeStateDtoMapper"/>.</summary>
    [TestFixture]
    public class TimeStateDtoMapperTests
    {
        [Test]
        public void RoundTrip_PreservesWeekAndPlan()
        {
            var f = new TimeFixture();
            f.Budget.Set("study", 20);

            TimeStateLoadResult loaded = TimeStateDtoMapper.FromDto(TimeStateDtoMapper.ToDto(f.State), f.Activities, f.Settings);

            Assert.That(loaded.Warnings, Is.Empty);
            Assert.That(loaded.State.Clock.Now, Is.EqualTo(f.State.Clock.Now));
            Assert.That(loaded.State.Budget.GetHours("study"), Is.EqualTo(20));
            Assert.That(loaded.State.Budget.GetHours("sleep"), Is.EqualTo(56));
            Assert.That(loaded.State.Budget.FreeHours, Is.EqualTo(f.Budget.FreeHours));
        }

        [Test]
        public void Load_SkipsRemovedActivities_WithWarnings()
        {
            var f = new TimeFixture();
            var dto = new TimeStateDto
            {
                CurrentWeek = 10,
                Allocations = new List<TimeAllocationDto>
                {
                    new TimeAllocationDto { ActivityId = "study", Hours = 5 },
                    new TimeAllocationDto { ActivityId = "juggling", Hours = 5 },
                    new TimeAllocationDto { ActivityId = "free_time", Hours = 5 },
                    new TimeAllocationDto { ActivityId = "sleep", Hours = 0 },
                },
            };

            TimeStateLoadResult loaded = TimeStateDtoMapper.FromDto(dto, f.Activities, f.Settings);

            Assert.That(loaded.Warnings, Has.Count.EqualTo(3));
            Assert.That(loaded.State.Budget.GetHours("study"), Is.EqualTo(5));
            Assert.That(loaded.State.Clock.Now.TotalWeeks, Is.EqualTo(10));
        }
    }
}
