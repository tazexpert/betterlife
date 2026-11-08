using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Education;
using BetterLife.Core.Time;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// Converts <see cref="CharacterEducation"/> to and from <see cref="EducationDto"/>.
    /// </summary>
    public static class EducationDtoMapper
    {
        /// <summary>
        /// Captures a character's schooling.
        /// </summary>
        /// <param name="education">Schooling.</param>
        /// <returns>The DTO.</returns>
        public static EducationDto ToDto(CharacterEducation education)
        {
            if (education == null)
            {
                throw new ArgumentNullException(nameof(education));
            }
            Enrollment current = education.Current;
            return new EducationDto
            {
                Current = current == null ? null : new EnrollmentDto
                {
                    ProgramId = current.ProgramId,
                    InstitutionId = current.InstitutionId,
                    StartedWeek = current.StartedOn.TotalWeeks,
                    YearStartedWeek = current.YearStartedOn.TotalWeeks,
                    YearsCompleted = current.YearsCompleted,
                    Repeats = current.Repeats,
                    Performance = current.Performance,
                    Scholarship = current.Scholarship,
                    Funding = current.Funding.ToString(),
                },
                Credentials = education.Credentials.Select(c => new CredentialDto
                {
                    ProgramId = c.ProgramId,
                    InstitutionId = c.InstitutionId,
                    GraduatedWeek = c.GraduatedOn.TotalWeeks,
                    FinalPerformance = c.FinalPerformance,
                }).ToList(),
                Attempts = Attempts(education.Attempts),
            };
        }

        /// <summary>
        /// Restores saved schooling. A null DTO (an older save) leaves it empty.
        /// </summary>
        /// <param name="dto">Saved schooling, or null.</param>
        /// <param name="education">Target.</param>
        public static void Restore(EducationDto dto, CharacterEducation education)
        {
            if (education == null)
            {
                throw new ArgumentNullException(nameof(education));
            }
            if (dto == null)
            {
                return;
            }
            try
            {
                EnrollmentDto c = dto.Current;
                Enrollment current = c == null ? null : new Enrollment(c.ProgramId, c.InstitutionId, new GameDate(c.StartedWeek), new GameDate(c.YearStartedWeek),
                    c.YearsCompleted, c.Repeats, c.Performance, c.Scholarship, ParseEnum<TuitionFunding>(c.Funding, nameof(c.Funding)));
                education.Restore(current,
                    (dto.Credentials ?? new List<CredentialDto>()).Where(x => x != null)
                        .Select(x => new Credential(x.ProgramId, x.InstitutionId, new GameDate(x.GraduatedWeek), x.FinalPerformance)).ToList(),
                    FromAttempts(dto.Attempts));
            }
            catch (ArgumentException ex)
            {
                throw new FormatException("Saved education is invalid: " + ex.Message, ex);
            }
        }

        internal static List<AttemptDto> Attempts(IReadOnlyDictionary<string, GameDate> attempts)
        {
            return attempts.OrderBy(a => a.Key, StringComparer.Ordinal).Select(a => new AttemptDto { Id = a.Key, Week = a.Value.TotalWeeks }).ToList();
        }

        internal static List<KeyValuePair<string, GameDate>> FromAttempts(List<AttemptDto> attempts)
        {
            return (attempts ?? new List<AttemptDto>()).Where(a => a != null && !string.IsNullOrEmpty(a.Id))
                .Select(a => new KeyValuePair<string, GameDate>(a.Id, new GameDate(a.Week))).ToList();
        }

        internal static TEnum ParseEnum<TEnum>(string value, string field) where TEnum : struct
        {
            if (string.IsNullOrEmpty(value) || !Enum.TryParse(value, false, out TEnum result) || !Enum.IsDefined(typeof(TEnum), result))
            {
                throw new FormatException($"Field '{field}' has invalid value '{value}'.");
            }
            return result;
        }
    }
}
