using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Time;

namespace BetterLife.Core.Scams
{
    /// <summary>
    /// One time the character was scammed.
    /// </summary>
    public sealed class ScamIncident
    {
        /// <summary>
        /// Creates or restores the incident.
        /// </summary>
        /// <param name="id">Id; positive.</param>
        /// <param name="scamId">Scam id.</param>
        /// <param name="week">Week it happened.</param>
        /// <param name="lost">Money lost.</param>
        /// <param name="recovered">Money recovered.</param>
        /// <param name="reported">True once reported.</param>
        public ScamIncident(int id, string scamId, GameDate week, decimal lost, decimal recovered = 0m, bool reported = false)
        {
            if (id <= 0 || lost < 0m || recovered < 0m || recovered > lost)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Id, loss or recovery out of range.");
            }
            Id = id;
            ScamId = ContentId.Require(scamId, nameof(scamId));
            Week = week;
            Lost = lost;
            Recovered = recovered;
            Reported = reported;
        }

        /// <summary>Id.</summary>
        public int Id { get; }

        /// <summary>Scam id.</summary>
        public string ScamId { get; }

        /// <summary>Week it happened.</summary>
        public GameDate Week { get; }

        /// <summary>Money lost.</summary>
        public decimal Lost { get; }

        /// <summary>Money recovered.</summary>
        public decimal Recovered { get; private set; }

        /// <summary>True once reported.</summary>
        public bool Reported { get; private set; }

        internal void MarkReported(decimal recovered)
        {
            Reported = true;
            Recovered = Math.Min(Lost, Recovered + recovered);
        }
    }
}
