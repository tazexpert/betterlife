using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Time;

namespace BetterLife.Core.Astrology
{
    /// <summary>
    /// All astrology content (content/astrology.json), validated for completeness at construction:
    /// twelve signs with distinct start days, all eight Thai birth days, twelve year animals with known elements.
    /// </summary>
    public sealed class AstrologyTables
    {
        /// <summary>Number of zodiac signs and of year animals.</summary>
        public const int CycleLength = 12;

        private readonly List<ZodiacSignDefinition> _signsByStart;
        private readonly Dictionary<ThaiDayOfWeek, BirthDayDefinition> _birthDays;
        private readonly List<YearAnimalDefinition> _yearAnimals;
        private readonly DefinitionCatalog<ElementDefinition> _elements;

        /// <summary>
        /// Creates the tables.
        /// </summary>
        /// <param name="baseFortune">Fortune before modifiers (0-100).</param>
        /// <param name="cycle">Yearly cycle.</param>
        /// <param name="yearlyNoise">Maximum random points per year and area; non-negative.</param>
        /// <param name="yearAnimalAnchorYear">A calendar year of the first animal in <paramref name="yearAnimals"/>.</param>
        /// <param name="signs">Twelve zodiac signs in order.</param>
        /// <param name="birthDays">All eight Thai birth days.</param>
        /// <param name="elements">Elements.</param>
        /// <param name="yearAnimals">Twelve year animals in cycle order.</param>
        public AstrologyTables(
            float baseFortune,
            FortuneCycle cycle,
            float yearlyNoise,
            int yearAnimalAnchorYear,
            IEnumerable<ZodiacSignDefinition> signs,
            IEnumerable<BirthDayDefinition> birthDays,
            IEnumerable<ElementDefinition> elements,
            IEnumerable<YearAnimalDefinition> yearAnimals)
        {
            if (!StatBounds.IsInRange(baseFortune))
            {
                throw new ArgumentOutOfRangeException(nameof(baseFortune), baseFortune, "Base fortune must be within 0-100.");
            }
            if (!(yearlyNoise >= 0f) || float.IsInfinity(yearlyNoise))
            {
                throw new ArgumentOutOfRangeException(nameof(yearlyNoise), yearlyNoise, "Noise must be non-negative.");
            }

            BaseFortune = baseFortune;
            Cycle = cycle ?? throw new ArgumentNullException(nameof(cycle));
            YearlyNoise = yearlyNoise;
            YearAnimalAnchorYear = yearAnimalAnchorYear;
            Signs = RequireCount(signs, nameof(signs));
            if (Signs.Select(s => s.Id).Distinct().Count() != CycleLength || Signs.Select(s => s.StartKey).Distinct().Count() != CycleLength)
            {
                throw new ArgumentException("Zodiac signs need unique ids and unique start days.", nameof(signs));
            }
            _signsByStart = Signs.OrderBy(s => s.StartKey).ToList();

            _birthDays = new Dictionary<ThaiDayOfWeek, BirthDayDefinition>();
            foreach (BirthDayDefinition day in birthDays ?? throw new ArgumentNullException(nameof(birthDays)))
            {
                if (_birthDays.ContainsKey(day.Day))
                {
                    throw new ArgumentException($"Birth day {day.Day} is defined twice.", nameof(birthDays));
                }
                _birthDays.Add(day.Day, day);
            }
            if (Enum.GetValues(typeof(ThaiDayOfWeek)).Cast<ThaiDayOfWeek>().Any(d => !_birthDays.ContainsKey(d)))
            {
                throw new ArgumentException("All eight Thai birth days must be defined.", nameof(birthDays));
            }

            _elements = new DefinitionCatalog<ElementDefinition>(elements ?? throw new ArgumentNullException(nameof(elements)));
            _yearAnimals = RequireCount(yearAnimals, nameof(yearAnimals));
            if (_yearAnimals.Select(a => a.Id).Distinct().Count() != CycleLength)
            {
                throw new ArgumentException("Year animal ids must be unique.", nameof(yearAnimals));
            }
            foreach (YearAnimalDefinition animal in _yearAnimals.Where(a => !_elements.Contains(a.ElementId)))
            {
                throw new ArgumentException($"Year animal '{animal.Id}' uses unknown element '{animal.ElementId}'.", nameof(yearAnimals));
            }
        }

        /// <summary>Fortune before modifiers.</summary>
        public float BaseFortune { get; }

        /// <summary>Yearly cycle.</summary>
        public FortuneCycle Cycle { get; }

        /// <summary>Maximum random points per year and area.</summary>
        public float YearlyNoise { get; }

        /// <summary>A year of the first animal.</summary>
        public int YearAnimalAnchorYear { get; }

        /// <summary>Zodiac signs in declaration order.</summary>
        public IReadOnlyList<ZodiacSignDefinition> Signs { get; }

        /// <summary>Year animals in cycle order.</summary>
        public IReadOnlyList<YearAnimalDefinition> YearAnimals => _yearAnimals;

        /// <summary>
        /// Returns the zodiac sign for a calendar date (the last sign whose start is on or before the date, wrapping at the year end).
        /// </summary>
        /// <param name="date">Date.</param>
        /// <returns>The sign.</returns>
        public ZodiacSignDefinition SignFor(CalendarDate date)
        {
            int key = date.Month * ZodiacSignDefinition.MonthDayFactor + date.Day;
            return _signsByStart.LastOrDefault(s => s.StartKey <= key) ?? _signsByStart[_signsByStart.Count - 1];
        }

        /// <summary>
        /// Returns the year animal of a calendar year.
        /// </summary>
        /// <param name="year">Calendar year.</param>
        /// <returns>The animal.</returns>
        public YearAnimalDefinition AnimalFor(int year)
        {
            int index = ((year - YearAnimalAnchorYear) % CycleLength + CycleLength) % CycleLength;
            return _yearAnimals[index];
        }

        /// <summary>
        /// Builds the astrology profile of a birth.
        /// </summary>
        /// <param name="birth">Birth facts.</param>
        /// <returns>The profile.</returns>
        public AstrologyProfile ProfileFor(BirthInfo birth)
        {
            if (birth == null)
            {
                throw new ArgumentNullException(nameof(birth));
            }
            ZodiacSignDefinition sign = SignFor(birth.Date);
            YearAnimalDefinition animal = AnimalFor(birth.Date.Year);
            int signIndex = Signs.ToList().IndexOf(sign);
            return new AstrologyProfile(sign, signIndex, _birthDays[birth.ThaiDay], animal, _elements.Get(animal.ElementId));
        }

        private static List<T> RequireCount<T>(IEnumerable<T> items, string paramName) where T : class
        {
            List<T> list = (items ?? throw new ArgumentNullException(paramName)).ToList();
            if (list.Count != CycleLength || list.Any(i => i == null))
            {
                throw new ArgumentException($"Exactly {CycleLength} entries are required.", paramName);
            }
            return list;
        }
    }
}
