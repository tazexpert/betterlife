using System;
using BetterLife.Core.Navigation;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Shell
{
    /// <summary>Tests for <see cref="ScreenRouter"/>.</summary>
    [TestFixture]
    public class ScreenRouterTests
    {
        private ScreenRouter _router;
        private int _changes;

        [SetUp]
        public void SetUp()
        {
            _changes = 0;
            _router = new ScreenRouter(ShellTabs.All);
            _router.Changed += () => _changes++;
        }

        [Test]
        public void Starts_AtHome_WithEveryTabAtItsRoot()
        {
            Assert.That(_router.CurrentTab.Id, Is.EqualTo(ScreenIds.Home));
            Assert.That(_router.Current, Is.EqualTo(new ScreenRequest(ScreenIds.Home)));
            Assert.That(_router.Tabs, Has.Count.EqualTo(6));
            Assert.That(_router.CanGoBack, Is.False);
        }

        [Test]
        public void EachTab_KeepsItsOwnHistory()
        {
            _router.SelectTab(ScreenIds.People);
            _router.Push(new ScreenRequest("person", "12"));
            _router.SelectTab(ScreenIds.Money);
            _router.SelectTab(ScreenIds.People);

            Assert.That(_router.Current, Is.EqualTo(new ScreenRequest("person", "12")));
            Assert.That(_router.Depth, Is.EqualTo(2));
            Assert.That(_changes, Is.EqualTo(4));
        }

        [Test]
        public void SelectingTheCurrentTabAgain_GoesBackToItsRoot()
        {
            _router.SelectTab(ScreenIds.People);
            _router.Push(new ScreenRequest("person", "12"));
            _router.Push(new ScreenRequest("interaction", "chat"));

            _router.SelectTab(ScreenIds.People);

            Assert.That(_router.Current, Is.EqualTo(new ScreenRequest(ScreenIds.People)));
            Assert.That(_router.Depth, Is.EqualTo(1));
        }

        [Test]
        public void Back_ClosesModals_ThenPops_ThenReturnsHome_ThenAsksToExit()
        {
            _router.SelectTab(ScreenIds.Work);
            _router.Push(new ScreenRequest("career", "clerk"));
            _router.OpenModal(new ScreenRequest("event", "7"));

            Assert.That(_router.Current.ScreenId, Is.EqualTo("event"));
            Assert.That(_router.Back(), Is.EqualTo(BackResult.ClosedModal));
            Assert.That(_router.Current.ScreenId, Is.EqualTo("career"));
            Assert.That(_router.Back(), Is.EqualTo(BackResult.Popped));
            Assert.That(_router.Back(), Is.EqualTo(BackResult.ReturnedHome));
            Assert.That(_router.CurrentTab.Id, Is.EqualTo(ScreenIds.Home));
            Assert.That(_router.Back(), Is.EqualTo(BackResult.Exit));
        }

        [Test]
        public void SwitchingTabsOrPushing_ClosesModals()
        {
            _router.OpenModal(new ScreenRequest("event", "1"));
            _router.SelectTab(ScreenIds.Money);
            Assert.That(_router.Modal, Is.Null);

            _router.OpenModal(new ScreenRequest("event", "2"));
            _router.Push(new ScreenRequest("asset", "condo"));
            Assert.That(_router.Modal, Is.Null);
            Assert.That(_router.CloseModal(), Is.False);
        }

        [Test]
        public void Reset_ReturnsEverythingToHome()
        {
            _router.SelectTab(ScreenIds.People);
            _router.Push(new ScreenRequest("person", "3"));
            _router.OpenModal(new ScreenRequest("event", "1"));

            _router.Reset();

            Assert.That((_router.CurrentTab.Id, _router.Depth, _router.Modal), Is.EqualTo((ScreenIds.Home, 1, (ScreenRequest)null)));
            _router.SelectTab(ScreenIds.People);
            Assert.That(_router.Depth, Is.EqualTo(1));
        }

        [Test]
        public void InvalidInput_IsRefused()
        {
            Assert.Throws<ArgumentException>(() => _router.SelectTab("casino"));
            Assert.Throws<ArgumentException>(() => new ScreenRouter(new TabDefinition[0]));
            Assert.Throws<ArgumentException>(() => new ScreenRouter(new[] { ShellTabs.All[0], ShellTabs.All[0] }));
        }
    }
}
