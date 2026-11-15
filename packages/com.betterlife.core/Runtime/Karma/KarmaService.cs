using System;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time;

namespace BetterLife.Core.Karma
{
    /// <summary>
    /// Karma rules: bad deeds schedule debts, merit pays them down partially, and karma shifts success chances.
    /// </summary>
    public sealed class KarmaService
    {
        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="settings">Karma settings.</param>
        public KarmaService(KarmaSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>Karma settings.</summary>
        public KarmaSettings Settings { get; }

        /// <summary>
        /// Records a bad deed: adds bad karma and schedules a debt for part of it.
        /// </summary>
        /// <param name="character">Character.</param>
        /// <param name="amount">Non-negative amount.</param>
        /// <param name="sourceKey">Localization key of the deed.</param>
        /// <param name="now">Current week.</param>
        /// <param name="random">Random stream for the due date.</param>
        /// <returns>The scheduled debt, or null when none.</returns>
        public KarmicDebt RecordBadDeed(Character character, float amount, string sourceKey, GameDate now, IRandomProvider random)
        {
            if (character == null)
            {
                throw new ArgumentNullException(nameof(character));
            }
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            character.Karma.AddBad(amount);
            float debtAmount = amount * Settings.DebtRatio;
            if (debtAmount <= 0f)
            {
                return null;
            }
            int delay = random.NextInt(Settings.DebtDelayMinWeeks, Settings.DebtDelayMaxWeeks + 1);
            var debt = new KarmicDebt(debtAmount, now.AddWeeks(delay), sourceKey);
            character.Karma.AddDebt(debt);
            return debt;
        }

        /// <summary>
        /// Makes merit: adds good karma and pays down pending debts by a share of the amount.
        /// Bad karma itself is never reduced.
        /// </summary>
        /// <param name="character">Character.</param>
        /// <param name="amount">Non-negative amount.</param>
        /// <returns>Debt actually paid down.</returns>
        public float MakeMerit(Character character, float amount)
        {
            if (character == null)
            {
                throw new ArgumentNullException(nameof(character));
            }
            character.Karma.AddGood(amount);
            return character.Karma.OffsetDebts(amount * Settings.MeritOffsetRatio);
        }

        /// <summary>
        /// Returns the chance points from karma: good × weight − bad × weight, clamped to ±cap.
        /// </summary>
        /// <param name="ledger">Karma ledger.</param>
        /// <returns>Chance points.</returns>
        public float ChancePoints(KarmaLedger ledger)
        {
            if (ledger == null)
            {
                throw new ArgumentNullException(nameof(ledger));
            }
            float raw = ledger.Good * Settings.GoodWeight - ledger.Bad * Settings.BadWeight;
            return Math.Max(-Settings.Cap, Math.Min(Settings.Cap, raw));
        }
    }
}
