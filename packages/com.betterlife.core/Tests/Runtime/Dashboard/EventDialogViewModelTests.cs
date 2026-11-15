using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Localization;
using BetterLife.Core.Saving;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.ViewModels;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Dashboard
{
    /// <summary>Tests for <see cref="EventDialogViewModel"/> and <see cref="DecisionService"/>.</summary>
    [TestFixture]
    public class EventDialogViewModelTests
    {
        [Test]
        public void Dialog_ShowsVisibleChoices_WithChanceAndLocks()
        {
            ILocalizer en = DashboardFixture.With(new Dictionary<string, string>
            {
                [EventDialogViewModel.ChanceKey] = "Chance {value}%",
                ["rule.effect.stat"] = "{subjectKey} {value}",
                ["stat.health.fitness.name"] = "Fitness",
            });
            using (LifeSession life = DashboardFixture.NewLife())
            {
                int id = Pending(life);
                EventDialogState dialog = EventDialogViewModel.Build(life, id, en, new MoneyFormatter(en));

                Assert.That(dialog.Title, Is.EqualTo("event.test.offer.title"));
                Assert.That(dialog.Body, Is.EqualTo("event.test.offer.body"));
                Assert.That(dialog.Choices.Select(c => c.Id), Is.EqualTo(new[] { "safe", "risky", "rich" }), "hidden locked choice is left out");
                ChoiceState safe = dialog.Choices[0];
                Assert.That(safe.ChanceText, Is.Null);
                Assert.That(safe.SuccessPreview, Is.EqualTo(new[] { "Fitness +2" }));
                ChoiceState risky = dialog.Choices[1];
                Assert.That(risky.ChanceText, Does.StartWith("Chance ").And.EndWith("%"));
                Assert.That(risky.SuccessPreview, Is.EqualTo(new[] { "Fitness +5–+10" }));
                Assert.That(risky.FailurePreview, Is.EqualTo(new[] { "Fitness -10" }));
                ChoiceState rich = dialog.Choices[2];
                Assert.That(rich.IsAvailable, Is.False);
                Assert.That(rich.LockedText, Is.EqualTo(EventDialogViewModel.LockedByKey), "the unmet (visible) cash requirement is listed");

                Assert.That(EventDialogViewModel.Build(life, id + 100, en, null), Is.Null, "not pending");
            }
        }

        [Test]
        public void Answer_TakesACheckpoint_ThenShowsTheResult()
        {
            ILocalizer keys = DashboardFixture.Keys();
            var undo = new UndoService(new SaveSerializer(), new InMemorySaveStorage(), new SystemRealTimeClock());
            var decisions = new DecisionService(undo, DashboardFixture.Content.ContentVersion);
            using (LifeSession life = DashboardFixture.NewLife())
            {
                int id = Pending(life);
                Assert.That(undo.HasSnapshot, Is.False);

                EventResolution resolution = decisions.Answer(life, id, "risky");
                EventResultState result = EventDialogViewModel.BuildResult(life, resolution, keys, new MoneyFormatter(keys));

                Assert.That(undo.HasSnapshot, Is.True);
                Assert.That(life.Events.Inbox, Is.Empty);
                Assert.That(result.Succeeded, Is.EqualTo(resolution.Succeeded));
                Assert.That(result.ResultText, Is.EqualTo(resolution.Succeeded ? "r.win" : "r.lose"));
                Assert.That(result.RollText, Is.EqualTo(resolution.Succeeded ? EventDialogViewModel.RollSuccessKey : EventDialogViewModel.RollFailureKey));
                Assert.That(result.Lines, Is.EqualTo(new[] { "rule.effect.stat" }), "the applied stat change");
            }
        }

        [Test]
        public void Answer_RejectsLockedChoices_AndEndedLives()
        {
            var decisions = new DecisionService(null, 1);
            using (LifeSession life = DashboardFixture.NewLife())
            {
                int id = Pending(life);
                Assert.Throws<InvalidOperationException>(() => decisions.Answer(life, id, "rich"));
                Assert.Throws<ArgumentNullException>(() => decisions.Answer(null, id, "safe"));
                Assert.That(decisions.Answer(life, id, "safe").Choice.Id, Is.EqualTo("safe"), "works without undo");
            }
        }

        private static int Pending(LifeSession life)
        {
            life.Advance(TimeSkip.Year);
            return life.Events.Inbox.Single().InstanceId;
        }
    }
}
