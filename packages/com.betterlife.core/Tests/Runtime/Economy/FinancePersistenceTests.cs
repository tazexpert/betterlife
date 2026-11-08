using System;
using System.Linq;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Economy;
using BetterLife.Core.Persistence;
using BetterLife.Core.Tests.Domain;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Economy
{
    /// <summary>Tests for the finance and economy save mappers.</summary>
    [TestFixture]
    public class FinancePersistenceTests
    {
        private static readonly GameDate Week = GameDate.FromYearWeek(2030, 5);

        [Test]
        public void Finances_RoundTripThroughTheCharacterSave()
        {
            Character original = TestCatalogs.NewCharacter();
            CharacterFinances finances = original.Finances;
            finances.SetLifestyle("modest");
            finances.SetBankruptUntil(Week.AddWeeks(520));
            finances.SetLastMonthlyIncome(12345.67m);
            Holding home = finances.Portfolio.Add("condo_city", 1500000.55m, Week);
            Debt mortgage = finances.Debts.Add(id => new Debt(id, "mortgage", DebtKind.Mortgage, 1350000.1m, 0.055f, 7665.43m, 359, 0m, home.Id, Week));
            mortgage.RestoreStatus(2, true);

            CharacterDto dto = CharacterDtoMapper.ToDto(original);
            Character copy = CharacterDtoMapper.FromDto(dto, TestCatalogs.All()).Character;

            CharacterFinances restored = copy.Finances;
            Assert.That(restored.LifestyleTierId, Is.EqualTo("modest"));
            Assert.That(restored.BankruptUntil, Is.EqualTo(Week.AddWeeks(520)));
            Assert.That(restored.LastMonthlyIncome, Is.EqualTo(12345.67m));
            Holding h = restored.Portfolio.Holdings.Single();
            Assert.That((h.Id, h.AssetId, h.Value, h.CostBasis, h.AcquiredOn), Is.EqualTo((home.Id, "condo_city", 1500000.55m, 1500000.55m, Week)));
            Debt d = restored.Debts.Debts.Single();
            Assert.That((d.ProductId, d.Kind, d.Balance, d.MonthlyPayment, d.MonthsRemaining, d.CollateralHoldingId, d.MissedPayments, d.IsDefaulted),
                Is.EqualTo(("mortgage", DebtKind.Mortgage, 1350000.1m, 7665.43m, 359, (int?)home.Id, 2, true)));
            Assert.That(d.AnnualRate, Is.EqualTo(0.055f));
            Assert.That(restored.Portfolio.NextId, Is.EqualTo(finances.Portfolio.NextId));
            Assert.That(restored.Debts.NextId, Is.EqualTo(finances.Debts.NextId));
        }

        [Test]
        public void OldSaveWithoutFinances_LoadsEmpty()
        {
            CharacterDto dto = CharacterDtoMapper.ToDto(TestCatalogs.NewCharacter());
            dto.Finances = null;

            Character copy = CharacterDtoMapper.FromDto(dto, TestCatalogs.All()).Character;

            Assert.That(copy.Finances.LifestyleTierId, Is.Null);
            Assert.That(copy.Finances.Debts.Debts, Is.Empty);
        }

        [Test]
        public void InvalidMoney_IsAFormatError()
        {
            Character original = TestCatalogs.NewCharacter();
            original.Finances.Portfolio.Add("index_fund", 100m, Week);
            CharacterDto dto = CharacterDtoMapper.ToDto(original);
            dto.Finances.Holdings[0].Value = "lots";

            Assert.Throws<FormatException>(() => CharacterDtoMapper.FromDto(dto, TestCatalogs.All()));
        }

        [Test]
        public void DropUnknown_RemovesMissingContent_AndReleasesCollateral()
        {
            var finances = new CharacterFinances();
            finances.SetLifestyle("old_tier");
            Holding yacht = finances.Portfolio.Add("yacht", 100m, Week);
            finances.Debts.Add(id => new Debt(id, "mortgage", DebtKind.Mortgage, 50m, 0.05f, 1m, 10, 0m, yacht.Id, Week));
            finances.Debts.Add(id => new Debt(id, "payday_loan", DebtKind.Informal, 50m, 1f, 0m, 0, 0m, null, Week));

            var warnings = FinancesDtoMapper.DropUnknown(finances, id => id != "yacht", id => id == "mortgage", id => false);

            Assert.That(warnings, Has.Count.EqualTo(3));
            Assert.That(finances.Portfolio.Holdings, Is.Empty);
            Assert.That(finances.Debts.Debts.Single().CollateralHoldingId, Is.Null);
            Assert.That(finances.LifestyleTierId, Is.Null);
        }

        [Test]
        public void EconomyState_RoundTrips_AndFallsBackToTheFirstPhase()
        {
            EconomySettings settings = FinanceFixture.Content.Get<EconomySettings>();
            var state = new EconomyState("recession", 2, 0.01f, 0.015f, 0.0734m);

            EconomyStateDto dto = EconomyStateDtoMapper.ToDto(state);
            EconomyState copy = EconomyStateDtoMapper.FromDto(dto, settings);
            dto.PhaseId = "removed_phase";
            EconomyState fallback = EconomyStateDtoMapper.FromDto(dto, settings);

            Assert.That((copy.PhaseId, copy.YearsLeftInPhase, copy.AnnualInflation, copy.PolicyRate, copy.PriceLevel),
                Is.EqualTo(("recession", 2, 0.01f, 0.015f, 0.0734m)));
            Assert.That(fallback.PhaseId, Is.EqualTo(settings.Phases.All[0].Id));
        }
    }
}
