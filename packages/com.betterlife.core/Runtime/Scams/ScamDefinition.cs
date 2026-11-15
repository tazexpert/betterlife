using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Luck;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Scams
{
    /// <summary>
    /// A kind of scam: who it targets, how hard it is to spot, what it takes and how the money may come back.
    /// </summary>
    public sealed class ScamDefinition : IDefinition
    {
        /// <summary>
        /// Creates the scam.
        /// </summary>
        /// <param name="id">Unique id.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="channel">How it reaches the character.</param>
        /// <param name="minAge">Youngest target age.</param>
        /// <param name="monthlyChance">Base monthly chance of being targeted, 0-1.</param>
        /// <param name="ageBands">Multipliers from an age upward (the highest matching band applies).</param>
        /// <param name="lonelinessWeight">Multiplier gained at full loneliness.</param>
        /// <param name="wealthWeight">Multiplier gained per tenfold money above the reference.</param>
        /// <param name="modifiers">Multipliers from conditions.</param>
        /// <param name="detection">Chance to spot the scam.</param>
        /// <param name="loss">What it takes.</param>
        /// <param name="effects">Effects of being scammed.</param>
        /// <param name="recovery">Getting money back.</param>
        public ScamDefinition(string id, string nameKey, ScamChannel channel, int minAge, float monthlyChance, IEnumerable<(int MinAge, float Multiplier)> ageBands,
            float lonelinessWeight, float wealthWeight, IEnumerable<ScamModifier> modifiers, ChanceSpec detection, ScamLoss loss, EffectSequence effects,
            ScamRecovery recovery)
        {
            if (minAge < 0 || !(monthlyChance >= 0f) || monthlyChance > 1f || lonelinessWeight < 0f || wealthWeight < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(minAge), "Scam values are out of range.");
            }
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            Channel = channel;
            MinAge = minAge;
            MonthlyChance = monthlyChance;
            AgeBands = (ageBands ?? Enumerable.Empty<(int, float)>()).OrderBy(b => b.MinAge).ToList();
            LonelinessWeight = lonelinessWeight;
            WealthWeight = wealthWeight;
            Modifiers = (modifiers ?? Enumerable.Empty<ScamModifier>()).ToList();
            Detection = detection ?? throw new ArgumentNullException(nameof(detection));
            Loss = loss ?? throw new ArgumentNullException(nameof(loss));
            Effects = effects ?? throw new ArgumentNullException(nameof(effects));
            Recovery = recovery ?? throw new ArgumentNullException(nameof(recovery));
        }

        /// <summary>Unique id.</summary>
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>How it reaches the character.</summary>
        public ScamChannel Channel { get; }

        /// <summary>Youngest target age.</summary>
        public int MinAge { get; }

        /// <summary>Base monthly chance of being targeted.</summary>
        public float MonthlyChance { get; }

        /// <summary>Age bands, youngest first.</summary>
        public IReadOnlyList<(int MinAge, float Multiplier)> AgeBands { get; }

        /// <summary>Multiplier gained at full loneliness.</summary>
        public float LonelinessWeight { get; }

        /// <summary>Multiplier gained per tenfold money above the reference.</summary>
        public float WealthWeight { get; }

        /// <summary>Multipliers from conditions.</summary>
        public IReadOnlyList<ScamModifier> Modifiers { get; }

        /// <summary>Chance to spot the scam.</summary>
        public ChanceSpec Detection { get; }

        /// <summary>What it takes.</summary>
        public ScamLoss Loss { get; }

        /// <summary>Effects of being scammed.</summary>
        public EffectSequence Effects { get; }

        /// <summary>Getting money back.</summary>
        public ScamRecovery Recovery { get; }

        /// <summary>
        /// Returns the age multiplier (1 below every band).
        /// </summary>
        /// <param name="age">Age in years.</param>
        /// <returns>Multiplier.</returns>
        public float AgeMultiplier(int age) => AgeBands.Where(b => age >= b.MinAge).Select(b => b.Multiplier).DefaultIfEmpty(1f).Last();
    }
}
