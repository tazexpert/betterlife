using System;
using System.Collections.Generic;
using BetterLife.Core.Content;
using BetterLife.Core.Events;
using BetterLife.Core.Luck;
using BetterLife.Core.Persistence;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time.Ticking;
using Newtonsoft.Json;

namespace BetterLife.Core.Saving
{
    /// <summary>
    /// One-step Undo: a snapshot taken before the latest decision can be restored a few times per real day,
    /// never in Ironman lives and never across an irreversible action (a bet).
    /// </summary>
    public sealed class UndoService
    {
        /// <summary>Undos per real day.</summary>
        public const int DefaultDailyLimit = 3;

        private const int DayYearFactor = 10000;
        private const int DayMonthFactor = 100;

        private readonly SaveSerializer _serializer;
        private readonly ISaveStorage _profile;
        private readonly IRealTimeClock _clock;
        private readonly int _dailyLimit;
        private string _snapshot;
        private int _snapshotIrreversible;

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="serializer">Save format used for snapshots.</param>
        /// <param name="profile">Storage for the daily quota.</param>
        /// <param name="clock">Real-world calendar.</param>
        /// <param name="dailyLimit">Undos per real day.</param>
        public UndoService(SaveSerializer serializer, ISaveStorage profile, IRealTimeClock clock, int dailyLimit = DefaultDailyLimit)
        {
            if (dailyLimit < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dailyLimit), dailyLimit, "Limit must be non-negative.");
            }
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _dailyLimit = dailyLimit;
        }

        /// <summary>True while a snapshot is held.</summary>
        public bool HasSnapshot => _snapshot != null;

        /// <summary>
        /// Remembers the life as it is now, replacing any earlier snapshot. Call before every player decision.
        /// Ironman lives keep no snapshot.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="contentVersion">Version of the content in use.</param>
        public void Checkpoint(LifeSession session, int contentVersion)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (session.IsIronman)
            {
                Clear();
                return;
            }
            _snapshot = _serializer.Serialize(LifeSaveMapper.ToDto(session, contentVersion));
            _snapshotIrreversible = session.Character.Vices.IrreversibleActions;
        }

        /// <summary>
        /// Forgets the snapshot (for example after starting another life).
        /// </summary>
        public void Clear() => _snapshot = null;

        /// <summary>
        /// Checks whether the current life can go back to the snapshot.
        /// </summary>
        /// <param name="current">Running life.</param>
        /// <returns>The quote.</returns>
        public UndoQuote Quote(LifeSession current)
        {
            if (current == null)
            {
                throw new ArgumentNullException(nameof(current));
            }
            int left = Math.Max(0, _dailyLimit - UsedToday());
            var reasons = new List<string>();
            if (current.IsIronman)
            {
                reasons.Add(UndoQuote.ReasonIronman);
            }
            if (_snapshot == null)
            {
                reasons.Add(UndoQuote.ReasonNoSnapshot);
            }
            if (left == 0)
            {
                reasons.Add(UndoQuote.ReasonDailyLimit);
            }
            if (_snapshot != null && current.Character.Vices.IrreversibleActions != _snapshotIrreversible)
            {
                reasons.Add(UndoQuote.ReasonIrreversible);
            }
            return new UndoQuote(reasons, left);
        }

        /// <summary>
        /// Goes back to the snapshot: builds a new session from it, counts today's use and forgets the snapshot.
        /// The caller disposes the old session.
        /// </summary>
        /// <param name="current">Running life.</param>
        /// <param name="content">Content in use.</param>
        /// <param name="bus">Event bus for the new session.</param>
        /// <param name="policy">Interrupt policy, or null for the default.</param>
        /// <param name="bonusSources">Item chance bonus sources, or null.</param>
        /// <param name="extraPhases">Extra tick phases, or null.</param>
        /// <returns>The restored session.</returns>
        public LifeSessionLoadResult Undo(LifeSession current, IContentRepository content, IEventBus bus, InterruptPolicy policy = null,
            IEnumerable<IChanceBonusSource> bonusSources = null, IEnumerable<ITickPhase> extraPhases = null)
        {
            UndoQuote quote = Quote(current);
            if (!quote.Available)
            {
                throw new InvalidOperationException("Undo is not available: " + string.Join(", ", quote.ReasonKeys));
            }
            LifeSessionLoadResult restored = SaveService.Build(_serializer.Deserialize(_snapshot).Save, content, bus, policy, bonusSources, extraPhases);
            RecordUse();
            _snapshot = null;
            return restored;
        }

        private int UsedToday()
        {
            UndoQuotaDto quota = ReadQuota();
            return quota.Day == Today() ? quota.Used : 0;
        }

        private void RecordUse()
        {
            _profile.Write(SlotNames.UndoQuota, JsonConvert.SerializeObject(new UndoQuotaDto { Day = Today(), Used = UsedToday() + 1 }));
        }

        private UndoQuotaDto ReadQuota()
        {
            string text = _profile.Read(SlotNames.UndoQuota);
            if (text == null)
            {
                return new UndoQuotaDto();
            }
            try
            {
                return JsonConvert.DeserializeObject<UndoQuotaDto>(text) ?? new UndoQuotaDto();
            }
            catch (JsonException)
            {
                return new UndoQuotaDto();
            }
        }

        private int Today()
        {
            DateTime today = _clock.Today;
            return today.Year * DayYearFactor + today.Month * DayMonthFactor + today.Day;
        }
    }
}
