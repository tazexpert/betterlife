using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;
using UnityEngine.UIElements;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Avatar picker: a grid of the faces of the player's sex at their current age. Tapping a face chooses it and goes back.
    /// </summary>
    public sealed class AvatarPickerScreenPresenter : ListScreenPresenter
    {
        /// <summary>Grid class.</summary>
        public const string GridClass = "avatar-grid";
        /// <summary>Class of the chosen face.</summary>
        public const string SelectedClass = "avatar--selected";

        /// <summary>
        /// Creates the picker.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public AvatarPickerScreenPresenter(ScreenContext context)
            : base(context, AvatarPickerViewModel.TitleKey)
        {
        }

        /// <inheritdoc />
        protected override void Fill(LifeSession session)
        {
            Content.Add(UiRows.Text(Context.Localizer.Get(AvatarPickerViewModel.HintKey), "list-screen__subtitle"));
            var grid = new VisualElement();
            grid.AddToClassList(GridClass);
            foreach (AvatarOption option in AvatarPickerViewModel.Build(session))
            {
                VisualElement face = AvatarImage.Create(option.Path, AvatarImage.MediumClass);
                face.EnableInClassList(SelectedClass, option.IsSelected);
                int faceId = option.FaceId;
                face.AddManipulator(new Clickable(() => Choose(faceId)));
                grid.Add(face);
            }
            Content.Add(grid);
        }

        private void Choose(int faceId)
        {
            LifeSession session = Context.Session;
            if (session != null && AvatarPickerViewModel.Choose(session, faceId))
            {
                Context.NotifyChanged();
                Context.Router.Back();
            }
        }
    }
}
