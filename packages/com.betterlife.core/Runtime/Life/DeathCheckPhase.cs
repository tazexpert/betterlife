using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Life
{
    /// <summary>
    /// Death check (order 600): rolls this week's death risk and, on death, picks a cause and stops time.
    /// </summary>
    public sealed class DeathCheckPhase : ITickPhase
    {
        /// <summary>Interrupt reason of a death.</summary>
        public const string DeathInterruptKey = "interrupt.death";

        private readonly MortalityModel _mortality;
        private readonly DeathService _deaths;
        private readonly IRandomProvider _random;

        /// <summary>
        /// Creates the phase.
        /// </summary>
        /// <param name="mortality">Mortality model.</param>
        /// <param name="deaths">Death service.</param>
        /// <param name="random">Random stream for death rolls and causes.</param>
        public DeathCheckPhase(MortalityModel mortality, DeathService deaths, IRandomProvider random)
        {
            _mortality = mortality ?? throw new ArgumentNullException(nameof(mortality));
            _deaths = deaths ?? throw new ArgumentNullException(nameof(deaths));
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <inheritdoc />
        public string Name => "death_check";

        /// <inheritdoc />
        public int Order => TickOrder.DeathCheck;

        /// <inheritdoc />
        public TickCadence Cadence => TickCadence.Weekly;

        /// <inheritdoc />
        public void Execute(TickContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (!context.Character.IsAlive)
            {
                return;
            }
            // Always roll, so the stream advances the same way every week.
            bool dies = _random.Chance(_mortality.WeeklyProbability(context.Character, context.Week));
            if (!dies)
            {
                return;
            }
            DeathCauseDefinition cause = _deaths.PickCause(context.Character, context.Week, _random);
            _deaths.Kill(context.Character, cause.Id, context.Week);
            context.RequestInterrupt(Severity.Critical, DeathInterruptKey, cause.Id);
        }
    }
}
