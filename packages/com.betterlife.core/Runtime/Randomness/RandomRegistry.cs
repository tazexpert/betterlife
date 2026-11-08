using System;
using System.Collections.Generic;

namespace BetterLife.Core.Randomness
{
    /// <summary>
    /// Owns every named random stream of one life. Each system receives its own stream so that adding
    /// random calls to one system never changes the results of another (stable Challenge Seeds).
    /// </summary>
    public sealed class RandomRegistry
    {
        private readonly Dictionary<string, IRandomProvider> _streams = new Dictionary<string, IRandomProvider>(StringComparer.Ordinal);

        /// <summary>
        /// Creates a registry for one life.
        /// </summary>
        /// <param name="lifeSeed">The seed of this life.</param>
        public RandomRegistry(ulong lifeSeed)
        {
            LifeSeed = lifeSeed;
        }

        /// <summary>The seed of this life.</summary>
        public ulong LifeSeed { get; }

        /// <summary>
        /// Returns the stream with the given name, creating it on first use.
        /// </summary>
        /// <param name="streamName">Stable, non-empty stream name, for example "events".</param>
        /// <returns>The named stream.</returns>
        public IRandomProvider GetStream(string streamName)
        {
            if (string.IsNullOrEmpty(streamName))
            {
                throw new ArgumentException("Stream name must be non-empty.", nameof(streamName));
            }

            if (!_streams.TryGetValue(streamName, out IRandomProvider stream))
            {
                stream = Pcg32.CreateDerived(LifeSeed, streamName);
                _streams.Add(streamName, stream);
            }
            return stream;
        }

        /// <summary>
        /// Captures the state of every created stream.
        /// </summary>
        /// <returns>Stream name to state map.</returns>
        public IReadOnlyDictionary<string, RandomState> CaptureAll()
        {
            var result = new Dictionary<string, RandomState>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, IRandomProvider> pair in _streams)
            {
                result.Add(pair.Key, pair.Value.CaptureState());
            }
            return result;
        }

        /// <summary>
        /// Restores stream states, creating streams that do not exist yet. Existing instances are updated in place,
        /// so systems holding a reference keep working.
        /// </summary>
        /// <param name="states">Stream name to state map.</param>
        public void RestoreAll(IReadOnlyDictionary<string, RandomState> states)
        {
            if (states == null)
            {
                throw new ArgumentNullException(nameof(states));
            }

            foreach (KeyValuePair<string, RandomState> pair in states)
            {
                GetStream(pair.Key).RestoreState(pair.Value);
            }
        }
    }
}
