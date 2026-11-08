using System;
using System.Collections.Generic;

namespace BetterLife.Core.Events
{
    /// <summary>
    /// Default <see cref="IEventBus"/>. Dispatches on the event's runtime type (so an event held as
    /// <see cref="IGameEvent"/> still reaches its handlers), queues nested publishes, and keeps delivering when a
    /// handler throws, reporting all failures together afterwards. Not thread-safe.
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Action<IGameEvent>>> _handlers = new Dictionary<Type, List<Action<IGameEvent>>>();
        private readonly Queue<IGameEvent> _pending = new Queue<IGameEvent>();
        private bool _isDispatching;

        /// <inheritdoc />
        public IDisposable Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (!_handlers.TryGetValue(typeof(T), out List<Action<IGameEvent>> list))
            {
                list = new List<Action<IGameEvent>>();
                _handlers.Add(typeof(T), list);
            }
            Action<IGameEvent> wrapper = e => handler((T)e);
            list.Add(wrapper);
            return new Subscription(() => list.Remove(wrapper));
        }

        /// <inheritdoc />
        public void Publish<T>(T gameEvent) where T : IGameEvent
        {
            if (gameEvent == null)
            {
                throw new ArgumentNullException(nameof(gameEvent));
            }

            _pending.Enqueue(gameEvent);
            if (_isDispatching)
            {
                return;
            }

            var errors = new List<Exception>();
            _isDispatching = true;
            try
            {
                while (_pending.Count > 0)
                {
                    Dispatch(_pending.Dequeue(), errors);
                }
            }
            finally
            {
                _isDispatching = false;
            }

            if (errors.Count > 0)
            {
                throw new AggregateException("One or more event handlers threw.", errors);
            }
        }

        private void Dispatch(IGameEvent gameEvent, List<Exception> errors)
        {
            if (!_handlers.TryGetValue(gameEvent.GetType(), out List<Action<IGameEvent>> list))
            {
                return;
            }

            // Snapshot so handlers may subscribe or unsubscribe during dispatch.
            foreach (Action<IGameEvent> handler in list.ToArray())
            {
                try
                {
                    handler(gameEvent);
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }
        }

        private sealed class Subscription : IDisposable
        {
            private Action _unsubscribe;

            public Subscription(Action unsubscribe)
            {
                _unsubscribe = unsubscribe;
            }

            public void Dispose()
            {
                _unsubscribe?.Invoke();
                _unsubscribe = null;
            }
        }
    }
}
