using System.Collections.Generic;
using BetterLife.Core.Localization;
using BetterLife.Core.Rules;
using BetterLife.Core.ViewModels;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Shell
{
    /// <summary>Tests for <see cref="TableLocalizer"/> and <see cref="MoneyFormatter"/>.</summary>
    [TestFixture]
    public class LocalizationTests
    {
        private static StringTables Tables()
        {
            return new StringTables(new[]
            {
                new StringTable("en", new Dictionary<string, string>
                {
                    ["nav.home"] = "Home", ["topbar.age"] = "Age {age}", ["only.english"] = "English",
                    ["money.full"] = "฿{value}", ["money.compact.thousand"] = "฿{value}K", ["money.compact.million"] = "฿{value}M",
                    ["money.compact.billion"] = "฿{value}B", ["money.negative"] = "-{value}",
                }),
                new StringTable("th", new Dictionary<string, string> { ["nav.home"] = "หน้าหลัก", ["topbar.age"] = "อายุ {age} ปี" }),
            });
        }

        [Test]
        public void Localizer_UsesTheLanguage_ThenTheFallback_ThenTheKey()
        {
            var thai = new TableLocalizer(Tables(), "th", "en");

            Assert.That(thai.Language, Is.EqualTo("th"));
            Assert.That(thai.Get("nav.home"), Is.EqualTo("หน้าหลัก"));
            Assert.That(thai.Get("only.english"), Is.EqualTo("English"));
            Assert.That(thai.Get("not.translated"), Is.EqualTo("not.translated"));
            Assert.That(thai.Format("topbar.age", ("age", "30")), Is.EqualTo("อายุ 30 ปี"));
            Assert.That(thai.Format(RuleText.Of("topbar.age", ("age", "5"))), Is.EqualTo("อายุ 5 ปี"));
            Assert.That(new TableLocalizer(Tables(), "jp", "en").Get("nav.home"), Is.EqualTo("Home"), "unknown language falls back");
        }

        [Test]
        public void StringTables_RefuseDuplicateLanguagesAndBadKeys()
        {
            var table = new StringTable("en", new Dictionary<string, string>());

            Assert.Throws<System.ArgumentException>(() => new StringTables(new[] { table, table }));
            Assert.Throws<System.ArgumentException>(() => new StringTable("en", new Dictionary<string, string> { ["Bad Key"] = "x" }));
        }

        [TestCase(0, "฿0")]
        [TestCase(999.99, "฿999")]
        [TestCase(12345, "฿12.3K")]
        [TestCase(4560000, "฿4.5M")]
        [TestCase(2500000000.0, "฿2.5B")]
        [TestCase(-15000, "-฿15K")]
        [TestCase(-0.5, "฿0")]
        public void Compact_ShortensLargeAmounts(decimal amount, string expected)
        {
            Assert.That(new MoneyFormatter(new TableLocalizer(Tables(), "en")).Compact(amount), Is.EqualTo(expected));
        }

        [Test]
        public void Full_GroupsDigits()
        {
            var money = new MoneyFormatter(new TableLocalizer(Tables(), "en"));

            Assert.That(money.Full(1234567.89m), Is.EqualTo("฿1,234,567"));
            Assert.That(money.Full(-2500m), Is.EqualTo("-฿2,500"));
        }
    }
}
