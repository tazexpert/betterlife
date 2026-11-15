using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Localization;
using BetterLife.Core.Rules;
using BetterLife.Core.Saving;
using BetterLife.Core.Settings;
using BetterLife.Core.Tests.Dashboard;
using BetterLife.Core.ViewModels;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Settings
{
    /// <summary>Tests for settings, language choice, subject names in rule texts.</summary>
    [TestFixture]
    public class SettingsTests
    {
        [Test]
        public void Store_RoundTrips_AndFallsBackToDefaults()
        {
            var storage = new InMemorySaveStorage();
            var store = new SettingsStore(storage);
            Assert.That(store.Load().ViceContent, Is.True, "defaults when nothing is saved");
            Assert.That(store.Load().Language, Is.Null);

            store.Save(new GameSettings { Language = "th", ViceContent = false });
            GameSettings loaded = new SettingsStore(storage).Load();
            Assert.That(loaded.Language, Is.EqualTo("th"));
            Assert.That(loaded.ViceContent, Is.False);

            storage.Write(SlotNames.Settings, "{ not json");
            Assert.That(store.Load().ViceContent, Is.True, "a broken file gives the defaults");
            Assert.That(SlotNames.IsProfile(SlotNames.Settings), Is.True, "not listed as a life save");
        }

        [TestCase("th", "en", "th")]
        [TestCase(null, "th", "th")]
        [TestCase(null, "fr", "en")]
        [TestCase("de", null, "en")]
        public void Language_PrefersTheChoice_ThenTheDevice(string saved, string device, string expected)
        {
            Assert.That(SettingsStore.ResolveLanguage(saved, device, new[] { "en", "th" }, "en"), Is.EqualTo(expected));
        }

        [Test]
        public void Screen_ListsLanguagesByTheirOwnName()
        {
            var tables = new StringTables(new[]
            {
                new StringTable("en", new Dictionary<string, string> { [SettingsViewModel.LanguageSelfKey] = "English" }),
                new StringTable("th", new Dictionary<string, string> { [SettingsViewModel.LanguageSelfKey] = "ไทย" }),
            });
            ILocalizer en = new TableLocalizer(tables, "en");
            SettingsState state = SettingsViewModel.Build(new GameSettings { ViceContent = false }, "en", tables, 2, "1/3", 7, en);

            Assert.That(state.Languages.Select(l => l.Label), Is.EqualTo(new[] { "English", "ไทย" }));
            Assert.That(state.Languages.Single(l => l.Id == "en").IsAvailable, Is.False, "the current language");
            Assert.That(state.ViceToggleText, Is.EqualTo(SettingsViewModel.ViceOnKey));
            Assert.That(state.Info.Select(r => r.Value), Is.EqualTo(new[] { "2", "1/3", "7" }));
        }

        [Test]
        public void RawSubjectIds_GetTheirDisplayName()
        {
            ILocalizer th = DashboardFixture.With(new Dictionary<string, string>
            {
                [RuleTextKeys.CondRelationshipCount] = "{subject}: {op} {value}",
                [RuleTextKeys.CondReputation] = "ชื่อเสียง ({subject})",
                ["relationship.type.spouse"] = "คู่สมรส",
            });
            var formatter = new RuleTextFormatter(th, new MoneyFormatter(th));

            Assert.That(formatter.Format(RuleText.Of(RuleTextKeys.CondRelationshipCount, (RuleTextKeys.ArgSubject, "spouse"),
                (RuleTextKeys.ArgOp, ">="), (RuleTextKeys.ArgValue, "1"))), Is.EqualTo("คู่สมรส: >= 1"));
            Assert.That(formatter.Format(RuleText.Of(RuleTextKeys.CondReputation, (RuleTextKeys.ArgSubject, "politics"))), Is.EqualTo("ชื่อเสียง (politics)"),
                "no name yet: the id stays");
        }
    }
}
