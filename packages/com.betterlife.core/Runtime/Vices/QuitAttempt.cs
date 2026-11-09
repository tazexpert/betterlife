using BetterLife.Core.Domain;
using BetterLife.Core.Time;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// An attempt to quit a vice with a method.
    /// </summary>
    public sealed class QuitAttempt
    {
        /// <summary>
        /// Creates the attempt.
        /// </summary>
        /// <param name="methodId">Quit method id.</param>
        /// <param name="startedOn">First week.</param>
        /// <param name="endsOn">Week of success when nothing is used before.</param>
        public QuitAttempt(string methodId, GameDate startedOn, GameDate endsOn)
        {
            MethodId = ContentId.Require(methodId, nameof(methodId));
            StartedOn = startedOn;
            EndsOn = endsOn;
        }

        /// <summary>Quit method id.</summary>
        public string MethodId { get; }

        /// <summary>First week.</summary>
        public GameDate StartedOn { get; }

        /// <summary>Week of success.</summary>
        public GameDate EndsOn { get; }
    }
}
