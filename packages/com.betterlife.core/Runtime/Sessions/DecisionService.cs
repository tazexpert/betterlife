using System;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Saving;

namespace BetterLife.Core.Sessions
{
    /// <summary>
    /// Applies the player's decisions. Every decision first takes an undo checkpoint, so the latest one can be taken back.
    /// </summary>
    public sealed class DecisionService
    {
        private readonly UndoService _undo;
        private readonly int _contentVersion;

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="undo">Undo snapshots; null disables checkpoints.</param>
        /// <param name="contentVersion">Version of the content in use.</param>
        public DecisionService(UndoService undo, int contentVersion)
        {
            _undo = undo;
            _contentVersion = contentVersion;
        }

        /// <summary>
        /// Answers a pending event.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="instanceId">Pending event instance id.</param>
        /// <param name="choiceId">Chosen option (must be available).</param>
        /// <returns>The resolution.</returns>
        public EventResolution Answer(LifeSession session, int instanceId, string choiceId)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (session.IsOver)
            {
                throw new InvalidOperationException("The life has ended.");
            }
            _undo?.Checkpoint(session, _contentVersion);
            return session.Events.Resolve(session.Character, instanceId, choiceId);
        }
    }
}
