using BetterLife.Core.Economy;
using BetterLife.Core.Rules.Serialization;
using BetterLife.Core.Tests.Rules;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Economy
{
    /// <summary>Tests for the netWorth and totalDebt conditions.</summary>
    [TestFixture]
    public class FinanceConditionTests
    {
        private RuleFixture _f;

        [SetUp]
        public void SetUp()
        {
            _f = new RuleFixture();
            _f.Character.Wallet.Deposit(1000m);
            _f.Character.Finances.Debts.Add(id => new Debt(id, "loan_shark", DebtKind.Informal, 5000m, 2.4f, 0m, 0, 0m, null, RuleFixture.Now));
        }

        [TestCase("{'type':'netWorth','op':'<','amount':0}", true)]
        [TestCase("{'type':'netWorth','op':'==','amount':-4000}", true)]
        [TestCase("{'type':'netWorth','op':'>=','amount':0}", false)]
        [TestCase("{'type':'totalDebt','op':'>','amount':1000}", true)]
        [TestCase("{'type':'totalDebt','op':'>','amount':1000,'kind':'informal'}", true)]
        [TestCase("{'type':'totalDebt','op':'>','amount':0,'kind':'mortgage'}", false)]
        public void Evaluates(string json, bool expected)
        {
            Assert.That(_f.Condition(json).Evaluate(_f.Context), Is.EqualTo(expected));
            Assert.That(_f.Condition(json).Explain(_f.Context).Passed, Is.EqualTo(expected));
        }

        [Test]
        public void UnknownDebtKind_IsAParseError()
        {
            var ex = Assert.Throws<RuleParseException>(() => _f.Condition("{'type':'totalDebt','op':'>','amount':0,'kind':'casino'}"));

            Assert.That(ex.Message, Does.Contain("casino"));
        }
    }
}
