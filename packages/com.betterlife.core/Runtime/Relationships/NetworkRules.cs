using System;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// How close work and business contacts help job applications.
    /// </summary>
    public sealed class NetworkRules
    {
        /// <summary>
        /// Creates the rules.
        /// </summary>
        /// <param name="minValue">Value from which a contact helps.</param>
        /// <param name="pointsPerContact">Hiring chance points per contact.</param>
        /// <param name="maxPoints">Cap of the bonus.</param>
        public NetworkRules(float minValue, float pointsPerContact, float maxPoints)
        {
            if (pointsPerContact < 0f || maxPoints < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(minValue), "Values are out of range.");
            }
            MinValue = minValue;
            PointsPerContact = pointsPerContact;
            MaxPoints = maxPoints;
        }

        /// <summary>Value from which a contact helps.</summary>
        public float MinValue { get; }

        /// <summary>Hiring chance points per contact.</summary>
        public float PointsPerContact { get; }

        /// <summary>Cap of the bonus.</summary>
        public float MaxPoints { get; }
    }
}
