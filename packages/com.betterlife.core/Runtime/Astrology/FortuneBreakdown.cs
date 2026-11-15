namespace BetterLife.Core.Astrology
{
    /// <summary>
    /// How a fortune value is composed, for the astrology screen.
    /// </summary>
    public sealed class FortuneBreakdown
    {
        /// <summary>
        /// Creates a breakdown.
        /// </summary>
        /// <param name="area">Area.</param>
        /// <param name="year">Calendar year.</param>
        /// <param name="baseFortune">Base points.</param>
        /// <param name="zodiac">Zodiac points.</param>
        /// <param name="birthDay">Birth day points.</param>
        /// <param name="element">Element points.</param>
        /// <param name="cycle">Yearly cycle points.</param>
        /// <param name="yearly">Yearly random points.</param>
        /// <param name="value">Final value (clamped 0-100).</param>
        public FortuneBreakdown(FortuneArea area, int year, float baseFortune, float zodiac, float birthDay, float element, float cycle, float yearly, float value)
        {
            Area = area;
            Year = year;
            Base = baseFortune;
            Zodiac = zodiac;
            BirthDay = birthDay;
            Element = element;
            Cycle = cycle;
            Yearly = yearly;
            Value = value;
        }

        /// <summary>Area.</summary>
        public FortuneArea Area { get; }

        /// <summary>Calendar year.</summary>
        public int Year { get; }

        /// <summary>Base points.</summary>
        public float Base { get; }

        /// <summary>Zodiac points.</summary>
        public float Zodiac { get; }

        /// <summary>Birth day points.</summary>
        public float BirthDay { get; }

        /// <summary>Element points.</summary>
        public float Element { get; }

        /// <summary>Yearly cycle points.</summary>
        public float Cycle { get; }

        /// <summary>Yearly random points.</summary>
        public float Yearly { get; }

        /// <summary>Final value, 0-100.</summary>
        public float Value { get; }
    }
}
