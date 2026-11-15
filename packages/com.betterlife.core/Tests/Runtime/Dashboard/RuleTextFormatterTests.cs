using System.Collections.Generic;
using BetterLife.Core.Karma;
using BetterLife.Core.Localization;
using BetterLife.Core.Rules;
using BetterLife.Core.ViewModels;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Dashboard
{
    /// <summary>Tests for <see cref="RuleTextFormatter"/>.</summary>
    [TestFixture]
    public class RuleTextFormatterTests
    {
        private RuleTextFormatter _formatter;

        [SetUp]
        public void SetUp()
        {
            ILocalizer en = DashboardFixture.With(new Dictionary<string, string>
            {
                [RuleTextKeys.EffectStat] = "{subjectKey} {value}",
                [RuleTextKeys.EffectCashPay] = "Pay {value}",
                [RuleTextKeys.EffectCashGain] = "Get {value}",
                [KarmaTextKeys.EffectMerit] = "Merit {value}",
                [RuleTextKeys.EffectHealthConditionAdd] = "Caught {subject}",
                ["stat.fitness.name"] = "Fitness",
                [MoneyFormatter.FullKey] = "฿{value}",
            });
            _formatter = new RuleTextFormatter(en, new MoneyFormatter(en), real => real * 2m);
        }

        [Test]
        public void KeyArguments_AreLocalized_AndStatChangesAreSigned()
        {
            Assert.That(_formatter.Format(Stat("5")), Is.EqualTo("Fitness +5"));
            Assert.That(_formatter.Format(Stat("-3.5")), Is.EqualTo("Fitness -3.5"));
            Assert.That(_formatter.Format(RuleText.Of(KarmaTextKeys.EffectMerit, (RuleTextKeys.ArgValue, "6"))), Is.EqualTo("Merit +6"));
            Assert.That(_formatter.Format(RuleText.Of(RuleTextKeys.EffectHealthConditionAdd, (RuleTextKeys.ArgSubject, "flu"))), Is.EqualTo("Caught flu"),
                "plain arguments stay as they are");
        }

        [Test]
        public void Ranges_BecomeOneValue()
        {
            RuleText range = RuleText.Of(RuleTextKeys.EffectStat, (RuleTextKeys.ArgSubjectKey, "stat.fitness.name"),
                (RuleTextKeys.ArgMin, "-2"), (RuleTextKeys.ArgMax, "4"));
            Assert.That(_formatter.Format(range), Is.EqualTo("Fitness -2–+4"));
        }

        [Test]
        public void Money_IsShownAtTodaysPrices_WithoutSign()
        {
            Assert.That(_formatter.Format(RuleText.Of(RuleTextKeys.EffectCashGain, (RuleTextKeys.ArgValue, "1500"))), Is.EqualTo("Get ฿3,000"));
            RuleText pay = RuleText.Of(RuleTextKeys.EffectCashPay, (RuleTextKeys.ArgMin, "-500"), (RuleTextKeys.ArgMax, "-100"));
            Assert.That(_formatter.Format(pay), Is.EqualTo("Pay ฿200–฿1,000"), "payment previews are negative ranges; shown low to high");
        }

        [Test]
        public void FormatAll_HandlesNull()
        {
            Assert.That(_formatter.FormatAll(null), Is.Empty);
            Assert.That(_formatter.FormatAll(new[] { Stat("1"), Stat("2") }), Is.EqualTo(new[] { "Fitness +1", "Fitness +2" }));
        }

        private static RuleText Stat(string value)
        {
            return RuleText.Of(RuleTextKeys.EffectStat, (RuleTextKeys.ArgSubjectKey, "stat.fitness.name"), (RuleTextKeys.ArgValue, value));
        }
    }
}
