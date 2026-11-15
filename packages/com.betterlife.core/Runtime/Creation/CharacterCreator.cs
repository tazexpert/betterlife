using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Family;
using BetterLife.Core.Domain.Genetics;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Domain.World;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time;

namespace BetterLife.Core.Creation
{
    /// <summary>
    /// Creates a new life from a seed. Each aspect (birth, identity, location, family, upbringing, genetics, names)
    /// draws from its own random stream and always draws the same amount, so overriding one aspect in Custom mode
    /// never changes the others, and a Challenge seed always produces the same life.
    /// </summary>
    public sealed class CharacterCreator
    {
        private const int HoursPerDay = 24;
        private const float HalfChance = 0.5f;
        private const string StreamPrefix = "creation.";

        private readonly CharacterCatalogs _catalogs;
        private readonly DefinitionCatalog<ParentPersonalityDefinition> _personalities;
        private readonly DefinitionCatalog<NameList> _names;
        private readonly GeneticsService _genetics;
        private readonly CreationSettings _settings;

        /// <summary>
        /// Creates the creator.
        /// </summary>
        /// <param name="content">Loaded content.</param>
        public CharacterCreator(IContentRepository content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            _catalogs = content.Get<CharacterCatalogs>();
            _personalities = content.Get<DefinitionCatalog<ParentPersonalityDefinition>>();
            _names = content.Get<DefinitionCatalog<NameList>>();
            _genetics = content.Get<GeneticsService>();
            _settings = content.Get<CreationSettings>();
        }

        /// <summary>
        /// Lists every problem with a request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Problems; empty when valid.</returns>
        public IReadOnlyList<CreationIssue> Validate(CreationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var issues = new List<CreationIssue>();
            if (request.Mode != CreationMode.Custom && request.HasOverrides)
            {
                issues.Add(new CreationIssue(nameof(request.Mode), CreationIssue.OverridesNotAllowed));
            }
            if (request.BirthDate.HasValue && (request.BirthDate.Value.Year < _settings.MinBirthYear || request.BirthDate.Value.Year > _settings.MaxBirthYear))
            {
                issues.Add(new CreationIssue(nameof(request.BirthDate), CreationIssue.BirthYearOutOfRange));
            }
            if (request.BirthHour.HasValue && (request.BirthHour.Value < 0 || request.BirthHour.Value >= HoursPerDay))
            {
                issues.Add(new CreationIssue(nameof(request.BirthHour), CreationIssue.BirthHourInvalid));
            }
            if (request.CountryId != null && !_catalogs.Countries.Contains(request.CountryId))
            {
                issues.Add(new CreationIssue(nameof(request.CountryId), CreationIssue.UnknownId));
            }
            if (request.CityId != null)
            {
                CountryDefinition owner = _catalogs.Countries.All.FirstOrDefault(c => c.FindCity(request.CityId) != null);
                if (owner == null)
                {
                    issues.Add(new CreationIssue(nameof(request.CityId), CreationIssue.UnknownId));
                }
                else if (request.CountryId != null && owner.Id != request.CountryId)
                {
                    issues.Add(new CreationIssue(nameof(request.CityId), CreationIssue.CityNotInCountry));
                }
            }
            CheckId(issues, nameof(request.FamilyBackgroundId), request.FamilyBackgroundId, _catalogs.FamilyBackgrounds.Contains);
            CheckId(issues, nameof(request.MotherPersonalityId), request.MotherPersonalityId, _personalities.Contains);
            CheckId(issues, nameof(request.FatherPersonalityId), request.FatherPersonalityId, _personalities.Contains);
            CheckName(issues, nameof(request.GivenName), request.GivenName);
            CheckName(issues, nameof(request.FamilyName), request.FamilyName);
            return issues;
        }

