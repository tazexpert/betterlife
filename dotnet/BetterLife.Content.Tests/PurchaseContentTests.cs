using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain;
using BetterLife.Core.Economy;
using BetterLife.Core.Events;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time.Ticking;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Real content for task 26: homes and vehicles on offer, the home owner's saving, and that buying replaces the life
    /// events about a first home or car.
    /// </summary>
    [TestFixture]
    public class PurchaseContentTests
    {
        [Test]
        public void OwningAHome_LowersLivingCosts_AndReplacesTheFirstHomeMilestone()
        {
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            var purchases = content.Get<DefinitionCatalog<PurchaseDefinition>>();
            Assert.That(purchases.All.Count(p => p.Category == PurchaseCategory.Home), Is.EqualTo(4));
            Assert.That(purchases.All.Count(p => p.Category == PurchaseCategory.Vehicle), Is.EqualTo(4));

            using (LifeSession life = AdultLife(content))
            {
                string tier = life.Finance.LifestyleTiers.All.First().Id;
                decimal before = life.Finance.LifestyleCost(tier);
                life.Character.Wallet.Deposit(2000000m);

                life.Finance.BuyPurchase("home.condo", financed: false);

                Assert.That(life.Finance.LifestyleCost(tier), Is.EqualTo(before * 0.75m).Within(1m), "no rent to pay");
                LifeEventDefinition milestone = content.Get<DefinitionCatalog<LifeEventDefinition>>().Get("adult.first_home");
                Assert.That(life.Character.Flags.IsSet("life.adult.first_home"), Is.True);
                Assert.That(milestone.Condition.Evaluate(new BetterLife.Core.Rules.RuleContext(life.Character, life.TimeState.Clock.Now,
                    new BetterLife.Core.Randomness.Pcg32(1UL, 1UL), "test")), Is.False, "the 'first home loan' event will not happen as well");
            }
        }

        private static LifeSession AdultLife(IContentRepository content)
        {
            for (ulong seed = 77UL; ; seed++)
            {
                CreationResult creation = new CharacterCreator(content).Create(new CreationRequest(seed, CreationMode.Random));
                LifeSession life = LifeSessionFactory.CreateNew(content, creation, new EventBus(), RepoContent.Simulation);
                while (!life.IsOver && life.Character.AgeAt(life.TimeState.Clock.Now) < 26)
                {
                    life.Advance(TimeSkip.Year);
                }
                if (!life.IsOver)
                {
                    return life;
                }
                life.Dispose();
            }
        }
    }
}
