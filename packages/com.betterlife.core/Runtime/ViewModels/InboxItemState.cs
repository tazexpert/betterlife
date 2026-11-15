using BetterLife.Core.Domain;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// One inbox row.
    /// </summary>
    public sealed class InboxItemState
    {
        /// <summary>
        /// Creates a row.
        /// </summary>
        /// <param name="kind">Decision or notice.</param>
        /// <param name="instanceId">Pending event instance id (decisions only).</param>
        /// <param name="title">Title.</param>
        /// <param name="subtitle">Deadline (decisions) or date (notices).</param>
        /// <param name="severity">Importance.</param>
        /// <param name="isUrgent">Whether the answer is due this week.</param>
        public InboxItemState(InboxItemKind kind, int? instanceId, string title, string subtitle, Severity severity, bool isUrgent)
        {
            Kind = kind;
            InstanceId = instanceId;
            Title = title;
            Subtitle = subtitle;
            Severity = severity;
            IsUrgent = isUrgent;
        }

        /// <summary>Decision or notice.</summary>
        public InboxItemKind Kind { get; }

        /// <summary>Pending event instance id (decisions only).</summary>
        public int? InstanceId { get; }

        /// <summary>Title.</summary>
        public string Title { get; }

        /// <summary>Deadline or date.</summary>
        public string Subtitle { get; }

        /// <summary>Importance.</summary>
        public Severity Severity { get; }

        /// <summary>Whether the answer is due this week.</summary>
        public bool IsUrgent { get; }
    }
}
