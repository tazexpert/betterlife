using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Time;
using BetterLife.Core.Vices;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// Converts <see cref="CharacterVices"/> to and from <see cref="VicesDto"/>.
    /// </summary>
    public static class VicesDtoMapper
    {
        /// <summary>
        /// Captures the character's habits.
        /// </summary>
        /// <param name="vices">Habits.</param>
        /// <returns>The DTO.</returns>
        public static VicesDto ToDto(CharacterVices vices)
        {
            if (vices == null)
            {
                throw new ArgumentNullException(nameof(vices));
            }
            return new VicesDto
            {
                States = vices.States.OrderBy(s => s.ViceId, StringComparer.Ordinal).Select(s => new ViceStateDto
                {
                    ViceId = s.ViceId,
                    Addiction = s.Addiction,
                    Tolerance = s.Tolerance,
                    Uses = s.Uses,
                    HasLastUse = s.LastUsedOn.HasValue,
                    LastUseWeek = s.LastUsedOn?.TotalWeeks ?? 0,
                    Bets = s.Bets,
                    NetWinnings = s.NetWinnings.ToString(CultureInfo.InvariantCulture),
                    Discovered = s.Discovered,
                    Relapses = s.Relapses,
                    Quits = s.Quits,
                    QuitMethodId = s.Quitting?.MethodId,
                    QuitStartWeek = s.Quitting?.StartedOn.TotalWeeks ?? 0,
                    QuitEndWeek = s.Quitting?.EndsOn.TotalWeeks ?? 0,
                }).ToList(),
                HasIrreversible = vices.LastIrreversibleWeek.HasValue,
                LastIrreversibleWeek = vices.LastIrreversibleWeek?.TotalWeeks ?? 0,
                IrreversibleActions = vices.IrreversibleActions,
            };
        }

        /// <summary>
        /// Restores saved habits. A null DTO (an older save) leaves them empty.
        /// </summary>
        /// <param name="dto">Saved habits, or null.</param>
        /// <param name="vices">Target.</param>
        public static void Restore(VicesDto dto, CharacterVices vices)
        {
            if (vices == null)
            {
                throw new ArgumentNullException(nameof(vices));
            }
            if (dto == null)
            {
                return;
            }
            try
            {
                List<ViceState> states = (dto.States ?? new List<ViceStateDto>()).Where(s => s != null).Select(s => new ViceState(s.ViceId, s.Addiction,
                    s.Tolerance, s.Uses, s.HasLastUse ? new GameDate(s.LastUseWeek) : (GameDate?)null, s.Bets,
                    s.NetWinnings == null ? 0m : FinancesDtoMapper.ParseMoney(s.NetWinnings, nameof(s.NetWinnings)), s.Discovered, s.Relapses, s.Quits,
                    s.QuitMethodId == null ? null : new QuitAttempt(s.QuitMethodId, new GameDate(s.QuitStartWeek), new GameDate(s.QuitEndWeek)))).ToList();
                vices.Restore(states, dto.HasIrreversible ? new GameDate(dto.LastIrreversibleWeek) : (GameDate?)null, dto.IrreversibleActions);
            }
            catch (ArgumentException ex)
            {
                throw new FormatException("Saved vices are invalid: " + ex.Message, ex);
            }
        }
    }
}
