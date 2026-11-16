using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Navigation;
using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;
using UnityEngine.UIElements;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Character creation: name, sex, date and time of birth, country and province, family wealth, parents' parenting
    /// styles and face. Starts from a random character; every field can be changed. The form keeps its values in a
    /// <see cref="CreationDraft"/> and is rebuilt from it when the sex or country changes.
    /// </summary>
    public sealed class CreationScreenPresenter : ListScreenPresenter
    {
        /// <summary>Class of a form field block.</summary>
        public const string FieldClass = "creation-field";
        /// <summary>Class of a selected choice button.</summary>
        public const string SelectedClass = "choice--selected";

        private CreationDraft _draft;
        private CreationFormViewModel _form;

        /// <summary>
        /// Creates the screen.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public CreationScreenPresenter(ScreenContext context)
            : base(context, CreationFormViewModel.TitleKey)
        {
        }

        /// <inheritdoc />
        protected override void Fill(LifeSession session)
        {
            ILifeController lives = Context.Lives;
            _form = _form ?? new CreationFormViewModel(lives.Content, Context.Localizer);
            _draft = _draft ?? lives.NewDraft();

            Content.Add(UiRows.Action(Context.Localizer.Get(CreationFormViewModel.RandomKey), () =>
            {
                _draft = lives.NewDraft();
                Refresh();
            }));

            Content.Add(Text(CreationFormViewModel.GivenNameKey, _draft.GivenName, v => _draft.GivenName = v));
            Content.Add(Text(CreationFormViewModel.FamilyNameKey, _draft.FamilyName, v => _draft.FamilyName = v));
            Content.Add(SexButtons());

            Content.Add(UiRows.Section(Context.Localizer.Get(CreationFormViewModel.BirthDateKey)));
            var date = new VisualElement();
            date.AddToClassList("creation-date");
            date.Add(Dropdown(null, Enumerable.Range(1, 31).Select(d => new ChoiceItem(d.ToString(CultureInfo.InvariantCulture), d.ToString(CultureInfo.InvariantCulture))).ToList(),
                _draft.Day.ToString(CultureInfo.InvariantCulture), id => _draft.Day = int.Parse(id, CultureInfo.InvariantCulture)));
            date.Add(Dropdown(null, _form.Months, _draft.Month.ToString(CultureInfo.InvariantCulture), id => _draft.Month = int.Parse(id, CultureInfo.InvariantCulture)));
            date.Add(Dropdown(null, _form.Years.Select(y => new ChoiceItem(y.ToString(CultureInfo.InvariantCulture), y.ToString(CultureInfo.InvariantCulture))).ToList(),
                _draft.Year.ToString(CultureInfo.InvariantCulture), id => _draft.Year = int.Parse(id, CultureInfo.InvariantCulture)));
            Content.Add(date);
            Content.Add(Dropdown(CreationFormViewModel.BirthHourKey, _form.Hours, _draft.Hour.ToString(CultureInfo.InvariantCulture),
                id => _draft.Hour = int.Parse(id, CultureInfo.InvariantCulture)));

            Content.Add(Dropdown(CreationFormViewModel.CountryKey, _form.Countries, _draft.CountryId, id =>
            {
                _draft.CountryId = id;
                _draft.CityId = _form.Cities(id).FirstOrDefault()?.Id;
                Refresh();
            }));
            Content.Add(Dropdown(CreationFormViewModel.CityKey, _form.Cities(_draft.CountryId), _draft.CityId, id => _draft.CityId = id));
            Content.Add(Dropdown(CreationFormViewModel.FamilyKey, _form.Families, _draft.FamilyBackgroundId, id => _draft.FamilyBackgroundId = id));
            Content.Add(Dropdown(CreationFormViewModel.MotherKey, _form.Personalities, _draft.MotherPersonalityId, id => _draft.MotherPersonalityId = id));
            Content.Add(Dropdown(CreationFormViewModel.FatherKey, _form.Personalities, _draft.FatherPersonalityId, id => _draft.FatherPersonalityId = id));

            Content.Add(UiRows.Section(Context.Localizer.Get(CreationFormViewModel.FaceKey)));
            Content.Add(Faces());

            Content.Add(UiRows.Action(Context.Localizer.Get(CreationFormViewModel.StartKey), Start));
        }

        private void Start()
        {
            IReadOnlyList<string> problems = _form.Problems(_draft);
            if (problems.Count > 0)
            {
                ShowResult(new ActionResultState(Context.Localizer.Get(CreationFormViewModel.TitleKey), false, problems));
                return;
            }
            CreationDraft draft = _draft;
            _draft = null;
            Context.Lives.StartNewLife(draft);
        }

        private VisualElement SexButtons()
        {
            var block = new VisualElement();
            block.AddToClassList(FieldClass);
            block.Add(UiRows.Section(Context.Localizer.Get(CreationFormViewModel.SexKey)));
            var row = new VisualElement();
            row.AddToClassList("choice-row");
            foreach (ChoiceItem sex in _form.Sexes)
            {
                var value = (BiologicalSex)Enum.Parse(typeof(BiologicalSex), sex.Id);
                var button = new Button(() =>
                {
                    if (_draft.Sex != value)
                    {
                        _draft.Sex = value;
                        _draft.AvatarFace = null;
                        Refresh();
                    }
                }) { text = sex.Label };
                button.AddToClassList("choice");
                button.EnableInClassList(SelectedClass, _draft.Sex == value);
                row.Add(button);
            }
            block.Add(row);
            return block;
        }

        private VisualElement Faces()
        {
            var grid = new VisualElement();
            grid.AddToClassList(AvatarPickerScreenPresenter.GridClass);
            IReadOnlyList<AvatarOption> faces = _form.Faces(_draft.Sex, _draft.AvatarFace);
            if (_draft.AvatarFace == null && faces.Count > 0)
            {
                _draft.AvatarFace = faces[0].FaceId;
                faces = _form.Faces(_draft.Sex, _draft.AvatarFace);
            }
            foreach (AvatarOption option in faces)
            {
                VisualElement face = AvatarImage.Create(option.Path, AvatarImage.SmallClass);
                face.EnableInClassList(AvatarPickerScreenPresenter.SelectedClass, option.IsSelected);
                int id = option.FaceId;
                face.AddManipulator(new Clickable(() =>
                {
                    _draft.AvatarFace = id;
                    Refresh();
                }));
                grid.Add(face);
            }
            return grid;
        }

        private VisualElement Text(string labelKey, string value, Action<string> onChange)
        {
            var field = new TextField(Context.Localizer.Get(labelKey)) { value = value ?? string.Empty, maxLength = PersonName.MaxPartLength };
            field.AddToClassList(FieldClass);
            field.RegisterValueChangedCallback(e => onChange(e.newValue));
            return field;
        }

        private VisualElement Dropdown(string labelKey, IReadOnlyList<ChoiceItem> items, string selectedId, Action<string> onChange)
        {
            List<string> labels = items.Select(i => i.Label).ToList();
            int index = Math.Max(0, items.ToList().FindIndex(i => i.Id == selectedId));
            var field = new DropdownField(labelKey == null ? null : Context.Localizer.Get(labelKey), labels, items.Count == 0 ? -1 : index);
            field.AddToClassList(FieldClass);
            field.RegisterValueChangedCallback(e =>
            {
                int chosen = labels.IndexOf(e.newValue);
                if (chosen >= 0)
                {
                    onChange(items[chosen].Id);
                }
            });
            return field;
        }
    }
}
