using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// A bet was settled.
    /// </summary>
    public sealed class GambleResultEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="viceId">Vice id.</param>
        /// <param name="stake">Money staked.</param>
        /// <param name="won">True when won.</param>
        /// <param name="payout">Gross payout (0 when lost).</param>
        /// <param name="week">Week.</param>
        public GambleResultEvent(string viceId, decimal stake, bool won, decimal payout, GameDate week)
        {
            ViceId = viceId;
            Stake = stake;
            Won = won;
            Payout = payout;
            Week = week;
        }

        /// <summary>Vice id.</summary>
        public string ViceId { get; }

        /// <summary>Money staked.</summary>
        public decimal Stake { get; }

        /// <summary>True when won.</summary>
        public bool Won { get; }

        /// <summary>Gross payout (0 when lost).</summary>
        public decimal Payout { get; }

        /// <summary>Week.</summary>
        public GameDate Week { get; }
    }
}
