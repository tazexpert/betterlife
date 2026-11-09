using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// Global vice rules from content/vice_settings.json.
    /// </summary>
    public sealed class ViceSettings
    {
        /// <summary>
        /// Creates the settings.
        /// </summary>
        /// <param name="visibleFromAddiction">Addiction from which the habit card appears.</param>
        /// <param name="firstTryMultiplier">Temptation multiplier before the first use.</param>
        /// <param name="chaseFromAddiction">Addiction from which the character uses without money (going into debt).</param>
        /// <param name="maxWeeklyTemptation">Cap of the weekly temptation chance.</param>
        /// <param name="stressWeight">Temptation change per 50 stress points above 50.</param>
        /// <param name="stressMinMultiplier">Lowest stress multiplier.</param>
        /// <param name="addictionWeight">Temptation change at full addiction.</param>
        /// <param name="resist">Resist rules.</param>
        /// <param name="supportMinValue">Relationship value from which someone supports the character.</param>
        /// <param name="supportPerTie">Resist points per supporter.</param>
        /// <param name="supportMax">Cap of support points.</param>
        /// <param name="badCompany">Bad company rules.</param>
        /// <param name="laziness">Laziness rules.</param>
        /// <param name="traitMultipliers">Temptation multiplier per character trait id.</param>
        /// <param name="helplineKey">Localization key of the help page.</param>
        public ViceSettings(float visibleFromAddiction, float firstTryMultiplier, float chaseFromAddiction, float maxWeeklyTemptation, float stressWeight,
            float stressMinMultiplier, float addictionWeight, ResistRules resist, float supportMinValue, float supportPerTie, float supportMax,
            BadCompanyRules badCompany, LazinessRules laziness, IReadOnlyDictionary<string, float> traitMultipliers, string helplineKey)
        {
            if (!StatBounds.IsInRange(visibleFromAddiction) || firstTryMultiplier < 0f || !(maxWeeklyTemptation > 0f) || maxWeeklyTemptation > 1f
                || stressMinMultiplier < 0f || addictionWeight < 0f || supportPerTie < 0f || supportMax < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(visibleFromAddiction), "Vice settings are out of range.");
            }
            VisibleFromAddiction = visibleFromAddiction;
            FirstTryMultiplier = firstTryMultiplier;
            ChaseFromAddiction = chaseFromAddiction;
            MaxWeeklyTemptation = maxWeeklyTemptation;
            StressWeight = stressWeight;
            StressMinMultiplier = stressMinMultiplier;
            AddictionWeight = addictionWeight;
            Resist = resist ?? throw new ArgumentNullException(nameof(resist));
            SupportMinValue = supportMinValue;
            SupportPerTie = supportPerTie;
            SupportMax = supportMax;
            BadCompany = badCompany ?? throw new ArgumentNullException(nameof(badCompany));
            Laziness = laziness ?? throw new ArgumentNullException(nameof(laziness));
            TraitMultipliers = traitMultipliers ?? new Dictionary<string, float>();
            HelplineKey = ContentId.RequireLocKey(helplineKey, nameof(helplineKey));
        }

        /// <summary>Addiction from which the habit card appears.</summary>
        public float VisibleFromAddiction { get; }

        /// <summary>Temptation multiplier before the first use.</summary>
        public float FirstTryMultiplier { get; }

        /// <summary>Addiction from which the character uses without money.</summary>
        public float ChaseFromAddiction { get; }

        /// <summary>Cap of the weekly temptation chance.</summary>
        public float MaxWeeklyTemptation { get; }

        /// <summary>Temptation change per 50 stress points above 50.</summary>
        public float StressWeight { get; }

        /// <summary>Lowest stress multiplier.</summary>
        public float StressMinMultiplier { get; }

        /// <summary>Temptation change at full addiction.</summary>
        public float AddictionWeight { get; }

        /// <summary>Resist rules.</summary>
        public ResistRules Resist { get; }

        /// <summary>Relationship value from which someone supports the character.</summary>
        public float SupportMinValue { get; }

        /// <summary>Resist points per supporter.</summary>
        public float SupportPerTie { get; }

        /// <summary>Cap of support points.</summary>
        public float SupportMax { get; }

        /// <summary>Bad company rules.</summary>
        public BadCompanyRules BadCompany { get; }

        /// <summary>Laziness rules.</summary>
        public LazinessRules Laziness { get; }

        /// <summary>Temptation multiplier per character trait id.</summary>
        public IReadOnlyDictionary<string, float> TraitMultipliers { get; }

        /// <summary>Localization key of the help page.</summary>
        public string HelplineKey { get; }
    }
}
