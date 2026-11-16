using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Avatars;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Family;
using BetterLife.Core.Domain.World;
using BetterLife.Core.Localization;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Option lists and checks of the character creation screen.
    /// </summary>
    public sealed class CreationFormViewModel
    {
        /// <summary>Screen title.</summary>
        public const string TitleKey = "creation.title";
        /// <summary>Field labels.</summary>
        public const string GivenNameKey = "creation.given_name";
        /// <summary>Field label.</summary>
        public const string FamilyNameKey = "creation.family_name";
        /// <summary>Field label.</summary>
        public const string SexKey = "creation.sex";
        /// <summary>Field label.</summary>
        public const string BirthDateKey = "creation.birth_date";
        /// <summary>Field label.</summary>
        public const string BirthHourKey = "creation.birth_hour";
        /// <summary>Field label.</summary>
        public const string CountryKey = "creation.country";
        /// <summary>Field label.</summary>
        public const string CityKey = "creation.city";
        /// <summary>Field label.</summary>
        public const string FamilyKey = "creation.family";
        /// <summary>Field label.</summary>
        public const string MotherKey = "creation.mother";
        /// <summary>Field label.</summary>
        public const string FatherKey = "creation.father";
        /// <summary>Field label.</summary>
        public const string FaceKey = "creation.face";
        /// <summary>Button text.</summary>
        public const string RandomKey = "creation.random";
        /// <summary>Button text.</summary>
        public const string StartKey = "creation.start";
        /// <summary>Button text.</summary>
        public const string CancelKey = "creation.cancel";
        /// <summary>Menu entry that opens the creation screen.</summary>
        public const string NewLifeKey = "creation.new_life";
        /// <summary>Birth hour text (placeholder <c>{hour}</c>).</summary>
        public const string HourFormatKey = "creation.hour_format";
        /// <summary>Family level text (placeholders <c>{level}</c>, <c>{name}</c>).</summary>
        public const string FamilyLevelKey = "creation.family_level";

        /// <summary>Age at which faces are previewed (a baby face is hard to judge).</summary>
        public const int FacePreviewAge = 20;

        private const string MonthPrefix = "month.m";
        private const string SexPrefix = "sex.";
        private const string HourFormat = "00";
        private const string ThaiLeadingVowels = "เแโใไ";

        private readonly IContentRepository _content;
        private readonly ILocalizer _localizer;
        private readonly CharacterCatalogs _catalogs;
        private readonly CreationSettings _settings;

        /// <summary>
        /// Creates the view model.
        /// </summary>
        /// <param name="content">Loaded content.</param>
        /// <param name="localizer">Localizer.</param>
        public CreationFormViewModel(IContentRepository content, ILocalizer localizer)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _catalogs = content.Get<CharacterCatalogs>();
            _settings = content.Get<CreationSettings>();
        }

        /// <summary>Sexes.</summary>
        public IReadOnlyList<ChoiceItem> Sexes => new[] { BiologicalSex.Female, BiologicalSex.Male }
            .Select(s => new ChoiceItem(s.ToString(), _localizer.Get(SexPrefix + s.ToString().ToLowerInvariant()))).ToList();

        /// <summary>Birth years, newest first.</summary>
        public IReadOnlyList<int> Years => Enumerable.Range(_settings.MinBirthYear, _settings.MaxBirthYear - _settings.MinBirthYear + 1).Reverse().ToList();

        /// <summary>Months 1-12 by name.</summary>
        public IReadOnlyList<ChoiceItem> Months => Enumerable.Range(1, 12)
            .Select(m => new ChoiceItem(m.ToString(CultureInfo.InvariantCulture), _localizer.Get(MonthPrefix + m.ToString(CultureInfo.InvariantCulture)))).ToList();

        /// <summary>Birth hours 0-23.</summary>
        public IReadOnlyList<ChoiceItem> Hours => Enumerable.Range(0, 24)
            .Select(h => new ChoiceItem(h.ToString(CultureInfo.InvariantCulture), _localizer.Format(HourFormatKey, ("hour", h.ToString(HourFormat, CultureInfo.InvariantCulture))))).ToList();

        /// <summary>Country every life is born in (the form shows its provinces only), or null to choose.</summary>
        public string HomeCountryId => _settings.HomeCountryId;

        /// <summary>Countries.</summary>
        public IReadOnlyList<ChoiceItem> Countries => _catalogs.Countries.All.Select(c => new ChoiceItem(c.Id, _localizer.Get(c.NameKey))).ToList();

        /// <summary>Family backgrounds, poorest first.</summary>
        public IReadOnlyList<ChoiceItem> Families => _catalogs.FamilyBackgrounds.All.OrderBy(b => b.HouseholdIncomeLevel)
            .Select(b => new ChoiceItem(b.Id, _localizer.Format(FamilyLevelKey, ("level", b.HouseholdIncomeLevel.ToString(CultureInfo.InvariantCulture)), ("name", _localizer.Get(b.NameKey)))))
            .ToList();

        /// <summary>Parenting styles.</summary>
        public IReadOnlyList<ChoiceItem> Personalities => _content.Get<DefinitionCatalog<ParentPersonalityDefinition>>().All
            .Select(p => new ChoiceItem(p.Id, _localizer.Get(p.NameKey))).ToList();

        /// <summary>
        /// Cities (provinces) of a country in alphabetical order; places kept only for old saves (weight 0) are left out.
        /// </summary>
        /// <param name="countryId">Country id.</param>
        /// <returns>The cities.</returns>
        public IReadOnlyList<ChoiceItem> Cities(string countryId)
        {
            CountryDefinition country = _catalogs.Countries.All.FirstOrDefault(c => c.Id == countryId);
            if (country == null)
            {
                return new ChoiceItem[0];
            }
            return country.Cities.Where(c => c.Weight > 0f)
                .Select(c => new ChoiceItem(c.Id, _localizer.Get(c.NameKey)))
                .OrderBy(c => SortKey(c.Label), StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>
        /// Faces of a sex, drawn at <see cref="FacePreviewAge"/>.
        /// </summary>
        /// <param name="sex">Sex.</param>
        /// <param name="selected">Chosen face, or null.</param>
        /// <returns>The faces (empty without an avatar set).</returns>
        public IReadOnlyList<AvatarOption> Faces(BiologicalSex sex, int? selected)
        {
            AvatarCatalog avatars = _catalogs.Avatars;
            if (avatars == null)
            {
                return new AvatarOption[0];
            }
            return avatars.FacesFor(sex).Select(f => new AvatarOption(f.Id, avatars.PathFor(f.Id, FacePreviewAge), f.Id == selected)).ToList();
        }

        /// <summary>
        /// Problems with the draft, as texts for the player (empty = ready to start).
        /// </summary>
        /// <param name="draft">Draft.</param>
        /// <returns>The problems.</returns>
        public IReadOnlyList<string> Problems(CreationDraft draft)
        {
            if (draft == null)
            {
                throw new ArgumentNullException(nameof(draft));
            }
            if (!draft.HasValidDate)
            {
                return new[] { _localizer.Get(CreationIssue.BirthDateInvalid) };
            }
            return new CharacterCreator(_content).Validate(draft.ToRequest(0UL)).Select(i => _localizer.Get(i.MessageKey)).Distinct().ToList();
        }

        /// <summary>
        /// Sort key for Thai: a leading vowel (เ แ โ ใ ไ) is written first but sorted after its consonant.
        /// </summary>
        private static string SortKey(string text)
        {
            if (text.Length > 1 && ThaiLeadingVowels.IndexOf(text[0]) >= 0)
            {
                return text[1].ToString() + text[0] + text.Substring(2);
            }
            return text;
        }
    }
}
