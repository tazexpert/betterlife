using System;
using System.Collections.Generic;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Track ids with their category and level ids, collected before tracks are parsed so rules can refer to them.
    /// </summary>
    public sealed class CareerTrackIndex
    {
        private readonly Dictionary<string, (CareerCategory? Category, List<string> Levels)> _tracks =
            new Dictionary<string, (CareerCategory? Category, List<string> Levels)>(StringComparer.Ordinal);

        /// <summary>
        /// Removes every track.
        /// </summary>
        public void Clear() => _tracks.Clear();

        /// <summary>
        /// Adds or replaces a track.
        /// </summary>
        /// <param name="trackId">Track id.</param>
        /// <param name="category">Category, or null when unknown.</param>
        /// <param name="levelIds">Level ids from the entry level up.</param>
        public void Add(string trackId, CareerCategory? category, IEnumerable<string> levelIds)
        {
            _tracks[trackId ?? throw new ArgumentNullException(nameof(trackId))] = (category, new List<string>(levelIds ?? new string[0]));
        }

        /// <summary>
        /// Returns true for a known track.
        /// </summary>
        /// <param name="trackId">Track id.</param>
        /// <returns>True when known.</returns>
        public bool Contains(string trackId) => trackId != null && _tracks.ContainsKey(trackId);

        /// <summary>
        /// Returns the index of a level in a track, or -1.
        /// </summary>
        /// <param name="trackId">Track id.</param>
        /// <param name="levelId">Level id.</param>
        /// <returns>Index.</returns>
        public int LevelIndex(string trackId, string levelId) => Contains(trackId) ? _tracks[trackId].Levels.IndexOf(levelId) : -1;

        /// <summary>
        /// Returns the level id at an index, or null.
        /// </summary>
        /// <param name="trackId">Track id.</param>
        /// <param name="index">Level index.</param>
        /// <returns>Level id.</returns>
        public string LevelIdAt(string trackId, int index)
        {
            return Contains(trackId) && index >= 0 && index < _tracks[trackId].Levels.Count ? _tracks[trackId].Levels[index] : null;
        }

        /// <summary>
        /// Returns the category of a track, or null.
        /// </summary>
        /// <param name="trackId">Track id.</param>
        /// <returns>Category.</returns>
        public CareerCategory? CategoryOf(string trackId) => Contains(trackId) ? _tracks[trackId].Category : null;
    }
}