        /// <summary>
        /// Creates the life. Throws when the request has issues.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The new life.</returns>
        public CreationResult Create(CreationRequest request)
        {
            IReadOnlyList<CreationIssue> issues = Validate(request);
            if (issues.Count > 0)
            {
                throw new InvalidOperationException("Invalid creation request: " + string.Join(", ", issues.Select(i => $"{i.Field} ({i.MessageKey})")));
            }

            var random = new RandomRegistry(request.Seed);
            CalendarDate birthDate = RollBirthDate(request, random.GetStream(StreamPrefix + "birth"), out int birthHour);
            (BiologicalSex sex, GenderProfile gender) = RollIdentity(request, random.GetStream(StreamPrefix + "identity"));
            (CountryDefinition country, CityDefinition city) = RollLocation(request, random.GetStream(StreamPrefix + "location"));
            IRandomProvider familyRandom = random.GetStream(StreamPrefix + "family");
            FamilyBackgroundDefinition background = Pick(familyRandom, _catalogs.FamilyBackgrounds.All, b => b.Weight, request.FamilyBackgroundId, _catalogs.FamilyBackgrounds);
            ParentPersonalityDefinition motherPersonality = Pick(familyRandom, _personalities.All, p => p.Weight, request.MotherPersonalityId, _personalities);
            ParentPersonalityDefinition fatherPersonality = Pick(familyRandom, _personalities.All, p => p.Weight, request.FatherPersonalityId, _personalities);
            int motherAge = familyRandom.NextInt(_settings.MinParentAge, _settings.MaxParentAge + 1);
            int fatherAge = familyRandom.NextInt(_settings.MinParentAge, _settings.MaxParentAge + 1);

            IRandomProvider geneticsRandom = random.GetStream(StreamPrefix + "genetics");
            Genome motherGenes = _genetics.RollFounder(geneticsRandom);
            Genome fatherGenes = _genetics.RollFounder(geneticsRandom);
            IReadOnlyList<string> motherTraits = _genetics.RollFounderTraits(geneticsRandom);
            IReadOnlyList<string> fatherTraits = _genetics.RollFounderTraits(geneticsRandom);
            Genome childGenes = _genetics.Inherit(new[] { motherGenes, fatherGenes }, geneticsRandom);
            IReadOnlyList<string> inheritedTraits = _genetics.InheritTraits(new[] { motherTraits, fatherTraits }, geneticsRandom);

            IRandomProvider nameRandom = random.GetStream(StreamPrefix + "names");
            NameList names = _names.TryGet(country.Id, out NameList list) ? list : _names.All.First();
            string familyName = PickName(nameRandom, names.Family, request.NameScript);
            string childGiven = PickName(nameRandom, names.GivenNames(sex), request.NameScript);
            string motherGiven = PickName(nameRandom, names.Female, request.NameScript);
            string fatherGiven = PickName(nameRandom, names.Male, request.NameScript);
            familyName = request.FamilyName ?? familyName;

            var character = new Character(
                CharacterId.Create(random.GetStream(StreamPrefix + "id")),
                new PersonName(request.GivenName ?? childGiven, familyName),
                gender,
                new BirthInfo(birthDate, birthHour, country.Id),
                _catalogs);
            character.MoveTo(new Residence(country.Id, city.Id));
            character.SetFamily(new FamilyInfo(background.Id, new[]
            {
                new ParentRecord(new PersonName(motherGiven, familyName), BiologicalSex.Female, birthDate.Year - motherAge, motherPersonality.Id, motherGenes, motherTraits),
                new ParentRecord(new PersonName(fatherGiven, familyName), BiologicalSex.Male, birthDate.Year - fatherAge, fatherPersonality.Id, fatherGenes, fatherTraits),
            }));

            ApplyGenes(character, childGenes);
            GameDate birthWeek = character.Birth.Week;
            IRandomProvider upbringing = random.GetStream(StreamPrefix + "upbringing");
            foreach (string trait in inheritedTraits.Concat(RollPersonalityTraits(upbringing, motherPersonality, fatherPersonality)))
            {
                character.Traits.Add(trait, birthWeek);
            }
            ApplyAmounts(character, background.StatModifiers, background.NameKey, birthWeek);
            ApplyAmounts(character, motherPersonality.StatModifiers, motherPersonality.NameKey, birthWeek);
            ApplyAmounts(character, fatherPersonality.StatModifiers, fatherPersonality.NameKey, birthWeek);
            return new CreationResult(character, random, request.Mode);
        }

