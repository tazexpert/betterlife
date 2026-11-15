using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Time;

namespace BetterLife.Core.Karma
{
    /// <summary>
    /// Good and bad karma kept separately (they never cancel out, 0-100 each) plus pending karmic debts.
    /// </summary>
    public sealed class KarmaLedger
    {
        private readonly List<KarmicDebt> _debts = new List<KarmicDebt>();

        /// <summary>Accumulated good karma.</summary>
        public float Good { get; private set; }

        /// <summary>Accumulated bad karma.</summary>
        public float Bad { get; private set; }

        /// <summary>Pending debts, earliest due first.</summary>
        public IReadOnlyList<KarmicDebt> Debts => _debts;

        /// <summary>Total of pending debts.</summary>
        public float PendingDebt => _debts.Sum(d => d.Amount);

        /// <summary>
        /// Adds good karma (clamped at the maximum).
        /// </summary>
        /// <param name="amount">Non-negative amount.</param>
        public void AddGood(float amount) => Good = StatBounds.Clamp(Good + RequireNonNegative(amount));

        /// <summary>
        /// Adds bad karma (clamped at the maximum).
        /// </summary>
        /// <param name="amount">Non-negative amount.</param>
        public void AddBad(float amount) => Bad = StatBounds.Clamp(Bad + RequireNonNegative(amount));

        /// <summary>
        /// Schedules a debt.
        /// </summary>
        /// <param name="debt">The debt.</param>
        public void AddDebt(KarmicDebt debt)
        {
            if (debt == null)
            {
                throw new ArgumentNullException(nameof(debt));
            }
            int index = _debts.FindIndex(d => d.DueOn > debt.DueOn);
            _debts.Insert(index < 0 ? _debts.Count : index, debt);
        }

        /// <summary>
        /// Pays down pending debts, earliest due first; fully paid debts disappear.
        /// </summary>
        /// <param name="amount">Non-negative offset.</param>
        /// <returns>The amount actually used.</returns>
        public float OffsetDebts(float amount)
        {
            float remaining = RequireNonNegative(amount);
            foreach (KarmicDebt debt in _debts)
            {
                if (remaining <= 0f)
                {
                    break;
                }
                remaining -= debt.Reduce(remaining);
            }
            _debts.RemoveAll(d => d.Amount <= 0f);
            return amount - remaining;
        }

        /// <summary>
        /// Removes and returns debts due on or before <paramref name="now"/>.
        /// </summary>
        /// <param name="now">Current week.</param>
        /// <returns>Due debts.</returns>
        public IReadOnlyList<KarmicDebt> TakeDue(GameDate now)
        {
            List<KarmicDebt> due = _debts.Where(d => d.DueOn <= now).ToList();
            _debts.RemoveAll(d => d.DueOn <= now);
            return due;
        }

        /// <summary>
        /// Restores saved values.
        /// </summary>
        /// <param name="good">Good karma.</param>
        /// <param name="bad">Bad karma.</param>
        /// <param name="debts">Pending debts.</param>
        public void Restore(float good, float bad, IEnumerable<KarmicDebt> debts)
        {
            Good = StatBounds.Clamp(RequireNonNegative(good));
            Bad = StatBounds.Clamp(RequireNonNegative(bad));
            _debts.Clear();
            foreach (KarmicDebt debt in debts ?? Enumerable.Empty<KarmicDebt>())
            {
                AddDebt(debt);
            }
        }

        private static float RequireNonNegative(float amount)
        {
            if (!(amount >= 0f) || float.IsInfinity(amount))
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be non-negative and finite.");
            }
            return amount;
        }
    }
}
