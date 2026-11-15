using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Scams;
using BetterLife.Core.Time;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// Converts <see cref="CharacterScams"/> to and from <see cref="ScamsDto"/>.
    /// </summary>
    public static class ScamsDtoMapper
    {
        /// <summary>
        /// Captures the character's history with scammers.
        /// </summary>
        /// <param name="scams">History.</param>
        /// <returns>The DTO.</returns>
        public static ScamsDto ToDto(CharacterScams scams)
        {
            if (scams == null)
            {
                throw new ArgumentNullException(nameof(scams));
            }
            return new ScamsDto
            {
                Incidents = scams.Incidents.Select(i => new ScamIncidentDto
                {
                    Id = i.Id,
                    ScamId = i.ScamId,
                    Week = i.Week.TotalWeeks,
                    Lost = i.Lost.ToString(CultureInfo.InvariantCulture),
                    Recovered = i.Recovered.ToString(CultureInfo.InvariantCulture),
                    Reported = i.Reported,
                }).ToList(),
                Targeted = scams.Targeted,
                Detected = scams.Detected,
                Victimized = scams.Victimized,
                NetLost = scams.NetLost.ToString(CultureInfo.InvariantCulture),
                NextIncidentId = scams.NextIncidentId,
            };
        }

        /// <summary>
        /// Restores a saved history. A null DTO (an older save) leaves it empty.
        /// </summary>
        /// <param name="dto">Saved history, or null.</param>
        /// <param name="scams">Target.</param>
        public static void Restore(ScamsDto dto, CharacterScams scams)
        {
            if (scams == null)
            {
                throw new ArgumentNullException(nameof(scams));
            }
            if (dto == null)
            {
                return;
            }
            try
            {
                List<ScamIncident> incidents = (dto.Incidents ?? new List<ScamIncidentDto>()).Where(i => i != null).Select(i => new ScamIncident(i.Id, i.ScamId,
                    new GameDate(i.Week), FinancesDtoMapper.ParseMoney(i.Lost, nameof(i.Lost)), FinancesDtoMapper.ParseMoney(i.Recovered ?? "0", nameof(i.Recovered)),
                    i.Reported)).ToList();
                scams.Restore(incidents, dto.Targeted, dto.Detected, dto.Victimized,
                    dto.NetLost == null ? 0m : FinancesDtoMapper.ParseMoney(dto.NetLost, nameof(dto.NetLost)), dto.NextIncidentId);
            }
            catch (ArgumentException ex)
            {
                throw new FormatException("Saved scams are invalid: " + ex.Message, ex);
            }
        }
    }
}