        private CalendarDate RollBirthDate(CreationRequest request, IRandomProvider random, out int hour)
        {
            int year = random.NextInt(_settings.MinBirthYear, _settings.MaxBirthYear + 1);
            int daysInYear = CalendarDate.IsLeapYear(year) ? GameCalendar.DaysPerCommonYear + 1 : GameCalendar.DaysPerCommonYear;
            int dayOfYear = random.NextInt(0, daysInYear);
            int rolledHour = random.NextInt(0, HoursPerDay);
            hour = request.BirthHour ?? rolledHour;
            return request.BirthDate ?? DateFromDayOfYear(year, dayOfYear);
        }

        private static (BiologicalSex, GenderProfile) RollIdentity(CreationRequest request, IRandomProvider random)
        {
            BiologicalSex rolled = random.Chance(HalfChance) ? BiologicalSex.Female : BiologicalSex.Male;
            BiologicalSex sex = request.Sex ?? rolled;
            GenderProfile profile = GenderProfile.DefaultFor(sex);
            if (request.GenderIdentity.HasValue)
            {
                profile = profile.WithIdentity(request.GenderIdentity.Value);
            }
            if (request.SpeechStyle.HasValue)
            {
                profile = profile.WithSpeechStyle(request.SpeechStyle.Value);
            }
            return (sex, profile);
        }

        private (CountryDefinition, CityDefinition) RollLocation(CreationRequest request, IRandomProvider random)
        {
            CountryDefinition rolledCountry = random.PickWeighted(_catalogs.Countries.All, c => c.Weight) ?? _catalogs.Countries.All.First();
            CountryDefinition country = request.CountryId != null
                ? _catalogs.Countries.Get(request.CountryId)
                : request.CityId != null ? _catalogs.Countries.All.First(c => c.FindCity(request.CityId) != null) : rolledCountry;
            CityDefinition rolledCity = random.PickWeighted(country.Cities, c => c.Weight) ?? country.Cities.First();
            CityDefinition city = request.CityId != null ? country.FindCity(request.CityId) : rolledCity;
            return (country, city);
        }

        private IEnumerable<string> RollPersonalityTraits(IRandomProvider random, params ParentPersonalityDefinition[] parents)
        {
            // Every chance is rolled so the stream length does not depend on the outcome.
            return parents.SelectMany(p => p.TraitChances).Select(t => (t.TraitId, Hit: random.Chance(t.Chance))).Where(t => t.Hit).Select(t => t.TraitId).ToList();
        }

        private void ApplyGenes(Character character, Genome genes)
        {
            foreach (KeyValuePair<StatId, float> gene in genes.Values)
            {
                character.Genome.Set(gene.Key, gene.Value);
                character.Stats.SetStored(gene.Key, gene.Value);
            }
        }

        private static void ApplyAmounts(Character character, IEnumerable<StatAmount> amounts, string sourceKey, GameDate week)
        {
            foreach (StatAmount amount in amounts)
            {
                character.ApplyStatChange(new StatChange(amount.StatId, amount.Amount, sourceKey), week);
            }
        }

        private static T Pick<T>(IRandomProvider random, IReadOnlyList<T> items, Func<T, float> weight, string overrideId, DefinitionCatalog<T> catalog)
            where T : class, IDefinition
        {
            T rolled = random.PickWeighted(items, weight) ?? items.First();
            return overrideId != null ? catalog.Get(overrideId) : rolled;
        }

        private static string PickName(IRandomProvider random, IReadOnlyList<NameEntry> pool, NameScript script)
        {
            return pool[random.NextInt(0, pool.Count)].In(script);
        }

        private static CalendarDate DateFromDayOfYear(int year, int dayOfYear)
        {
            int month = 1;
            int remaining = dayOfYear;
            while (remaining >= CalendarDate.DaysInMonth(year, month))
            {
                remaining -= CalendarDate.DaysInMonth(year, month);
                month++;
            }
            return new CalendarDate(year, month, remaining + 1);
        }

        private static void CheckId(List<CreationIssue> issues, string field, string id, Func<string, bool> exists)
        {
            if (id != null && !exists(id))
            {
                issues.Add(new CreationIssue(field, CreationIssue.UnknownId));
            }
        }

        private static void CheckName(List<CreationIssue> issues, string field, string name)
        {
            if (name == null)
            {
                return;
            }
            string trimmed = name.Trim();
            if (trimmed.Length == 0 || trimmed.Length > PersonName.MaxPartLength)
            {
                issues.Add(new CreationIssue(field, CreationIssue.NameInvalid));
            }
        }
    }
}
