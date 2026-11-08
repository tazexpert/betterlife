using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Family;
using BetterLife.Core.Domain.Flags;
using BetterLife.Core.Domain.Genetics;
using BetterLife.Core.Domain.Life;
using BetterLife.Core.Domain.Skills;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Domain.Traits;
using BetterLife.Core.Domain.World;
using BetterLife.Core.Careers;
using BetterLife.Core.Economy;
using BetterLife.Core.Education;
using BetterLife.Core.Karma;
using BetterLife.Core.Time;

namespace BetterLife.Core.Domain.Characters
{
    /// <summary>
    /// Aggregate root of the player character. <see cref="ApplyStatChange"/> is the only way to change stats,
    /// so every change passes through trait modifiers, clamping and the change log.
    /// </summary>
    public sealed class Character
    {
        private readonly List<HealthCondition> _conditions = new List<HealthCondition>();

        /// <summary>
        /// Creates a character with default stats and no traits, skills or flags.
        /// </summary>
        /// <param name="id">Character id.</param>
        /// <param name="name">Name.</param>
        /// <param name="gender">Gender profile.</param>
        /// <param name="birth">Birth facts.</param>
        /// <param name="catalogs">Content catalogs.</param>
        public Character(CharacterId id, PersonName name, GenderProfile gender, BirthInfo birth, CharacterCatalogs catalogs)
        {
            if (catalogs == null)
            {
                throw new ArgumentNullException(nameof(catalogs));
            }

            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Gender = gender ?? throw new ArgumentNullException(nameof(gender));
            Birth = birth ?? throw new ArgumentNullException(nameof(birth));
            Catalogs = catalogs;
            Stats = new StatBlock(catalogs.Stats);
            ChangeLog = new StatChangeLog();
            History = new StatHistory(catalogs.Stats.CoreStatIds);
            Reputation = new ReputationBook(catalogs.ReputationDomains);
            Wallet = new Wallet();
            Traits = new TraitSet(catalogs.Traits);
            Skills = new SkillSet(catalogs.Skills);
            Flags = new FlagSet(catalogs.Flags);
            Karma = new KarmaLedger();
            Genome = new Genome();
            Residence = new Residence(birth.CountryId, null);
        }

        /// <summary>Character id.</summary>
        public CharacterId Id { get; }

        /// <summary>Name.</summary>
        public PersonName Name { get; private set; }

        /// <summary>Gender profile.</summary>
        public GenderProfile Gender { get; private set; }

        /// <summary>Birth facts.</summary>
        public BirthInfo Birth { get; }

        /// <summary>Content catalogs.</summary>
        public CharacterCatalogs Catalogs { get; }

        /// <summary>Stat values (read through <see cref="IStatReader.GetValue"/>).</summary>
        public StatBlock Stats { get; }

        /// <summary>Recent applied stat changes.</summary>
        public StatChangeLog ChangeLog { get; }

        /// <summary>Weekly samples of core stats.</summary>
        public StatHistory History { get; }

        /// <summary>Hidden reputation per domain.</summary>
        public ReputationBook Reputation { get; }

        /// <summary>Active illnesses and injuries.</summary>
        public IReadOnlyList<HealthCondition> Conditions => _conditions;

        /// <summary>Cash.</summary>
        public Wallet Wallet { get; }

        /// <summary>Traits.</summary>
        public TraitSet Traits { get; }

        /// <summary>Skills.</summary>
        public SkillSet Skills { get; }

        /// <summary>Story flags.</summary>
        public FlagSet Flags { get; }

        /// <summary>Good and bad karma and pending karmic debts.</summary>
        public KarmaLedger Karma { get; }

        /// <summary>Assets, loans and lifestyle.</summary>
        public CharacterFinances Finances { get; } = new CharacterFinances();

        /// <summary>Schooling: current enrollment and credentials.</summary>
        public CharacterEducation Education { get; } = new CharacterEducation();

        /// <summary>Work: current job, past jobs and pension.</summary>
        public CharacterCareer Career { get; } = new CharacterCareer();

        /// <summary>Cash + assets − debts.</summary>
        public decimal NetWorth => Finances.NetWorth(Wallet.Cash);

        /// <summary>Inherited potential, passed on to children.</summary>
        public Genome Genome { get; }

        /// <summary>Where the character lives (country of birth until set).</summary>
        public Residence Residence { get; private set; }

        /// <summary>The family the character was born into, or null before creation sets it.</summary>
        public FamilyInfo Family { get; private set; }

        /// <summary>How the character died, or null while alive.</summary>
        public DeathRecord Death { get; private set; }

        /// <summary>True until the character dies.</summary>
        public bool IsAlive => Death == null;

        /// <summary>
        /// Returns the life stage at a week.
        /// </summary>
        /// <param name="now">Week.</param>
        /// <returns>The stage.</returns>
        public LifeStageDefinition LifeStage(GameDate now) => Catalogs.LifeStages.StageFor(AgeAt(now));

        /// <summary>
        /// Ends the life. Dying twice is an error.
        /// </summary>
        /// <param name="causeId">Death cause id.</param>
        /// <param name="week">Week of death.</param>
        /// <returns>The death record.</returns>
        public DeathRecord Die(string causeId, GameDate week)
        {
            if (!IsAlive)
            {
                throw new InvalidOperationException("The character is already dead.");
            }
            Death = new DeathRecord(week, causeId, AgeAt(week));
            return Death;
        }

