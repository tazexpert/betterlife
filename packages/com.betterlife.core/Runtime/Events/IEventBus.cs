using System;

namespace BetterLife.Core.Events
{
    /// <summary>
    /// Synchronous, type-safe message bus used for all cross-system communication.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Registers a handler for events of exactly type <typeparamref name="T"/>.
        /// Handlers are called in subscription order.
        /// </summary>
        /// <typeparam name="T">Event type.</typeparam>
        /// <param name="handler">Handler to call.</param>
        /// <returns>A token; dispose it to unsubscribe.</returns>
        IDisposable Subscribe<T>(Action<T> handler) where T : IGameEvent;

        /// <summary>
        /// Publishes an event. If called while another event is being dispatched, the event is queued and
        /// delivered after the current one has reached every handler (FIFO).
        /// </summary>
        /// <typeparam name="T">Event type.</typeparam>
        /// <param name="gameEvent">Event to publish.</param>
        void Publish<T>(T gameEvent) where T : IGameEvent;
    }
}
