using System;
using System.Collections.Generic;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a character. Lists are used everywhere because JsonUtility cannot serialize dictionaries; decimals are invariant-culture strings.</summary>
    [Serializable]
    public sealed class CharacterDto
    {
        /// <summary>Character id.</summary>
        public string Id;

        /// <summary>Given name.</summary>
        public string GivenName;

        /// <summary>Family name.</summary>
        public string FamilyName;

        /// <summary>BiologicalSex enum name.</summary>
        public string Sex;

        /// <summary>GenderIdentity enum name.</summary>
        public string GenderIdentity;

        /// <summary>SpeechStyle enum name.</summary>
        public string SpeechStyle;

        /// <summary>Birth date.</summary>
        public CalendarDateDto Birth;

        /// <summary>Birth hour, 0-23.</summary>
        public int BirthHour;

        /// <summary>Birth country id.</summary>
        public string BirthCountryId;

        /// <summary>Stored stat values (derived stats are not saved).</summary>
        public List<StatValueDto> Stats = new List<StatValueDto>();

        /// <summary>Reputation per domain.</summary>
        public List<ReputationDto> Reputation = new List<ReputationDto>();

        /// <summary>Active health conditions.</summary>
        public List<HealthConditionDto> Conditions = new List<HealthConditionDto>();

        /// <summary>Cash as an invariant-culture decimal string.</summary>
        public string Cash;

        /// <summary>Held traits.</summary>
        public List<TraitDto> Traits = new List<TraitDto>();

        /// <summary>Skill progress.</summary>
        public List<SkillDto> Skills = new List<SkillDto>();

        /// <summary>Active flags.</summary>
        public List<FlagDto> Flags = new List<FlagDto>();

        /// <summary>Karma ledger.</summary>
        public KarmaDto Karma = new KarmaDto();

        /// <summary>Recent stat changes.</summary>
        public List<StatChangeLogDto> ChangeLog = new List<StatChangeLogDto>();

        /// <summary>Sparkline samples per core stat.</summary>
        public List<StatHistoryDto> History = new List<StatHistoryDto>();

        /// <summary>Whether HistoryLastWeek is meaningful.</summary>
        public bool HasHistory;

        /// <summary>Week of the newest sample (GameDate.TotalWeeks).</summary>
        public int HistoryLastWeek;
    }
}
