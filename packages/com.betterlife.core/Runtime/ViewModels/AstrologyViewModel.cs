using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Astrology;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Karma;
using BetterLife.Core.Localization;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Builds the astrology screen: birth chart, this year's fortune per area (with its parts) and karma.
    /// </summary>
    public static class AstrologyViewModel
    {
        /// <summary>Profile label keys.</summary>
        public const string ZodiacKey = "astrology.zodiac";
        /// <summary>Thai birth day label.</summary>
        public const string BirthDayKey = "astrology.birth_day";
        /// <summary>Year animal label.</summary>
        public const string AnimalKey = "astrology.year_animal";
        /// <summary>Element label.</summary>
        public const string ElementKey = "astrology.element";
        /// <summary>Fortune heading (placeholder <c>{year}</c>).</summary>
        public const string YearKey = "astrology.year";
        /// <summary>Fortune area key prefix (<c>fortune.area.finance</c>...).</summary>
        public const string AreaPrefix = "fortune.area.";
        /// <summary>Fortune part label keys.</summary>
        public const string BaseKey = "fortune.part.base";
        /// <summary>Zodiac part.</summary>
        public const string ZodiacPartKey = "fortune.part.zodiac";
        /// <summary>Birth day part.</summary>
        public const string BirthDayPartKey = "fortune.part.birth_day";
        /// <summary>Element part.</summary>
        public const string ElementPartKey = "fortune.part.element";
        /// <summary>Life cycle part.</summary>
        public const string CyclePartKey = "fortune.part.cycle";
        /// <summary>This year's luck part.</summary>
        public const string YearlyPartKey = "fortune.part.yearly";
        /// <summary>Merit label.</summary>
        public const string GoodKey = "karma.good";
        /// <summary>Sin label.</summary>
        public const string BadKey = "karma.bad";
        /// <summary>Chance points from karma label.</summary>
        public const string PointsKey = "karma.points";
        /// <summary>Pending karmic debt label.</summary>
        public const string DebtKey = "karma.debt";
        /// <summary>Format of signed points.</summary>
        public const string PointsFormat = "+0.#;-0.#;0";

        /// <summary>
        /// Builds the screen.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="localizer">Localizer.</param>
        /// <returns>The state.</returns>
        public static AstrologyState Build(LifeSession session, ILocalizer localizer)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (localizer == null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }
            Character character = session.Character;
            GameDate now = session.TimeState.Clock.Now;
            FortuneCalculator fortune = session.Luck.Fortune;
            AstrologyProfile chart = fortune.Tables.ProfileFor(character.Birth);
            var profile = new List<ValueRow>
            {
                new ValueRow(localizer.Get(ZodiacKey), localizer.Get(chart.Zodiac.NameKey)),
                new ValueRow(localizer.Get(BirthDayKey), localizer.Get(chart.BirthDay.NameKey)),
                new ValueRow(localizer.Get(AnimalKey), localizer.Get(chart.YearAnimal.NameKey)),
                new ValueRow(localizer.Get(ElementKey), localizer.Get(chart.Element.NameKey)),
            };

            List<FortuneRow> fortunes = FortuneAreas.All
                .Select(area => Fortune(fortune.Explain(character, area, now), localizer))
                .ToList();

            KarmaLedger ledger = character.Karma;
            var karma = new List<ValueRow>
            {
                new ValueRow(localizer.Get(GoodKey), ledger.Good.ToString(DashboardViewModel.ValueFormat, CultureInfo.InvariantCulture), string.Empty, RowTone.Good),
                new ValueRow(localizer.Get(BadKey), ledger.Bad.ToString(DashboardViewModel.ValueFormat, CultureInfo.InvariantCulture), string.Empty,
                    ledger.Bad > 0f ? RowTone.Bad : RowTone.Neutral),
                Signed(localizer.Get(PointsKey), session.Luck.Karma.ChancePoints(ledger)),
                new ValueRow(localizer.Get(DebtKey), ledger.PendingDebt.ToString(DashboardViewModel.ValueFormat, CultureInfo.InvariantCulture), string.Empty,
                    ledger.PendingDebt > 0f ? RowTone.Bad : RowTone.Neutral),
            };

            return new AstrologyState(profile, localizer.Format(YearKey, ("year", now.Year.ToString(CultureInfo.InvariantCulture))), fortunes, karma);
        }

        private static FortuneRow Fortune(FortuneBreakdown breakdown, ILocalizer localizer)
        {
            var parts = new List<ValueRow>
            {
                new ValueRow(localizer.Get(BaseKey), breakdown.Base.ToString(DashboardViewModel.ValueFormat, CultureInfo.InvariantCulture)),
                Signed(localizer.Get(ZodiacPartKey), breakdown.Zodiac),
                Signed(localizer.Get(BirthDayPartKey), breakdown.BirthDay),
                Signed(localizer.Get(ElementPartKey), breakdown.Element),
                Signed(localizer.Get(CyclePartKey), breakdown.Cycle),
                Signed(localizer.Get(YearlyPartKey), breakdown.Yearly),
            };
            return new FortuneRow(localizer.Get(SessionText.EnumKey(AreaPrefix, breakdown.Area)), breakdown.Value,
                breakdown.Value.ToString(DashboardViewModel.ValueFormat, CultureInfo.InvariantCulture), parts);
        }

        private static ValueRow Signed(string label, float points)
        {
            return new ValueRow(label, points.ToString(PointsFormat, CultureInfo.InvariantCulture), string.Empty, ValueRow.ToneOf(points));
        }
    }
}
