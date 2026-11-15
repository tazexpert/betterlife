using System;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Time.Ticking.Phases
{
    /// <summary>
    /// Weekly drift of stored stats toward their resting values (traits apply, the target is never overshot),
    /// skill decay, and expiry of temporary flags.
    /// </summary>
    public sealed class NaturalDecayPhase : ITickPhase
    {
        /// <summary>Source key shown in breakdowns for natural drift.</summary>
        public const string SourceKey = "source.time.natural_drift";

        private const float MinimumStep = 0.0001f;

        /// <inheritdoc />
        public string Name => "natural_decay";

        /// <inheritdoc />
        public int Order => TickOrder.NaturalDecay;

        /// <inheritdoc />
        public TickCadence Cadence => TickCadence.Weekly;

        /// <inheritdoc />
        public void Execute(TickContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Character character = context.Character;
            StatCatalog catalog = character.Catalogs.Stats;
            foreach (StatDefinition definition in catalog.Definitions)
            {
                if (definition.Decay == null || catalog.IsDerived(definition.Id))
                {
                    continue;
                }

                float request = DriftRequest(character, definition.Id, definition.Decay);
                if (Math.Abs(request) > MinimumStep)
                {
                    character.ApplyStatChange(new StatChange(definition.Id, request, SourceKey), context.Week);
                }
            }

            character.Skills.ApplyWeeklyDecay(context.Week);
            character.Flags.RemoveExpired(context.Week);
        }

        // Returns the delta to request so that, after trait multipliers, the stat moves toward the target
        // by at most the weekly rate without passing it.
        private static float DriftRequest(Character character, StatId statId, StatDecayRule rule)
        {
            float gap = rule.Target - character.Stats.GetValue(statId);
            float step = Math.Max(-rule.RatePerWeek, Math.Min(rule.RatePerWeek, gap));
            if (step == 0f)
            {
                return 0f;
            }

            float multiplier = character.Traits.GetMultiplier(statId, step);
            return Math.Abs(step * multiplier) > Math.Abs(gap) ? gap / multiplier : step;
        }
    }
}
