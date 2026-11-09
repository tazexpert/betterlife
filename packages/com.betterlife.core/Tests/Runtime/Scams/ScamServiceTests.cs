using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Relationships;
using BetterLife.Core.Scams;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Scams
{
    /// <summary>Tests for <see cref="ScamService"/>.</summary>
    [TestFixture]
    public class ScamServiceTests
    {
        [Test]
        public void Targeting_MultipliesAgeLonelinessTimeOnlineMoneyAndSituation()
        {
            var young = new ScamFixture();
            var elder = new ScamFixture(2065);
            elder.MakeTarget();
            elder.Character.Wallet.Deposit(100000m);
            elder.Hours(socialize: 30);

            var lines = elder.Service.Targeting("phone_gang").Multipliers.ToDictionary(l => l.Key, l => l.Multiplier);

            Assert.That(young.Service.Targeting("phone_gang").Multipliers.Single(l => l.Key == ScamTargetingBreakdown.AgeKey).Multiplier, Is.EqualTo(1f));
            Assert.That(lines[ScamTargetingBreakdown.AgeKey], Is.EqualTo(2.5f));
            Assert.That(lines[ScamTargetingBreakdown.LonelinessKey], Is.EqualTo(1f + 20f / 100f).Within(1e-4));
            Assert.That(lines[ScamTargetingBreakdown.OnlineKey], Is.EqualTo(1f + 20 * 0.02f).Within(1e-4));
            Assert.That(lines[ScamTargetingBreakdown.WealthKey], Is.EqualTo(1.5f).Within(1e-4), "ten times the reference");
            Assert.That(lines[ScamTargetingBreakdown.SituationKey], Is.EqualTo(3f));
            Assert.That(elder.Service.Targeting("phone_gang").MonthlyChance, Is.EqualTo(0.5f), "capped");
        }

        [Test]
        public void Detection_LosesPointsToStress_AndGainsFromPeopleWhoWarn()
        {
            var f = new ScamFixture();
            f.ChangeStat(StatIds.Stress, 60f);
            f.Add(RelationshipType.Parent, 70f);
            f.Add(RelationshipType.Friend, 55f);
            f.Add(RelationshipType.Contact, 90f);

            ScamDetectionQuote quote = f.Service.Detection("phone_gang");

            Assert.That(quote.StressAdjustment, Is.EqualTo(-0.2f * 30f).Within(1e-3));
            Assert.That(quote.SupportAdjustment, Is.EqualTo(10f), "contacts do not warn");
            Assert.That(quote.Chance.Final, Is.EqualTo(50f + (50f - 50f) * 0.5f - 6f + 10f).Within(1f));
        }

        [Test]
        public void BeingScammed_TakesCashThenSavings_AndTeachesALesson()
        {
            var f = new ScamFixture(2065);
            f.MakeTarget();
            f.Character.Wallet.Deposit(10000m);
            BetterLife.Core.Economy.Holding savings = f.Deposit(20000m);
            f.ChangeStat(StatIds.StreetSmarts, -50f);
            var scammed = new List<ScammedEvent>();
            f.Bus.Subscribe<ScammedEvent>(scammed.Add);
            float stress = f.Stat(StatIds.Stress);

            for (int i = 0; i < 60 && scammed.Count == 0; i++)
            {
                f.Month();
            }

            ScamIncident incident = scammed.Single().Incident;
            Assert.That(incident.Lost, Is.EqualTo(10000m * 0.5m + 20000m * 0.5m));
            Assert.That(f.Character.Wallet.Cash, Is.EqualTo(0m));
            Assert.That(savings.Value, Is.EqualTo(15000m));
            Assert.That(f.Stat(StatIds.StreetSmarts), Is.EqualTo(15f).Within(1e-3), "0 after the setup, +15 lesson");
            Assert.That(f.Stat(StatIds.Stress), Is.GreaterThan(stress));
            Assert.That(f.LastTick.Interrupts.Single().Severity, Is.EqualTo(Severity.Normal), "below the major loss line");
            Assert.That((f.Character.Scams.Victimized, f.Character.Scams.NetLost), Is.EqualTo((1, 15000m)));
        }

        [Test]
        public void BigLosses_StopTime_AndTheMinimumIsLimitedToWhatIsThere()
        {
            var rich = new ScamFixture(2065);
            var poor = new ScamFixture(2065);
            foreach (ScamFixture f in new[] { rich, poor })
            {
                f.MakeTarget();
                f.ChangeStat(StatIds.StreetSmarts, -50f);
            }
            rich.Character.Wallet.Deposit(100000m);
            poor.Character.Wallet.Deposit(500m);

            for (int i = 0; i < 60 && rich.Character.Scams.Victimized == 0; i++)
            {
                rich.Month();
            }
            for (int i = 0; i < 60 && poor.Character.Scams.Victimized == 0; i++)
            {
                poor.Month();
            }

            Assert.That(rich.Character.Scams.Incidents.Single().Lost, Is.EqualTo(50000m));
            Assert.That(rich.LastTick.Interrupts.Single().Severity, Is.EqualTo(Severity.Major));
            Assert.That(poor.Character.Scams.Incidents.Single().Lost, Is.EqualTo(500m), "the 1,000 minimum cannot take more than there is");
        }

        [Test]
        public void StreetSmartPeople_SpotMostScams()
        {
            var f = new ScamFixture(2065);
            f.MakeTarget();
            f.ChangeStat(StatIds.StreetSmarts, 50f);
            f.Character.Wallet.Deposit(1000000m);
            var spotted = new List<ScamDetectedEvent>();
            f.Bus.Subscribe<ScamDetectedEvent>(spotted.Add);

            for (int i = 0; i < 60; i++)
            {
                f.Month();
            }

            Assert.That(f.Character.Scams.Targeted, Is.GreaterThan(10));
            Assert.That(f.Character.Scams.Detected, Is.EqualTo(spotted.Count));
            Assert.That(spotted.Count, Is.GreaterThan(f.Character.Scams.Victimized));
        }

        [Test]
        public void Children_AreNotTargetedByAdultScams()
        {
            var f = new ScamFixture(2010);
            f.MakeTarget();
            for (int i = 0; i < 60; i++)
            {
                f.Month();
            }

            Assert.That(f.Character.Scams.Targeted, Is.EqualTo(0));
        }

        [Test]
        public void Reporting_CanRecoverHalf_OnlyOnce_AndOnlyInTime()
        {
            int recovered = 0;
            for (ulong seed = 1; seed <= 40; seed++)
            {
                var f = new ScamFixture(seed: seed);
                ScamIncident incident = f.Character.Scams.AddIncident("phone_gang", f.Clock.Now, 10000m, 3);
                ScamReportResult result = f.Service.Report(incident.Id);
                Assert.That(result.Reported, Is.True);
                if (result.Recovered > 0m)
                {
                    recovered++;
                    Assert.That((result.Recovered, f.Character.Wallet.Cash, f.Character.Scams.NetLost), Is.EqualTo((5000m, 5000m, 5000m)));
                }
                Assert.That(f.Service.Report(incident.Id).ReasonKeys, Is.EqualTo(new[] { ScamReportResult.ReasonReported }));
            }
            var late = new ScamFixture();
            ScamIncident old = late.Character.Scams.AddIncident("phone_gang", late.Clock.Now.AddWeeks(-5), 10000m, 3);

            Assert.That(recovered, Is.InRange(10, 35), "about 60%");
            Assert.That(late.Service.Report(old.Id).ReasonKeys, Is.EqualTo(new[] { ScamReportResult.ReasonTooLate }));
            Assert.That(late.Service.Report(999).ReasonKeys, Is.EqualTo(new[] { ScamReportResult.ReasonUnknown }));
        }

        [Test]
        public void Incidents_KeepTheNewest_ButCountEveryVictimization()
        {
            var scams = new CharacterScams();
            for (int i = 0; i < 5; i++)
            {
                scams.AddIncident("phone_gang", BetterLife.Core.Time.GameDate.FromYearWeek(2030, i), 100m, 3);
            }

            Assert.That(scams.Incidents.Select(i => i.Id), Is.EqualTo(new[] { 3, 4, 5 }));
            Assert.That((scams.Victimized, scams.NetLost), Is.EqualTo((5, 500m)));
        }
    }
}
