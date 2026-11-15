using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// The character's loans.
    /// </summary>
    public sealed class DebtBook
    {
        private readonly List<Debt> _debts = new List<Debt>();

        /// <summary>Loans in the order they were taken.</summary>
        public IReadOnlyList<Debt> Debts => _debts;

        /// <summary>Id the next loan will get.</summary>
        public int NextId { get; private set; } = 1;

        /// <summary>Total owed.</summary>
        public decimal TotalBalance => _debts.Sum(d => d.Balance);

        /// <summary>
        /// Returns a loan by id, or null.
        /// </summary>
        /// <param name="id">Loan id.</param>
        /// <returns>The loan or null.</returns>
        public Debt Find(int id) => _debts.FirstOrDefault(d => d.Id == id);

        /// <summary>
        /// Adds a loan with the next id.
        /// </summary>
        /// <param name="create">Builds the loan from its id.</param>
        /// <returns>The loan.</returns>
        public Debt Add(Func<int, Debt> create)
        {
            if (create == null)
            {
                throw new ArgumentNullException(nameof(create));
            }
            Debt debt = create(NextId++);
            _debts.Add(debt);
            return debt;
        }

        /// <summary>
        /// Removes a loan.
        /// </summary>
        /// <param name="id">Loan id.</param>
        /// <returns>True when it existed.</returns>
        public bool Remove(int id) => _debts.RemoveAll(d => d.Id == id) > 0;

        /// <summary>
        /// Restores saved loans.
        /// </summary>
        /// <param name="debts">Loans.</param>
        /// <param name="nextId">Next id; raised above every saved id.</param>
        public void Restore(IEnumerable<Debt> debts, int nextId)
        {
            _debts.Clear();
            _debts.AddRange(debts ?? Enumerable.Empty<Debt>());
            NextId = Math.Max(Math.Max(1, nextId), _debts.Select(d => d.Id).DefaultIfEmpty(0).Max() + 1);
        }
    }
}
