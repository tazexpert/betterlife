using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Flags;
using BetterLife.Core.Domain.Skills;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Domain.Traits;
using BetterLife.Core.Time;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// Converts a <see cref="Character"/> to and from <see cref="CharacterDto"/>.
    /// Loading skips entries whose ids no longer exist in the content and reports them as warnings,
    /// so removing content never corrupts a save. Structurally invalid data throws <see cref="FormatException"/>.
    /// </summary>
    public static class CharacterDtoMapper
    {
        /// <summary>
        /// Captures a character.
        /// </summary>
        /// <param name="character">Character to save.</param>
        /// <returns>The DTO.</returns>
        public static CharacterDto ToDto(Character character)
        {
            if (character == null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            GameDate? lastWeek = character.History.LastRecordedWeek;
            return new CharacterDto
            {
                Id = character.Id.Value,
                GivenName = character.Name.GivenName,
                FamilyName = character.Name.FamilyName,
                Sex = character.Gender.Sex.ToString(),
                GenderIdentity = character.Gender.Identity.ToString(),
                SpeechStyle = character.Gender.SpeechStyle.ToString(),
                Birth = FoundationDtoMapper.ToDto(character.Birth.Date),
                BirthHour = character.Birth.Hour,
                BirthCountryId = character.Birth.CountryId,
                Stats = character.Stats.StoredValues
                    .Select(p => new StatValueDto { Id = p.Key.Value, Value = p.Value })
                    .OrderBy(s => s.Id, StringComparer.Ordinal).ToList(),
                Reputation = character.Reputation.Values
                    .Select(p => new ReputationDto { DomainId = p.Key, Value = p.Value })
                    .OrderBy(r => r.DomainId, StringComparer.Ordinal).ToList(),
                Conditions = character.Conditions
                    .Select(c => new HealthConditionDto { Id = c.ConditionId, Severity = c.Severity, StartedWeek = c.StartedOn.TotalWeeks })
                    .ToList(),
                Cash = character.Wallet.Cash.ToString(CultureInfo.InvariantCulture),
                Traits = character.Traits.Instances
                    .Select(t => new TraitDto { Id = t.TraitId, AcquiredWeek = t.AcquiredOn.TotalWeeks, Revealed = t.IsRevealed })
                    .ToList(),
                Skills = character.Skills.All
                    .Select(s => new SkillDto { Id = s.SkillId, Level = s.Level, Peak = s.PeakLevel, LastPracticedWeek = s.LastPracticedOn.TotalWeeks })
                    .OrderBy(s => s.Id, StringComparer.Ordinal).ToList(),
                Flags = character.Flags.Entries
                    .Select(f => new FlagDto
                    {
                        Id = f.FlagId,
                        Value = f.Value,
                        SetWeek = f.SetOn.TotalWeeks,
                        HasExpiry = f.ExpiresOn.HasValue,
                        ExpiresWeek = f.ExpiresOn.HasValue ? f.ExpiresOn.Value.TotalWeeks : 0,
                    })
                    .OrderBy(f => f.Id, StringComparer.Ordinal).ToList(),
                ChangeLog = character.ChangeLog.Entries
                    .Select(e => new StatChangeLogDto { Week = e.Week.TotalWeeks, StatId = e.StatId.Value, Delta = e.Delta, SourceKey = e.SourceKey })
                    .ToList(),
                History = character.History.TrackedStats
                    .Select(id => new StatHistoryDto { StatId = id.Value, Values = character.History.GetSamples(id).ToList() })
                    .ToList(),
                HasHistory = lastWeek.HasValue,
                HistoryLastWeek = lastWeek.HasValue ? lastWeek.Value.TotalWeeks : 0,
            };
        }

        /// <summary>
        /// Rebuilds a character.
        /// </summary>
        /// <param name="dto">Saved character.</param>
        /// <param name="catalogs">Current content catalogs.</param>
        /// <returns>The character and any warnings.</returns>
        public static CharacterLoadResult FromDto(CharacterDto dto, CharacterCatalogs catalogs)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            if (catalogs == null)
            {
                throw new ArgumentNullException(nameof(catalogs));
            }

            var warnings = new List<string>();
            Character character = CreateShell(dto, catalogs);
            RestoreStats(dto, character, catalogs.Stats, warnings);
            RestoreReputation(dto, character, catalogs.ReputationDomains, warnings);
            RestoreConditions(dto, character, catalogs.HealthConditions, warnings);
            RestoreWallet(dto, character);
            RestoreTraits(dto, character, catalogs.Traits, warnings);
            RestoreSkills(dto, character, catalogs.Skills, warnings);
            RestoreFlags(dto, character, catalogs.Flags, warnings);
            RestoreChangeLog(dto, character, catalogs.Stats, warnings);
            RestoreHistory(dto, character, warnings);
            return new CharacterLoadResult(character, warnings);
        }

        private static Character CreateShell(CharacterDto dto, CharacterCatalogs catalogs)
        {
            try
            {
                var gender = new GenderProfile(
                    ParseEnum<BiologicalSex>(dto.Sex, nameof(dto.Sex)),
                    ParseEnum<GenderIdentity>(dto.GenderIdentity, nameof(dto.GenderIdentity)),
                    ParseEnum<SpeechStyle>(dto.SpeechStyle, nameof(dto.SpeechStyle)));
                var birth = new BirthInfo(FoundationDtoMapper.ToCalendarDate(dto.Birth), dto.BirthHour, dto.BirthCountryId);
                return new Character(new CharacterId(dto.Id), new PersonName(dto.GivenName, dto.FamilyName), gender, birth, catalogs);
            }
            catch (ArgumentException ex)
            {
                throw new FormatException("Character identity data is invalid: " + ex.Message, ex);
            }
        }

        private static void RestoreStats(CharacterDto dto, Character character, StatCatalog catalog, List<string> warnings)
        {
            foreach (StatValueDto stat in Items(dto.Stats))
            {
                if (!TryStatId(stat.Id, out StatId id) || !catalog.Contains(id) || catalog.IsDerived(id))
                {
                    warnings.Add($"Skipped stat '{stat.Id}': not a stored stat in current content.");
                    continue;
                }
                character.Stats.SetStored(id, stat.Value);
            }
        }

        private static void RestoreReputation(CharacterDto dto, Character character, DefinitionCatalog<ReputationDomainDefinition> catalog, List<string> warnings)
        {
            foreach (ReputationDto reputation in Items(dto.Reputation))
            {
                if (!catalog.Contains(reputation.DomainId))
                {
                    warnings.Add($"Skipped reputation '{reputation.DomainId}': not in current content.");
                    continue;
                }
                Wrap(() => character.Reputation.Set(reputation.DomainId, reputation.Value), "reputation");
            }
        }

        private static void RestoreConditions(CharacterDto dto, Character character, DefinitionCatalog<HealthConditionDefinition> catalog, List<string> warnings)
        {
            foreach (HealthConditionDto condition in Items(dto.Conditions))
            {
                if (!catalog.Contains(condition.Id))
                {
                    warnings.Add($"Skipped health condition '{condition.Id}': not in current content.");
                    continue;
                }
                Wrap(() => character.SetCondition(new HealthCondition(condition.Id, condition.Severity, new GameDate(condition.StartedWeek))), "condition");
            }
        }

        private static void RestoreWallet(CharacterDto dto, Character character)
        {
            if (!decimal.TryParse(dto.Cash, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal cash) || cash < 0m)
            {
                throw new FormatException($"Cash '{dto.Cash}' is not a valid non-negative decimal.");
            }
            if (cash > 0m)
            {
                character.Wallet.Deposit(cash);
            }
        }

        private static void RestoreTraits(CharacterDto dto, Character character, DefinitionCatalog<TraitDefinition> catalog, List<string> warnings)
        {
            foreach (TraitDto trait in Items(dto.Traits))
            {
                if (!catalog.Contains(trait.Id))
                {
                    warnings.Add($"Skipped trait '{trait.Id}': not in current content.");
                    continue;
                }
                character.Traits.Restore(new TraitInstance(trait.Id, new GameDate(trait.AcquiredWeek), trait.Revealed));
            }
        }

        private static void RestoreSkills(CharacterDto dto, Character character, DefinitionCatalog<SkillDefinition> catalog, List<string> warnings)
        {
            foreach (SkillDto skill in Items(dto.Skills))
            {
                if (!catalog.Contains(skill.Id))
                {
                    warnings.Add($"Skipped skill '{skill.Id}': not in current content.");
                    continue;
                }
                Wrap(() => character.Skills.Restore(new SkillProgress(skill.Id, skill.Level, skill.Peak, new GameDate(skill.LastPracticedWeek))), "skill");
            }
        }

        private static void RestoreFlags(CharacterDto dto, Character character, DefinitionCatalog<FlagDefinition> catalog, List<string> warnings)
        {
            foreach (FlagDto flag in Items(dto.Flags))
            {
                if (!catalog.Contains(flag.Id))
                {
                    warnings.Add($"Skipped flag '{flag.Id}': not in current content.");
                    continue;
                }
                GameDate? expires = flag.HasExpiry ? new GameDate(flag.ExpiresWeek) : (GameDate?)null;
                Wrap(() => character.Flags.Restore(new FlagEntry(flag.Id, flag.Value, new GameDate(flag.SetWeek), expires)), "flag");
            }
        }

        private static void RestoreChangeLog(CharacterDto dto, Character character, StatCatalog catalog, List<string> warnings)
        {
            foreach (StatChangeLogDto entry in Items(dto.ChangeLog))
            {
                if (!TryStatId(entry.StatId, out StatId id) || !catalog.Contains(id) || !ContentId.IsValid(entry.SourceKey))
                {
                    warnings.Add($"Skipped change log entry for '{entry.StatId}'.");
                    continue;
                }
                character.ChangeLog.Add(new StatChangeLogEntry(new GameDate(entry.Week), id, entry.Delta, entry.SourceKey));
            }
        }

        private static void RestoreHistory(CharacterDto dto, Character character, List<string> warnings)
        {
            GameDate? lastWeek = dto.HasHistory ? new GameDate(dto.HistoryLastWeek) : (GameDate?)null;
            foreach (StatHistoryDto history in Items(dto.History))
            {
                if (!TryStatId(history.StatId, out StatId id) || !character.History.TrackedStats.Contains(id))
                {
                    warnings.Add($"Skipped history for '{history.StatId}': not a tracked stat.");
                    continue;
                }
                character.History.Restore(id, history.Values ?? new List<float>(), lastWeek);
            }
        }

        private static IEnumerable<T> Items<T>(List<T> list) where T : class
        {
            return (list ?? new List<T>()).Where(item => item != null);
        }

        private static bool TryStatId(string value, out StatId id)
        {
            id = default;
            if (!ContentId.IsValid(value))
            {
                return false;
            }
            id = new StatId(value);
            return true;
        }

        private static TEnum ParseEnum<TEnum>(string value, string field) where TEnum : struct
        {
            if (string.IsNullOrEmpty(value) || !Enum.TryParse(value, false, out TEnum result) || !Enum.IsDefined(typeof(TEnum), result))
            {
                throw new FormatException($"Field '{field}' has invalid value '{value}'.");
            }
            return result;
        }

        private static void Wrap(Action restore, string what)
        {
            try
            {
                restore();
            }
            catch (ArgumentException ex)
            {
                throw new FormatException($"Saved {what} is invalid: {ex.Message}", ex);
            }
        }
    }
}
