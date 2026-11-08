using System;
using System.Collections.Generic;

namespace BetterLife.Core.Events
{
    /// <summary>
    /// Default <see cref="IEventBus"/>. Dispatches on exact event type, queues nested publishes, and keeps
    /// delivering when a handler throws, reporting all failures together afterwards. Not thread-safe.
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();
        private readonly Queue<Action<List<Exception>>> _pending = new Queue<Action<List<Exception>>>();
        private bool _isDispatching;

        /// <inheritdoc />
        public IDisposable Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (!_handlers.TryGetValue(typeof(T), out List<Delegate> list))
            {
                list = new List<Delegate>();
                _handlers.Add(typeof(T), list);
            }
            list.Add(handler);
            return new Subscription(() => list.Remove(handler));
        }

        /// <inheritdoc />
        public void Publish<T>(T gameEvent) where T : IGameEvent
        {
            if (gameEvent == null)
            {
                throw new ArgumentNullException(nameof(gameEvent));
            }

            _pending.Enqueue(errors => Dispatch(gameEvent, errors));
            if (_isDispatching)
            {
                return;
            }

            var errorsCollected = new List<Exception>();
            _isDispatching = true;
            try
            {
                while (_pending.Count > 0)
                {
                    _pending.Dequeue()(errorsCollected);
                }
            }
            finally
            {
                _isDispatching = false;
            }

            if (errorsCollected.Count > 0)
            {
                throw new AggregateException("One or more event handlers threw.", errorsCollected);
            }
        }

        private void Dispatch<T>(T gameEvent, List<Exception> errors) where T : IGameEvent
        {
            if (!_handlers.TryGetValue(typeof(T), out List<Delegate> list))
            {
                return;
            }

            // Snapshot so handlers may subscribe or unsubscribe during dispatch.
            foreach (Delegate handler in list.ToArray())
            {
                try
                {
                    ((Action<T>)handler)(gameEvent);
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
