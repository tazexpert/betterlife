using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Time;

namespace BetterLife.Core.Astrology
{
    /// <summary>
    /// Fortune modifiers of a Thai astrological birth day.
    /// </summary>
    public sealed class BirthDayDefinition
    {
        /// <summary>
        /// Creates a definition.
        /// </summary>
        /// <param name="day">Thai birth day.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="modifiers">Fortune points.</param>
        public BirthDayDefinition(ThaiDayOfWeek day, string nameKey, FortuneModifiers modifiers)
        {
            Day = day;
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            Modifiers = modifiers ?? throw new ArgumentNullException(nameof(modifiers));
        }

        /// <summary>Thai birth day.</summary>
        public ThaiDayOfWeek Day { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Fortune points.</summary>
        public FortuneModifiers Modifiers { get; }
    }
}
