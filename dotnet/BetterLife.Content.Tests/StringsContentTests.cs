using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Localization;
using BetterLife.Core.Navigation;
using BetterLife.Core.ViewModels;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Checks the real string tables: every language has the same keys, and every key the shell uses exists.
    /// </summary>
    [TestFixture]
    public class StringsContentTests
    {
        private StringTables _tables;

        [OneTimeSetUp]
        public void Load()
        {
            _tables = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository().Get<StringTables>();
        }

        [Test]
        public void EveryLanguage_HasTheSameKeys()
        {
            StringTable english = _tables.Find("en");
            Assert.That(_tables.Languages, Is.EquivalentTo(new[] { "en", "th" }));
            foreach (string language in _tables.Languages)
            {
                Assert.That(_tables.Find(language).Keys, Is.EquivalentTo(english.Keys), $"{language} keys differ from en");
            }
        }

        [Test]
        public void ShellKeys_AllHaveText()
        {
            var keys = ShellTabs.All.Select(t => t.LabelKey)
                .Concat(new[] { TopBarViewModel.NameKey, TopBarViewModel.AgeKey, TopBarViewModel.DateKey })
                .Concat(Enumerable.Range(1, 12).Select(m => TopBarViewModel.MonthKeyPrefix + m))
                .Concat(new[] { MoneyFormatter.FullKey, MoneyFormatter.ThousandKey, MoneyFormatter.MillionKey, MoneyFormatter.BillionKey, MoneyFormatter.NegativeKey });

            foreach (string language in _tables.Languages)
            {
                Assert.That(keys.Where(k => _tables.Find(language).Find(k) == null), Is.Empty, language);
            }
        }
    }
}
