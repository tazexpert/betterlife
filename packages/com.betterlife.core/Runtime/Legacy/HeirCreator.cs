using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Avatars;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Family;
using BetterLife.Core.Domain.Flags;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Education;
using BetterLife.Core.Randomness;
using BetterLife.Core.Relationships;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time;

namespace BetterLife.Core.Legacy
{
    /// <summary>
    /// Turns a living child of a finished life into the next playable character: identity and genes from the NPC,
    /// the family around them, schooling for their age, and the estate, reputation, karma and flags they inherit.
    /// </summary>
    public sealed class HeirCreator
    {
        private const string StreamPrefix = "legacy.";
        private const int HoursPerDay = 24;

        private readonly CharacterCatalogs _catalogs;
        private readonly DefinitionCatalog<EducationProgramDefinition> _programs;
        private readonly LegacySettings _settings;

        /// <summary>
        /// Creates the creator.
        /// </summary>
        /// <param name="catalogs">Character catalogs.</param>
        /// <param name="programs">Education programs (schooling credited to older heirs).</param>
        /// <param name="settings">Inheritance rules.</param>
        public HeirCreator(CharacterCatalogs catalogs, DefinitionCatalog<EducationProgramDefinition> programs, LegacySettings settings)
        {
            _catalogs = catalogs ?? throw new ArgumentNullException(nameof(catalogs));
            _programs = programs ?? throw new ArgumentNullException(nameof(programs));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Living children who can carry on the family line.
        /// </summary>
        /// <param name="deceased">A finished life.</param>
        /// <returns>Relationships with living children (empty while the character is alive).</returns>
        public static IReadOnlyList<Relationship> Heirs(LifeSession deceased)
        {
            if (deceased == null)
            {
                throw new ArgumentNullException(nameof(deceased));
            }
            if (!deceased.IsOver)
            {
                return new Relationship[0];
            }
            return deceased.Character.Relationships.Living(RelationshipType.Child).ToList();
        }

        /// <summary>
        /// Creates the heir.
        /// </summary>
        /// <param name="deceased">A finished life.</param>
        /// <param name="heirNpcId">NPC id of a living child.</param>
        /// <param name="report">How the estate was settled.</param>
        /// <param name="random">Random streams of the new life (seeded from the old life and the heir).</param>
        /// <returns>The new character, at the deceased's death week.</returns>
        /// <exception cref="InvalidOperationException">The life is not over, or the NPC is not a living child.</exception>
        public Character Create(LifeSession deceased, int heirNpcId, out InheritanceReport report, out RandomRegistry random)
        {
            IReadOnlyList<Relationship> heirs = Heirs(deceased);
            Relationship link = heirs.FirstOrDefault(r => r.NpcId == heirNpcId)
                ?? throw new InvalidOperationException("The heir must be a living child of a finished life.");
            Character parent = deceased.Character;
            Npc npc = parent.Relationships.Npc(heirNpcId);
            GameDate start = parent.Death.Week;

            random = new RandomRegistry(StableHash.Fnv1a64(deceased.Seed.ToString(CultureInfo.InvariantCulture) + ":" + heirNpcId.ToString(CultureInfo.InvariantCulture)));
            int hour = random.GetStream(StreamPrefix + "birth").NextInt(0, HoursPerDay);
            var heir = new Character(
                CharacterId.Create(random.GetStream(StreamPrefix + "id")),
                npc.Name,
                GenderProfile.DefaultFor(npc.Sex),
                new BirthInfo(ToCalendarDate(npc.BornOn), hour, parent.Residence?.CountryId ?? parent.Birth.CountryId),
                _catalogs);
            if (parent.Residence != null)
            {
                heir.MoveTo(parent.Residence);
            }
            Npc partner = parent.Relationships.Living()
                .Where(r => RelationshipNames.IsRomantic(r.Type))
                .Select(r => parent.Relationships.Npc(r.NpcId))
                .FirstOrDefault(n => n != null);
            heir.SetFamily(new FamilyInfo(parent.Family?.BackgroundId ?? _catalogs.FamilyBackgrounds.All.First().Id, Parents(parent, partner)));

            foreach (KeyValuePair<StatId, float> gene in npc.Genome.Values)
            {
                heir.Genome.Set(gene.Key, gene.Value);
                heir.Stats.SetStored(gene.Key, gene.Value);
            }
            foreach (string trait in npc.TraitIds.Where(_catalogs.Traits.Contains))
            {
                heir.Traits.Add(trait, heir.Birth.Week);
            }

            AddFamily(heir, deceased, link, npc, partner);
            CreditSchooling(heir, start);
            report = Inherit(heir, deceased, heirs.Count, start);
            heir.SetGeneration(parent.Generation + 1);
            heir.SetAvatarFace(AvatarFaces.Optional(AvatarFaces.Of(parent, npc)));
            return heir;
        }

        /// <summary>
        /// The deceased as a record of the family tree.
        /// </summary>
        /// <param name="deceased">A finished life.</param>
        /// <returns>The record (net worth in the money of that time).</returns>
        public static AncestorRecord Record(LifeSession deceased)
        {
            Character c = deceased.Character;
            return new AncestorRecord(c.Generation, c.Name.GivenName, c.Name.FamilyName, c.Birth.Week, c.Death.Week, c.Death.CauseId, c.Death.AgeYears,
                deceased.Economy.State.Nominal(c.NetWorth));
        }

        private static IEnumerable<ParentRecord> Parents(Character parent, Npc partner)
        {
            string personality = parent.Family?.Parents.FirstOrDefault()?.PersonalityId;
            var parents = new List<ParentRecord>();
            if (personality == null)
            {
                return parents;
            }
            parents.Add(new ParentRecord(parent.Name, parent.Gender.Sex, parent.Birth.Date.Year, personality, parent.Genome, parent.Traits.Instances.Select(t => t.TraitId).ToList()));
            if (partner != null)
            {
                parents.Add(new ParentRecord(partner.Name, partner.Sex, partner.BornOn.Year, personality, partner.Genome, partner.TraitIds));
            }
            return parents;
        }

        private static void AddFamily(Character heir, LifeSession deceased, Relationship link, Npc self, Npc partner)
        {
            Character parent = deceased.Character;
            GameDate born = heir.Birth.Week;
            heir.Relationships.Add(id => new Npc(id, parent.Name, parent.Gender.Sex, parent.Birth.Week, parent.Genome, parent.Traits.Instances.Select(t => t.TraitId).ToList(), parent.Death.Week)
                .WithAvatarFace(AvatarFaces.Optional(AvatarFaces.Of(parent))),
                RelationshipType.Parent, link.Value, born);
            if (partner != null)
            {
                heir.Relationships.Add(id => Copy(id, partner, parent), RelationshipType.Parent, link.Value, born);
            }
            foreach (Relationship sibling in parent.Relationships.All.Where(r => r.Type == RelationshipType.Child && r.NpcId != self.Id))
            {
                Npc npc = parent.Relationships.Npc(sibling.NpcId);
                if (npc != null)
                {
                    heir.Relationships.Add(id => Copy(id, npc, parent), RelationshipType.Sibling, sibling.Value, born);
                }
            }
        }

        private static Npc Copy(int id, Npc npc, Character owner) =>
            new Npc(id, npc.Name, npc.Sex, npc.BornOn, npc.Genome, npc.TraitIds, npc.DiedOn).WithAvatarFace(AvatarFaces.Optional(AvatarFaces.Of(owner, npc)));

        /// <summary>
        /// Credits the automatic schooling an heir of this age would have finished (a grown child arrives with the
        /// usual compulsory education).
        /// </summary>
        private void CreditSchooling(Character heir, GameDate now)
        {
            int age = heir.AgeAt(now);
            var credentials = new List<Credential>();
            int highest = 0;
            foreach (EducationProgramDefinition program in _programs.All.Where(p => p.AutoEnroll).OrderBy(p => p.Rank))
            {
                int finishAge = program.MinEntryAge + program.Years;
                if (highest < program.RequiresRank || age < finishAge || program.Institutions.All.Count == 0)
                {
                    continue;
                }
                string institution = program.Institutions.All.OrderBy(i => i.TuitionPerYear).First().Id;
                credentials.Add(new Credential(program.Id, institution, heir.Birth.Week.AddWeeks(finishAge * GameCalendar.WeeksPerYear), _settings.HeirEducationPerformance));
                highest = Math.Max(highest, program.Rank);
            }
            heir.Education.Restore(null, credentials, new KeyValuePair<string, GameDate>[0]);
        }

        private InheritanceReport Inherit(Character heir, LifeSession deceased, int heirs, GameDate now)
        {
            Character parent = deceased.Character;
            decimal assets = parent.Wallet.Cash + parent.Finances.Portfolio.TotalValue;
            InheritanceReport report = InheritanceReport.Settle(assets, parent.Finances.Debts.TotalBalance, heirs, _settings);
            if (report.Received > 0m)
            {
                heir.Wallet.Deposit(report.Received);
            }
            foreach (KeyValuePair<string, float> reputation in parent.Reputation.Values)
            {
                if (_catalogs.ReputationDomains.Contains(reputation.Key))
                {
                    heir.Reputation.Set(reputation.Key, reputation.Value * _settings.ReputationShare);
                }
            }
            heir.Karma.AddGood(parent.Karma.Good * _settings.KarmaShare);
            heir.Karma.AddBad(parent.Karma.Bad * _settings.KarmaShare);
            foreach (FlagEntry flag in parent.Flags.Entries)
            {
                if (_catalogs.Flags.TryGet(flag.FlagId, out FlagDefinition definition) && definition.Inheritable)
                {
                    if (definition.Kind == FlagKind.Counter)
                    {
                        heir.Flags.Increment(flag.FlagId, now, flag.Value);
                    }
                    else
                    {
                        heir.Flags.Set(flag.FlagId, now);
                    }
                }
            }
            return report;
        }

        private static CalendarDate ToCalendarDate(GameDate week)
        {
            int year = week.Year;
            int remaining = week.DayOfYear;
            int month = 1;
            while (month < GameCalendar.MonthsPerYear && remaining >= CalendarDate.DaysInMonth(year, month))
            {
                remaining -= CalendarDate.DaysInMonth(year, month);
                month++;
            }
            return new CalendarDate(year, month, Math.Min(remaining + 1, CalendarDate.DaysInMonth(year, month)));
        }
    }
}
