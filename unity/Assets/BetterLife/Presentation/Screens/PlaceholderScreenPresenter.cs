using BetterLife.Core.Localization;
using BetterLife.Core.Navigation;
using BetterLife.Presentation.Shell;
using UnityEngine.UIElements;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Stand-in for screens that are not built yet: shows the tab name and a "coming soon" note.
    /// </summary>
    public sealed class PlaceholderScreenPresenter : IScreenPresenter
    {
        /// <summary>Title key prefix (screen ids of tab roots match the nav label keys).</summary>
        public const string TitleKeyPrefix = "nav.";

        private readonly ILocalizer _localizer;
        private readonly Label _title;

        /// <summary>
        /// Creates the placeholder.
        /// </summary>
        /// <param name="localizer">Localizer.</param>
        public PlaceholderScreenPresenter(ILocalizer localizer)
        {
            _localizer = localizer;
            Root = UiAssets.Layout(UiAssets.Placeholder).Instantiate();
            Root.style.flexGrow = 1;
            _title = Root.Q<Label>("title");
            UiText.Localize(Root, localizer);
        }

        /// <inheritdoc />
        public VisualElement Root { get; }

        /// <inheritdoc />
        public void Show(ScreenRequest request)
        {
            _title.text = _localizer.Get(TitleKeyPrefix + request.ScreenId);
        }

        /// <inheritdoc />
        public void Refresh()
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
