using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Careers;
using BetterLife.Core.Time;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// Converts <see cref="CharacterCareer"/> to and from <see cref="CareerDto"/>.
    /// </summary>
    public static class CareerDtoMapper
    {
        /// <summary>
        /// Captures a character's working life.
        /// </summary>
        /// <param name="career">Working life.</param>
        /// <returns>The DTO.</returns>
        public static CareerDto ToDto(CharacterCareer career)
        {
            if (career == null)
            {
                throw new ArgumentNullException(nameof(career));
            }
            Job job = career.Current;
            return new CareerDto
            {
                Current = job == null ? null : new JobDto
                {
                    TrackId = job.TrackId,
                    LevelIndex = job.LevelIndex,
                    StartedWeek = job.StartedOn.TotalWeeks,
                    LevelSinceWeek = job.LevelSince.TotalWeeks,
                    Performance = job.Performance,
                    BossSatisfaction = job.BossSatisfaction,
                    LowBossMonths = job.LowBossMonths,
                    HasReview = job.NextReviewOn.HasValue,
                    NextReviewWeek = job.NextReviewOn.HasValue ? job.NextReviewOn.Value.TotalWeeks : 0,
                },
                History = career.History.Select(h => new PastJobDto
                {
                    TrackId = h.TrackId,
                    LevelIndex = h.LevelIndex,
                    StartedWeek = h.StartedOn.TotalWeeks,
                    EndedWeek = h.EndedOn.TotalWeeks,
                    Reason = h.Reason.ToString(),
                }).ToList(),
                Retired = career.IsRetired,
                PensionMonthly = career.PensionMonthly.ToString(CultureInfo.InvariantCulture),
                Attempts = EducationDtoMapper.Attempts(career.Attempts),
            };
        }

        /// <summary>
        /// Restores a saved working life. A null DTO (an older save) leaves it empty.
        /// </summary>
        /// <param name="dto">Saved working life, or null.</param>
        /// <param name="career">Target.</param>
        public static void Restore(CareerDto dto, CharacterCareer career)
        {
            if (career == null)
            {
                throw new ArgumentNullException(nameof(career));
            }
            if (dto == null)
            {
                return;
            }
            try
            {
                JobDto j = dto.Current;
                Job job = j == null ? null : new Job(j.TrackId, j.LevelIndex, new GameDate(j.StartedWeek), new GameDate(j.LevelSinceWeek), j.Performance,
                    j.BossSatisfaction, j.LowBossMonths, j.HasReview ? new GameDate(j.NextReviewWeek) : (GameDate?)null);
                List<PastJob> history = (dto.History ?? new List<PastJobDto>()).Where(h => h != null)
                    .Select(h => new PastJob(h.TrackId, h.LevelIndex, new GameDate(h.StartedWeek), new GameDate(h.EndedWeek),
                        EducationDtoMapper.ParseEnum<JobEndReason>(h.Reason, nameof(h.Reason)))).ToList();
                decimal pension = dto.PensionMonthly == null ? 0m : FinancesDtoMapper.ParseMoney(dto.PensionMonthly, nameof(dto.PensionMonthly));
                career.Restore(job, history, dto.Retired, pension, EducationDtoMapper.FromAttempts(dto.Attempts));
            }
            catch (ArgumentException ex)
            {
                throw new FormatException("Saved career is invalid: " + ex.Message, ex);
            }
        }
    }
}
