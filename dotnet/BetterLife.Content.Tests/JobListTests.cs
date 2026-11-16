using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain.Skills;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Events;
using BetterLife.Core.Localization;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.ViewModels;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Real content: a young person without skills is offered part-time jobs first, and jobs that need a skill (chef,
    /// smuggling, pro sport, celebrity chef) are not listed until they have it.
    /// </summary>
    [TestFixture]
    public class JobListTests
    {
        private const string CookingSkill = "cooking";
        private const float ChoresCookingLimit = 30f;


        [Test]
        public void Beginners_SeePartTimeJobsFirst_AndNoSkilledJobs()
        {
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            var th = new TableLocalizer(content.Get<StringTables>(), "th", "en");
            for (ulong seed = 20UL; ; seed++)
            {
                CreationResult creation = new CharacterCreator(content).Create(new CreationRequest(seed, CreationMode.Random));
                using (LifeSession life = LifeSessionFactory.CreateNew(content, creation, new EventBus(), RepoContent.Simulation))
                {
                    while (!life.IsOver && life.Character.AgeAt(life.TimeState.Clock.Now) < 18)
                    {
                        life.Advance(TimeSkip.Year);
                    }
                    if (life.IsOver)
                    {
                        continue;
                    }
                    var c = life.Character;
                    c.Stats.SetStored(StatIds.Fitness, 50f);
                    c.Stats.SetStored(new StatId("hidden.street_smarts"), 30f);
                    var jobs = WorkViewModel.Build(life, th, new MoneyFormatter(th)).JobOptions.ToList();
                    TestContext.WriteLine($"cooking {c.Skills.GetLevel(CookingSkill):0.0}");
                    TestContext.WriteLine(string.Join(" | ", jobs.Select(j => j.Title + " — " + j.Detail)));

                    Assert.That(jobs.Select(j => j.TrackId), Does.Not.Contain("chef").And.Not.Contain("smuggler").And.Not.Contain("pro_athlete").And.Not.Contain("celebrity_chef"));
                    Assert.That(jobs.Take(3).Select(j => j.TrackId), Is.EquivalentTo(new[] { "convenience_store", "department_store", "food_delivery" }), "part-time first");
                    Assert.That(c.Skills.GetLevel(CookingSkill), Is.LessThan(ChoresCookingLimit), "household chores alone do not make a chef");
                    Assert.That(jobs.First().Detail, Does.StartWith(th.Get(WorkViewModel.PartTimeKey)));
                    return;
                }
            }
        }
    }
}
