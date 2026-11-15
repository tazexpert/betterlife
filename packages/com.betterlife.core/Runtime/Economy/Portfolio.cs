using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Time;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// The character's assets.
    /// </summary>
    public sealed class Portfolio
    {
        private readonly List<Holding> _holdings = new List<Holding>();

        /// <summary>Holdings in purchase order.</summary>
        public IReadOnlyList<Holding> Holdings => _holdings;

        /// <summary>Id the next holding will get.</summary>
        public int NextId { get; private set; } = 1;

        /// <summary>Total value.</summary>
        public decimal TotalValue => _holdings.Sum(h => h.Value);

        /// <summary>
        /// Returns a holding by id, or null.
        /// </summary>
        /// <param name="id">Holding id.</param>
        /// <returns>The holding or null.</returns>
        public Holding Find(int id) => _holdings.FirstOrDefault(h => h.Id == id);

        /// <summary>
        /// Adds a new position.
        /// </summary>
        /// <param name="assetId">Asset id.</param>
        /// <param name="value">Value; positive.</param>
        /// <param name="week">Week of purchase.</param>
        /// <returns>The new holding.</returns>
        public Holding Add(string assetId, decimal value, GameDate week)
        {
            if (value <= 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be positive.");
            }
            var holding = new Holding(NextId++, assetId, Money.Round(value), Money.Round(value), week);
            _holdings.Add(holding);
            return holding;
        }

        /// <summary>
        /// Removes a holding.
        /// </summary>
        /// <param name="id">Holding id.</param>
        /// <returns>True when it existed.</returns>
        public bool Remove(int id) => _holdings.RemoveAll(h => h.Id == id) > 0;

        /// <summary>
        /// Restores saved holdings.
        /// </summary>
        /// <param name="holdings">Holdings.</param>
        /// <param name="nextId">Next id; raised above every saved id.</param>
        public void Restore(IEnumerable<Holding> holdings, int nextId)
        {
            _holdings.Clear();
            _holdings.AddRange(holdings ?? Enumerable.Empty<Holding>());
            NextId = Math.Max(Math.Max(1, nextId), _holdings.Select(h => h.Id).DefaultIfEmpty(0).Max() + 1);
        }
    }
}
