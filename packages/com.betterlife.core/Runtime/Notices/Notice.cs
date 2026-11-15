using BetterLife.Core.Domain;
using BetterLife.Core.Time;

namespace BetterLife.Core.Notices
{
    /// <summary>
    /// Something that happened while time passed (promotion, scam, graduation...), shown as news in the inbox.
    /// </summary>
    public sealed class Notice
    {
        /// <summary>
        /// Creates a notice.
        /// </summary>
        /// <param name="severity">How important it is.</param>
        /// <param name="titleKey">Localization key of the title (the interrupt reason).</param>
        /// <param name="sourceId">Id of what caused it (event, vice, scam, career track...).</param>
        /// <param name="week">When it happened.</param>
        public Notice(Severity severity, string titleKey, string sourceId, GameDate week)
        {
            Severity = severity;
            TitleKey = titleKey;
            SourceId = sourceId;
            Week = week;
        }

        /// <summary>How important it is.</summary>
        public Severity Severity { get; }

        /// <summary>Localization key of the title.</summary>
        public string TitleKey { get; }

        /// <summary>Id of what caused it.</summary>
        public string SourceId { get; }

        /// <summary>When it happened.</summary>
        public GameDate Week { get; }
    }
}
