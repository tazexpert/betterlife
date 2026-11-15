using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Events;
using BetterLife.Core.Luck;
using BetterLife.Core.Persistence;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Saving
{
    /// <summary>
    /// Saves and loads whole lives in slots.
    /// </summary>
    public sealed class SaveService
    {
        private readonly ISaveStorage _storage;
        private readonly SaveSerializer _serializer;
        private readonly IContentRepository _content;

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="storage">Where saves live.</param>
        /// <param name="serializer">Save format.</param>
        /// <param name="content">Content in use.</param>
        public SaveService(ISaveStorage storage, SaveSerializer serializer, IContentRepository content)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _content = content ?? throw new ArgumentNullException(nameof(content));
        }

        /// <summary>
        /// Saves a life into a slot.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="slot">Slot name.</param>
        /// <returns>The slot summary.</returns>
        public SaveSummaryDto Save(LifeSession session, string slot)
        {
            SlotNames.Require(slot);
            LifeSaveDto dto = LifeSaveMapper.ToDto(session, _content.ContentVersion);
            _storage.Write(slot, _serializer.Serialize(dto));
            return dto.Summary;
        }

        /// <summary>
        /// Saves a life into the autosave slot.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <returns>The slot summary.</returns>
        public SaveSummaryDto Autosave(LifeSession session) => Save(session, SlotNames.Autosave);

        /// <summary>
        /// Reads a slot without building the life.
        /// </summary>
        /// <param name="slot">Slot name.</param>
        /// <returns>The save.</returns>
        /// <exception cref="SaveLoadException">The slot is empty, damaged or too new.</exception>
        public SaveReadResult Read(string slot)
        {
            string text = _storage.Read(SlotNames.Require(slot)) ?? throw new SaveLoadException(SaveErrorKind.Missing, $"Slot '{slot}' is empty.");
            SaveReadResult result = _serializer.Deserialize(text);
            if (result.Save.ContentVersion > _content.ContentVersion)
            {
                throw new SaveLoadException(SaveErrorKind.TooNew,
                    $"The save uses content version {result.Save.ContentVersion}; this game has {_content.ContentVersion}.");
            }
            return result;
        }

        /// <summary>
        /// Loads a life from a slot.
        /// </summary>
        /// <param name="slot">Slot name.</param>
        /// <param name="bus">Event bus for the new session.</param>
        /// <param name="policy">Interrupt policy, or null for the default.</param>
        /// <param name="bonusSources">Item chance bonus sources, or null.</param>
        /// <param name="extraPhases">Extra tick phases, or null.</param>
        /// <returns>The session and warnings.</returns>
        /// <exception cref="SaveLoadException">The slot cannot be loaded.</exception>
        public LifeSessionLoadResult Load(string slot, IEventBus bus, InterruptPolicy policy = null, IEnumerable<IChanceBonusSource> bonusSources = null,
            IEnumerable<ITickPhase> extraPhases = null)
        {
            return Build(Read(slot).Save, _content, bus, policy, bonusSources, extraPhases);
        }

        /// <summary>
        /// Lists the life saves (not profile slots) with their summaries.
        /// </summary>
        /// <returns>Slots.</returns>
        public IReadOnlyList<SaveSlotInfo> Slots()
        {
            return _storage.Slots().Where(s => s != SlotNames.UndoQuota).Select(slot =>
            {
                try
                {
                    return new SaveSlotInfo(slot, Read(slot).Save.Summary, null);
                }
                catch (SaveLoadException ex)
                {
                    return new SaveSlotInfo(slot, null, ex.Kind);
                }
            }).ToList();
        }

        /// <summary>
        /// Deletes a slot.
        /// </summary>
        /// <param name="slot">Slot name.</param>
        /// <returns>True when deleted.</returns>
        public bool Delete(string slot) => _storage.Delete(SlotNames.Require(slot));

        internal static LifeSessionLoadResult Build(LifeSaveDto save, IContentRepository content, IEventBus bus, InterruptPolicy policy,
            IEnumerable<IChanceBonusSource> bonusSources, IEnumerable<ITickPhase> extraPhases)
        {
            try
            {
                return LifeSessionFactory.Load(content, save, bus, policy, bonusSources, extraPhases);
            }
            catch (FormatException ex)
            {
                throw new SaveLoadException(SaveErrorKind.Invalid, "The save data is invalid: " + ex.Message, ex);
            }
        }
    }
}
