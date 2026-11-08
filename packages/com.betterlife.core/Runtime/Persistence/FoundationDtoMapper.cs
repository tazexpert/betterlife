using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// Converts Foundation domain types to and from their save DTOs.
    /// </summary>
    public static class FoundationDtoMapper
    {
        private const string HexFormat = "x16";

        /// <summary>Converts a game date to its DTO.</summary>
        /// <param name="date">Date to convert.</param>
        /// <returns>The DTO.</returns>
        public static GameDateDto ToDto(GameDate date)
        {
            return new GameDateDto { TotalWeeks = date.TotalWeeks };
        }

        /// <summary>Converts a DTO back to a game date.</summary>
        /// <param name="dto">DTO to convert.</param>
        /// <returns>The date.</returns>
        public static GameDate ToGameDate(GameDateDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            return new GameDate(dto.TotalWeeks);
        }

        /// <summary>Converts a calendar date to its DTO.</summary>
        /// <param name="date">Date to convert.</param>
        /// <returns>The DTO.</returns>
        public static CalendarDateDto ToDto(CalendarDate date)
        {
            return new CalendarDateDto { Year = date.Year, Month = date.Month, Day = date.Day };
        }

        /// <summary>Converts a DTO back to a validated calendar date.</summary>
        /// <param name="dto">DTO to convert.</param>
        /// <returns>The date.</returns>
        public static CalendarDate ToCalendarDate(CalendarDateDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            return new CalendarDate(dto.Year, dto.Month, dto.Day);
        }

        /// <summary>Captures a registry into its DTO.</summary>
        /// <param name="registry">Registry to capture.</param>
        /// <returns>The DTO.</returns>
        public static RandomRegistryDto ToDto(RandomRegistry registry)
        {
            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            return new RandomRegistryDto
            {
                LifeSeed = ToHex(registry.LifeSeed),
                Streams = registry.CaptureAll()
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => new RandomStreamDto
                    {
                        Name = pair.Key,
                        Seed = ToHex(pair.Value.Seed),
                        State = ToHex(pair.Value.State),
                        Increment = ToHex(pair.Value.Increment),
                    })
                    .ToList(),
            };
        }

        /// <summary>Rebuilds a registry from its DTO.</summary>
        /// <param name="dto">DTO to read.</param>
        /// <returns>A registry with every stream restored.</returns>
        public static RandomRegistry ToRandomRegistry(RandomRegistryDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            var registry = new RandomRegistry(ParseHex(dto.LifeSeed, nameof(dto.LifeSeed)));
            var states = new Dictionary<string, RandomState>(StringComparer.Ordinal);
            foreach (RandomStreamDto stream in dto.Streams ?? new List<RandomStreamDto>())
            {
                if (stream == null || string.IsNullOrEmpty(stream.Name))
                {
                    throw new FormatException("Random stream entry is missing a name.");
                }
                states[stream.Name] = new RandomState(
                    ParseHex(stream.Seed, nameof(stream.Seed)),
                    ParseHex(stream.State, nameof(stream.State)),
                    ParseHex(stream.Increment, nameof(stream.Increment)));
            }
            registry.RestoreAll(states);
            return registry;
        }

        private static string ToHex(ulong value)
        {
            return value.ToString(HexFormat, CultureInfo.InvariantCulture);
        }

        private static ulong ParseHex(string value, string fieldName)
        {
            if (!ulong.TryParse(value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out ulong result))
            {
                throw new FormatException($"Field '{fieldName}' is not a valid 64-bit hex value: '{value}'.");
            }
            return result;
        }
    }
}
