using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Localization;
using BetterLife.Core.Notices;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Builds the inbox from pending events and the notice feed.
    /// </summary>
    public static class InboxViewModel
    {
        /// <summary>Deadline text key (placeholder <c>{weeks}</c>).</summary>
        public const string RespondKey = "inbox.respond";
        /// <summary>Deadline text when the answer is due this week.</summary>
        public const string DueNowKey = "inbox.due_now";
        /// <summary>Text of an empty inbox.</summary>
        public const string EmptyKey = "inbox.empty";

        /// <summary>
        /// Builds the inbox.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="notices">News feed (null shows none).</param>
        /// <param name="localizer">Localizer.</param>
        /// <returns>The state.</returns>
        public static InboxState Build(LifeSession session, NoticeFeed notices, ILocalizer localizer)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (localizer == null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }
            GameDate now = session.TimeState.Clock.Now;
            List<InboxItemState> decisions = session.Events.Inbox
                .OrderBy(p => p.Deadline)
                .ThenBy(p => p.InstanceId)
                .Select(p => Decision(session, p, now, localizer))
                .ToList();
            List<InboxItemState> news = (notices?.Items ?? new Notice[0])
                .Select(n => new InboxItemState(InboxItemKind.Notice, null, localizer.Get(n.TitleKey), GameDateText.MonthYear(n.Week, localizer), n.Severity, false))
                .ToList();
            return new InboxState(decisions, news, localizer.Get(EmptyKey));
        }

        /// <summary>
        /// Deadline text of a pending event.
        /// </summary>
        /// <param name="pending">Pending event.</param>
        /// <param name="now">Current week.</param>
        /// <param name="localizer">Localizer.</param>
        /// <returns>The text.</returns>
        public static string Deadline(PendingEvent pending, GameDate now, ILocalizer localizer)
        {
            int weeks = Math.Max(0, now.WeeksUntil(pending.Deadline));
            return weeks == 0
                ? localizer.Get(DueNowKey)
                : localizer.Format(RespondKey, ("weeks", weeks.ToString(CultureInfo.InvariantCulture)));
        }

        private static InboxItemState Decision(LifeSession session, PendingEvent pending, GameDate now, ILocalizer localizer)
        {
            bool known = session.Events.Events.TryGet(pending.EventId, out LifeEventDefinition definition);
            string title = known ? localizer.Get(definition.TitleKey) : pending.EventId;
            Domain.Severity severity = known ? definition.Severity : Domain.Severity.Normal;
            return new InboxItemState(InboxItemKind.Decision, pending.InstanceId, title, Deadline(pending, now, localizer), severity,
                now.WeeksUntil(pending.Deadline) <= 0);
        }
    }
}
