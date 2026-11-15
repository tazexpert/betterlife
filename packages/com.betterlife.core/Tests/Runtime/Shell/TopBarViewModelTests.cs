using BetterLife.Core.Localization;
using BetterLife.Core.Sessions;
using BetterLife.Core.Tests.Saving;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.ViewModels;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Shell
{
    /// <summary>Tests for <see cref="TopBarViewModel"/>.</summary>
    [TestFixture]
    public class TopBarViewModelTests
    {
        [Test]
        public void TopBar_ShowsNameAgeDateEraCashAndInbox()
        {
            ILocalizer keys = new TableLocalizer(new StringTables(new StringTable[0]), "en");
            using (LifeSession life = SaveFixture.NewLife())
            {
                life.Advance(TimeSkip.Year);
                life.Character.Wallet.Deposit(20000m);
                TopBarState state = TopBarViewModel.Build(life, keys, new MoneyFormatter(keys));

                Assert.That(state.Name, Is.EqualTo(TopBarViewModel.NameKey), "no table: keys are shown as they are");
                Assert.That(state.InboxCount, Is.EqualTo(life.Events.Inbox.Count));
                Assert.That(state.CanAdvance, Is.True);
                Assert.That(state.CashText, Is.EqualTo(MoneyFormatter.ThousandKey), "nominal cash is in the thousands");
            }
        }

        [Test]
        public void TopBar_UsesTheLocalizedPatterns()
        {
            var tables = new StringTables(new[]
            {
                new StringTable("en", new System.Collections.Generic.Dictionary<string, string>
                {
                    [TopBarViewModel.NameKey] = "{given} {family}", [TopBarViewModel.AgeKey] = "Age {age}", [TopBarViewModel.DateKey] = "{month} {year}",
                    ["month.m1"] = "January", [MoneyFormatter.FullKey] = "฿{value}",
                }),
            });
            ILocalizer en = new TableLocalizer(tables, "en");
            using (LifeSession life = SaveFixture.NewLife())
            {
                TopBarState state = TopBarViewModel.Build(life, en, new MoneyFormatter(en));
                var now = life.TimeState.Clock.Now;

                Assert.That(state.Name, Is.EqualTo(life.Character.Name.GivenName + " " + life.Character.Name.FamilyName));
                Assert.That(state.AgeText, Is.EqualTo("Age 0"));
                Assert.That(state.DateText, Does.EndWith(" " + now.Year));
                Assert.That(state.CashText, Is.EqualTo("฿0"));
            }
        }
    }
}
