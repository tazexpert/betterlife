using System;
using System.Collections.Generic;

namespace BetterLife.Core.Scams
{
    /// <summary>
    /// Global scam rules from content/scam_settings.json.
    /// </summary>
    public sealed class ScamSettings
    {
        /// <summary>
        /// Creates the value.
        /// </summary>
        /// <param name="onlineActivityIds">Activities counted as time online.</param>
        /// <param name="onlineBaselineHours">Weekly online hours that do not count.</param>
        /// <param name="perOnlineHour">Targeting multiplier added per online hour above the baseline.</param>
        /// <param name="maxOnline">Largest online multiplier.</param>
        /// <param name="wealthReference">Money at which the wealth multiplier is neutral.</param>
        /// <param name="stressPenaltyPerPoint">Detection chance points per stress point above 50 (negative).</param>
        /// <param name="supportMinValue">Relationship value from which someone warns the character.</param>
        /// <param name="supportPerTie">Detection points per such person.</param>
        /// <param name="supportMax">Cap of support points.</param>
        /// <param name="lessonGain">Street smarts gained by being scammed.</param>
        /// <param name="detectedGain">Street smarts gained by spotting a scam.</param>
        /// <param name="maxIncidents">Incidents kept (oldest dropped).</param>
        /// <param name="maxMonthlyChance">Cap of the monthly targeting chance.</param>
        /// <param name="helplineKey">Localization key of the help page.</param>
        /// <param name="majorLoss">Loss from which being scammed stops time (smaller losses only notify).</param>
        public ScamSettings(IReadOnlyCollection<string> onlineActivityIds, int onlineBaselineHours, float perOnlineHour, float maxOnline, decimal wealthReference, float stressPenaltyPerPoint, float supportMinValue, float supportPerTie, float supportMax, float lessonGain, float detectedGain, int maxIncidents, float maxMonthlyChance, string helplineKey, decimal majorLoss)
        {
            if (perOnlineHour < 0f || maxOnline < 1f || wealthReference <= 0m || supportPerTie < 0f || supportMax < 0f || lessonGain < 0f || detectedGain < 0f || maxIncidents < 1 || !(maxMonthlyChance > 0f) || maxMonthlyChance > 1f || majorLoss < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(onlineActivityIds), "Values are out of range.");
            }
            OnlineActivityIds = onlineActivityIds ?? throw new ArgumentNullException(nameof(onlineActivityIds));
            OnlineBaselineHours = onlineBaselineHours;
            PerOnlineHour = perOnlineHour;
            MaxOnline = maxOnline;
            WealthReference = wealthReference;
            StressPenaltyPerPoint = stressPenaltyPerPoint;
            SupportMinValue = supportMinValue;
            SupportPerTie = supportPerTie;
            SupportMax = supportMax;
            LessonGain = lessonGain;
            DetectedGain = detectedGain;
            MaxIncidents = maxIncidents;
            MaxMonthlyChance = maxMonthlyChance;
            HelplineKey = helplineKey ?? throw new ArgumentNullException(nameof(helplineKey));
            MajorLoss = majorLoss;
        }

        /// <summary>Activities counted as time online.</summary>
        public IReadOnlyCollection<string> OnlineActivityIds { get; }

        /// <summary>Weekly online hours that do not count.</summary>
        public int OnlineBaselineHours { get; }

        /// <summary>Targeting multiplier added per online hour above the baseline.</summary>
        public float PerOnlineHour { get; }

        /// <summary>Largest online multiplier.</summary>
        public float MaxOnline { get; }

        /// <summary>Money at which the wealth multiplier is neutral.</summary>
        public decimal WealthReference { get; }

        /// <summary>Detection chance points per stress point above 50 (negative).</summary>
        public float StressPenaltyPerPoint { get; }

        /// <summary>Relationship value from which someone warns the character.</summary>
        public float SupportMinValue { get; }

        /// <summary>Detection points per such person.</summary>
        public float SupportPerTie { get; }

        /// <summary>Cap of support points.</summary>
        public float SupportMax { get; }

        /// <summary>Street smarts gained by being scammed.</summary>
        public float LessonGain { get; }

        /// <summary>Street smarts gained by spotting a scam.</summary>
        public float DetectedGain { get; }

        /// <summary>Incidents kept (oldest dropped).</summary>
        public int MaxIncidents { get; }

        /// <summary>Cap of the monthly targeting chance.</summary>
        public float MaxMonthlyChance { get; }

        /// <summary>Localization key of the help page.</summary>
        public string HelplineKey { get; }

        /// <summary>Loss from which being scammed stops time.</summary>
        public decimal MajorLoss { get; }
    }
}
