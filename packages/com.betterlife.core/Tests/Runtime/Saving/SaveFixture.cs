using System;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Events;
using BetterLife.Core.Persistence;
using BetterLife.Core.Saving;
using BetterLife.Core.Sessions;
using BetterLife.Core.Tests.Content;
using BetterLife.Core.Tests.LifeEvents;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Tests.Saving
{
    /// <summary>
    /// Test content with events, a creator, and helpers to run lives and compare their saved state.
    /// </summary>
    internal static class SaveFixture
    {
        private static IContentRepository _content;

        public static IContentRepository Content => _content ?? (_content = TestContent.Load(TestContent.Source("{ 'type': 'events', 'path': 'events/test.json' }")
            .Add("events/test.json", EventFixture.Events(EventFixture.Simple("test.common")))));

        public static LifeSession NewLife(ulong seed = 2026UL, bool ironman = false)
        {
            CreationResult creation = new CharacterCreator(Content).Create(new CreationRequest(seed, CreationMode.Random));
            return LifeSessionFactory.CreateNew(Content, creation, new EventBus(), ironman: ironman);
        }

        public static void Advance(LifeSession life, int years)
        {
            for (int i = 0; i < years && !life.IsOver; i++)
            {
                life.Advance(TimeSkip.Year);
            }
        }

        /// <summary>The whole saved state as text, for comparing two lives.</summary>
        public static string Snapshot(LifeSession life) => new SaveSerializer().Serialize(LifeSaveMapper.ToDto(life, Content.ContentVersion));
    }
}
