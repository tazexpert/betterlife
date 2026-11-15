using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Time.Ticking
{
    /// <summary>
    /// Decides which interrupt requests stop time skipping. The threshold is a player setting.
    /// </summary>
    public sealed class InterruptPolicy
    {
        /// <summary>Default threshold from the design: Major and Critical stop the skip.</summary>
        public const Severity DefaultThreshold = Severity.Major;

        /// <summary>
        /// Creates a policy.
        /// </summary>
        /// <param name="threshold">Lowest severity that stops time skipping.</param>
        public InterruptPolicy(Severity threshold = DefaultThreshold)
        {
            Threshold = threshold;
        }

        /// <summary>Lowest severity that stops time skipping.</summary>
        public Severity Threshold { get; set; }

        /// <summary>
        /// Whether a request that needs the player's answer stops time at any severity (true in the game; mass
        /// simulations turn it off so unanswered events simply expire).
        /// </summary>
        public bool StopForDecisions { get; set; } = true;

        /// <summary>
        /// Returns the most severe request at or above the threshold, or null when skipping may continue.
        /// </summary>
        /// <param name="requests">Requests made this week.</param>
        /// <returns>The stopping request, or null.</returns>
        public InterruptRequest FindStop(IEnumerable<InterruptRequest> requests)
        {
            if (requests == null)
            {
                throw new ArgumentNullException(nameof(requests));
            }
            return requests.Where(r => r.Severity >= Threshold || (StopForDecisions && r.RequiresAnswer))
                .OrderByDescending(r => r.RequiresAnswer && StopForDecisions)
                .ThenByDescending(r => r.Severity)
                .FirstOrDefault();
        }
    }
}
