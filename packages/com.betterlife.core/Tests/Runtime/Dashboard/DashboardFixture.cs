using System.Collections.Generic;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Events;
using BetterLife.Core.Localization;
using BetterLife.Core.Sessions;
using BetterLife.Core.Tests.Content;
using BetterLife.Core.Tests.LifeEvents;

namespace BetterLife.Core.Tests.Dashboard
{
    /// <summary>
    /// A life whose content has one major choice event that fires in the first week, plus localizer helpers.
    /// </summary>
    internal static class DashboardFixture
    {
        public const string ChoiceEvent = @"{ 'id': 'test.offer', 'titleKey': 'event.test.offer.title', 'bodyKey': 'event.test.offer.body',
            'category': 'test', 'trigger': 'random', 'severity': 'major', 'tone': 'neutral', 'cooldownWeeks': 1000, 'maxOccurrences': 1, 'defaultChoice': 'safe',
            'choices': [
              { 'id': 'safe', 'textKey': 'c.safe', 'success': { 'resultKey': 'r.safe', 'effects': [ { 'type': 'stat', 'stat': 'health.fitness', 'amount': 2 } ] } },
              { 'id': 'risky', 'textKey': 'c.risky', 'chance': { 'base': 50, 'karma': false },
                'success': { 'resultKey': 'r.win', 'effects': [ { 'type': 'stat', 'stat': 'health.fitness', 'amount': { 'min': 5, 'max': 10 } } ] },
                'failure': { 'resultKey': 'r.lose', 'effects': [ { 'type': 'stat', 'stat': 'health.fitness', 'amount': -10 } ] } },
              { 'id': 'rich', 'textKey': 'c.rich', 'condition': { 'type': 'cash', 'op': '>=', 'amount': 1000000000 },
                'success': { 'resultKey': 'r.rich' } },
              { 'id': 'secret', 'textKey': 'c.secret', 'hideWhenLocked': true, 'condition': { 'type': 'flag', 'flag': 'career.fired_once' },
                'success': { 'resultKey': 'r.secret' } } ] }";

        private static IContentRepository _content;

        public static IContentRepository Content => _content ?? (_content = TestContent.Load(
            TestContent.Source("{ 'type': 'events', 'path': 'events/test.json' }", 1f).Add("events/test.json", EventFixture.Events(ChoiceEvent))));

        public static LifeSession NewLife(ulong seed = 7UL)
        {
            CreationResult creation = new CharacterCreator(Content).Create(new CreationRequest(seed, CreationMode.Random));
            return LifeSessionFactory.CreateNew(Content, creation, new EventBus());
        }

        /// <summary>A localizer without strings: every text is its key.</summary>
        public static ILocalizer Keys() => new TableLocalizer(new StringTables(new StringTable[0]), "en");

        public static ILocalizer With(IDictionary<string, string> strings)
        {
            return new TableLocalizer(new StringTables(new[] { new StringTable("en", new Dictionary<string, string>(strings)) }), "en");
        }
    }
}
