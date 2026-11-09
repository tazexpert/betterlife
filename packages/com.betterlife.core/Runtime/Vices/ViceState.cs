using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Time;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// The character's history with one vice.
    /// </summary>
    public sealed class ViceState
    {
        /// <summary>
        /// Creates or restores the state.
        /// </summary>
        /// <param name="viceId">Vice id.</param>
        /// <param name="addiction">Addiction 0-100.</param>
        /// <param name="tolerance">Tolerance 0-100.</param>
        /// <param name="uses">Uses so far.</param>
        /// <param name="lastUsedOn">Week of the last use, or null.</param>
        /// <param name="bets">Bets placed (gambling).</param>
        /// <param name="netWinnings">Money won minus money staked (gambling).</param>
        /// <param name="discovered">True once family found out.</param>
        /// <param name="relapses">Failed quit attempts.</param>
        /// <param name="quits">Successful quit attempts.</param>
        /// <param name="quitting">Current quit attempt, or null.</param>
        public ViceState(string viceId, float addiction = 0f, float tolerance = 0f, int uses = 0, GameDate? lastUsedOn = null, int bets = 0,
            decimal netWinnings = 0m, bool discovered = false, int relapses = 0, int quits = 0, QuitAttempt quitting = null)
        {
            if (uses < 0 || bets < 0 || relapses < 0 || quits < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(uses), "Counts must be non-negative.");
            }
            ViceId = ContentId.Require(viceId, nameof(viceId));
            Addiction = StatBounds.Clamp(addiction);
            Tolerance = StatBounds.Clamp(tolerance);
            Uses = uses;
            LastUsedOn = lastUsedOn;
            Bets = bets;
            NetWinnings = netWinnings;
            Discovered = discovered;
            Relapses = relapses;
            Quits = quits;
            Quitting = quitting;
        }

        /// <summary>Vice id.</summary>
        public string ViceId { get; }

        /// <summary>Addiction 0-100.</summary>
        public float Addiction { get; private set; }

        /// <summary>Tolerance 0-100.</summary>
        public float Tolerance { get; private set; }

        /// <summary>Uses so far.</summary>
        public int Uses { get; private set; }

        /// <summary>Week of the last use, or null.</summary>
        public GameDate? LastUsedOn { get; private set; }

        /// <summary>Bets placed.</summary>
        public int Bets { get; private set; }

        /// <summary>Money won minus money staked.</summary>
        public decimal NetWinnings { get; private set; }

        /// <summary>True once family found out.</summary>
        public bool Discovered { get; private set; }

        /// <summary>Failed quit attempts.</summary>
        public int Relapses { get; private set; }

        /// <summary>Successful quit attempts.</summary>
        public int Quits { get; private set; }

        /// <summary>Current quit attempt, or null.</summary>
        public QuitAttempt Quitting { get; private set; }

        /// <summary>True once used at least once.</summary>
        public bool EverUsed => Uses > 0;

        /// <summary>
        /// Returns whole weeks since the last use (0 when never used).
        /// </summary>
        /// <param name="now">Week.</param>
        /// <returns>Clean streak in weeks.</returns>
        public int CleanWeeks(GameDate now) => LastUsedOn.HasValue ? Math.Max(0, now.TotalWeeks - LastUsedOn.Value.TotalWeeks) : 0;

        /// <summary>
        /// Changes addiction, clamped to 0-100.
        /// </summary>
        /// <param name="delta">Change.</param>
        public void ChangeAddiction(float delta) => Addiction = StatBounds.Clamp(Addiction + delta);

        internal void RecordUse(GameDate week, float addictionGain, float toleranceGain)
        {
            Uses++;
            LastUsedOn = week;
            ChangeAddiction(addictionGain);
            Tolerance = StatBounds.Clamp(Tolerance + toleranceGain);
        }

        internal void RecordBet(decimal net)
        {
            Bets++;
            NetWinnings += net;
        }

        internal void Rest(float addictionLoss, float toleranceLoss)
        {
            ChangeAddiction(-addictionLoss);
            Tolerance = StatBounds.Clamp(Tolerance - toleranceLoss);
        }

        internal void MarkDiscovered() => Discovered = true;

        internal void StartQuit(QuitAttempt attempt) => Quitting = attempt;

        internal void Relapse()
        {
            Relapses++;
            Quitting = null;
        }

        internal void CompleteQuit()
        {
            Quits++;
            Quitting = null;
            Tolerance = 0f;
        }
    }
}
