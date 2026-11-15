using System.Collections.Generic;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// An event waiting for the player's answer.
    /// </summary>
    public sealed class EventDialogState
    {
        /// <summary>
        /// Creates the state.
        /// </summary>
        /// <param name="instanceId">Pending event instance id.</param>
        /// <param name="title">Title.</param>
        /// <param name="body">Story text.</param>
        /// <param name="deadlineText">Deadline text.</param>
        /// <param name="choices">Visible choices in order.</param>
        public EventDialogState(int instanceId, string title, string body, string deadlineText, IReadOnlyList<ChoiceState> choices)
        {
            InstanceId = instanceId;
            Title = title;
            Body = body;
            DeadlineText = deadlineText;
            Choices = choices;
        }

        /// <summary>Pending event instance id.</summary>
        public int InstanceId { get; }

        /// <summary>Title.</summary>
        public string Title { get; }

        /// <summary>Story text.</summary>
        public string Body { get; }

        /// <summary>Deadline text.</summary>
        public string DeadlineText { get; }

        /// <summary>Visible choices in order.</summary>
        public IReadOnlyList<ChoiceState> Choices { get; }
    }
}
