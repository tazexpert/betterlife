using System;
using BetterLife.Core.Time;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// A child on the way with a partner.
    /// </summary>
    public sealed class Pregnancy
    {
        /// <summary>
        /// Creates the pregnancy.
        /// </summary>
        /// <param name="partnerNpcId">The other parent.</param>
        /// <param name="dueOn">Birth week.</param>
        public Pregnancy(int partnerNpcId, GameDate dueOn)
        {
            if (partnerNpcId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(partnerNpcId), partnerNpcId, "Id must be positive.");
            }
            PartnerNpcId = partnerNpcId;
            DueOn = dueOn;
        }

        /// <summary>The other parent.</summary>
        public int PartnerNpcId { get; }

        /// <summary>Birth week.</summary>
        public GameDate DueOn { get; }
    }
}
