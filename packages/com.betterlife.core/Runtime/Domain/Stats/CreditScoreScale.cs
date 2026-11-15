using System;

namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// Converts the internal 0-100 credit score stat to the 300-900 range shown to the player
    /// (modelled on the Thai credit bureau scale).
    /// </summary>
    public static class CreditScoreScale
    {
        /// <summary>Lowest displayed score.</summary>
        public const int DisplayMin = 300;

        /// <summary>Highest displayed score.</summary>
        public const int DisplayMax = 900;

        /// <summary>
        /// Converts an internal value to the displayed score.
        /// </summary>
        /// <param name="statValue">Internal value, clamped to 0-100.</param>
        /// <returns>Displayed score from 300 to 900.</returns>
        public static int ToDisplay(float statValue)
        {
            float t = StatBounds.Clamp(statValue) / StatBounds.Max;
            return (int)Math.Round(DisplayMin + t * (DisplayMax - DisplayMin), MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Converts a displayed score back to the internal value.
        /// </summary>
        /// <param name="displayScore">Displayed score, clamped to 300-900.</param>
        /// <returns>Internal value from 0 to 100.</returns>
        public static float FromDisplay(int displayScore)
        {
            int clamped = Math.Max(DisplayMin, Math.Min(DisplayMax, displayScore));
            return (clamped - DisplayMin) * StatBounds.Max / (DisplayMax - DisplayMin);
        }
    }
}
