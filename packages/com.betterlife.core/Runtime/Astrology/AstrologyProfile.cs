using BetterLife.Core.Time;

namespace BetterLife.Core.Astrology
{
    /// <summary>
    /// A person's fixed astrological facts derived from their birth.
    /// </summary>
    public sealed class AstrologyProfile
    {
        /// <summary>
        /// Creates a profile.
        /// </summary>
        /// <param name="zodiac">Zodiac sign.</param>
        /// <param name="zodiacIndex">Position of the sign in the table (0-11), used for the personal cycle phase.</param>
        /// <param name="birthDay">Thai astrological birth day.</param>
        /// <param name="yearAnimal">Year animal.</param>
        /// <param name="element">Element of the birth year.</param>
        public AstrologyProfile(ZodiacSignDefinition zodiac, int zodiacIndex, BirthDayDefinition birthDay, YearAnimalDefinition yearAnimal, ElementDefinition element)
        {
            Zodiac = zodiac;
            ZodiacIndex = zodiacIndex;
            BirthDay = birthDay;
            YearAnimal = yearAnimal;
            Element = element;
        }

        /// <summary>Zodiac sign.</summary>
        public ZodiacSignDefinition Zodiac { get; }

        /// <summary>Position of the sign in the table.</summary>
        public int ZodiacIndex { get; }

        /// <summary>Thai astrological birth day.</summary>
        public BirthDayDefinition BirthDay { get; }

        /// <summary>Thai day value.</summary>
        public ThaiDayOfWeek ThaiDay => BirthDay.Day;

        /// <summary>Year animal.</summary>
        public YearAnimalDefinition YearAnimal { get; }

        /// <summary>Element of the birth year.</summary>
        public ElementDefinition Element { get; }
    }
}
