using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Randomness;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using BetterLife.Core.Tests.Domain;
using BetterLife.Core.Time;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Tests.Rules
{
    /// <summary>
    /// Shared setup for rule tests: a character, a context and a parser.
    /// </summary>
    internal sealed class RuleFixture
    {
        public static readonly GameDate Now = GameDate.FromYearWeek(2026, 10);
        public const string Source = "event.test";

        public RuleFixture(ulong seed = 7UL)
        {
            Catalogs = TestCatalogs.All();
            Character = TestCatalogs.NewCharacter(Catalogs);
            Random = new RandomRegistry(seed).GetStream("events");
            Context = new RuleContext(Character, Now, Random, Source);
            Registry = DefaultRules.CreateRegistry();
        }

        public CharacterCatalogs Catalogs { get; }

        public Character Character { get; }

        public IRandomProvider Random { get; }

        public RuleContext Context { get; }

        public RuleRegistry Registry { get; }

        public ICondition Condition(string json)
        {
            return Registry.ParseCondition(JToken.Parse(json), new RuleReadContext(Catalogs));
        }

        public EffectSequence Effects(string json)
        {
            return Registry.ParseEffects(JToken.Parse(json), new RuleReadContext(Catalogs));
        }
    }
}
