using BetterLife.Core.Time;

namespace BetterLife.Core.Domain.Life
{
    /// <summary>
    /// When and how a character died.
    /// </summary>
    public sealed class DeathRecord
    {
        /// <summary>
        /// Creates a record.
        /// </summary>
        /// <param name="week">Week of death.</param>
        /// <param name="causeId">Death cause id.</param>
        /// <param name="ageYears">Age at death.</param>
        public DeathRecord(GameDate week, string causeId, int ageYears)
        {
            Week = week;
            CauseId = ContentId.Require(causeId, nameof(causeId));
            AgeYears = ageYears;
        }

        /// <summary>Week of death.</summary>
        public GameDate Week { get; }

        /// <summary>Death cause id.</summary>
        public string CauseId { get; }

        /// <summary>Age at death.</summary>
        public int AgeYears { get; }
    }
}