        /// <summary>
        /// Restores a saved death.
        /// </summary>
        /// <param name="death">Saved record.</param>
        public void RestoreDeath(DeathRecord death)
        {
            Death = death ?? throw new ArgumentNullException(nameof(death));
        }

        /// <summary>
        /// Moves the character. The country and city must be declared, and the city must belong to the country.
        /// </summary>
        /// <param name="residence">New residence.</param>
        public void MoveTo(Residence residence)
        {
            if (residence == null)
            {
                throw new ArgumentNullException(nameof(residence));
            }
            CountryDefinition country = Catalogs.Countries.Get(residence.CountryId);
            if (residence.CityId != null && country.FindCity(residence.CityId) == null)
            {
                throw new ArgumentException($"City '{residence.CityId}' is not in '{residence.CountryId}'.", nameof(residence));
            }
            Residence = residence;
        }

        /// <summary>
        /// Returns the effective location modifiers where the character lives.
        /// </summary>
        /// <returns>Complete modifiers.</returns>
        public LocationModifiers LocationModifiers()
        {
            return Catalogs.Countries.Get(Residence.CountryId).ModifiersIn(Residence.CityId);
        }

        /// <summary>
        /// Sets the birth family. The background must be declared.
        /// </summary>
        /// <param name="family">Family info.</param>
        public void SetFamily(FamilyInfo family)
        {
            if (family == null)
            {
                throw new ArgumentNullException(nameof(family));
            }
            Catalogs.FamilyBackgrounds.Get(family.BackgroundId);
            Family = family;
        }

        /// <summary>
        /// Returns the age in whole years.
        /// </summary>
        /// <param name="now">Current week.</param>
        /// <returns>Age in years.</returns>
        public int AgeAt(GameDate now) => Birth.Date.AgeAt(now);

        /// <summary>
        /// Applies a change to a stored stat: trait multipliers, then clamping, then logging.
        /// Derived stats cannot be changed directly; change their inputs instead.
        /// </summary>
        /// <param name="change">The change request.</param>
        /// <param name="now">Current week.</param>
        /// <returns>What was requested and what was applied.</returns>
        public StatChangeResult ApplyStatChange(StatChange change, GameDate now)
        {
            if (change == null)
            {
                throw new ArgumentNullException(nameof(change));
            }
            if (Catalogs.Stats.IsDerived(change.StatId))
            {
                throw new InvalidOperationException($"Stat '{change.StatId}' is derived; change one of its inputs instead.");
            }

            float modified = change.Delta * Traits.GetMultiplier(change.StatId, change.Delta);
            float before = Stats.GetValue(change.StatId);
            float after = Stats.SetStored(change.StatId, before + modified);
            float applied = after - before;
            if (applied != 0f)
            {
                ChangeLog.Add(new StatChangeLogEntry(now, change.StatId, applied, change.SourceKey));
            }
            return new StatChangeResult(change.StatId, change.Delta, modified, applied, after);
        }

        /// <summary>
        /// Explains a stat: derived stats show their formula, stored stats show recent changes by source.
        /// </summary>
        /// <param name="statId">Stat id.</param>
        /// <param name="now">Current week.</param>
        /// <returns>The breakdown.</returns>
        public StatBreakdown Explain(StatId statId, GameDate now)
        {
            if (Catalogs.Stats.IsDerived(statId))
            {
                return Stats.ExplainFormula(statId);
            }
            return new StatBreakdown(statId, Stats.GetValue(statId), StatBreakdownKind.RecentChanges, ChangeLog.Summarize(statId, now));
        }

        /// <summary>
        /// Records this week's core stat values for sparklines. Call once per week.
        /// </summary>
        /// <param name="week">Current week.</param>
        public void RecordWeeklyHistory(GameDate week)
        {
            History.Record(week, Stats);
            ChangeLog.Prune(week);
        }

        /// <summary>
        /// Adds or replaces a health condition with the same id.
        /// </summary>
        /// <param name="condition">A condition of a declared type.</param>
        public void SetCondition(HealthCondition condition)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }
            Catalogs.HealthConditions.Get(condition.ConditionId);
            RemoveCondition(condition.ConditionId);
            _conditions.Add(condition);
        }

        /// <summary>
        /// Removes a health condition.
        /// </summary>
        /// <param name="conditionId">Condition id.</param>
        /// <returns>True when it was active.</returns>
        public bool RemoveCondition(string conditionId)
        {
            return _conditions.RemoveAll(c => string.Equals(c.ConditionId, conditionId, StringComparison.Ordinal)) > 0;
        }

        /// <summary>
        /// Returns true when a condition is active.
        /// </summary>
        /// <param name="conditionId">Condition id.</param>
        /// <returns>True when active.</returns>
        public bool HasCondition(string conditionId)
        {
            return _conditions.Any(c => string.Equals(c.ConditionId, conditionId, StringComparison.Ordinal));
        }

        /// <summary>
        /// Renames the character (for example after marriage).
        /// </summary>
        /// <param name="name">New name.</param>
        public void Rename(PersonName name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Updates the gender profile (player choice).
        /// </summary>
        /// <param name="gender">New profile; must keep the same biological sex.</param>
        public void UpdateGender(GenderProfile gender)
        {
            if (gender == null)
            {
                throw new ArgumentNullException(nameof(gender));
            }
            if (gender.Sex != Gender.Sex)
            {
                throw new InvalidOperationException("Biological sex is fixed at birth.");
            }
            Gender = gender;
        }
    }
}
