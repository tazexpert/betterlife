using BetterLife.Core.Localization;
using UnityEngine.UIElements;

namespace BetterLife.Presentation.Shell
{
    /// <summary>
    /// Replaces UXML texts written as <c>@loc.key</c> with localized text. The key is kept in <c>userData</c>
    /// so the tree can be localized again after a language change.
    /// </summary>
    public static class UiText
    {
        /// <summary>Prefix that marks a text as a localization key.</summary>
        public const string KeyPrefix = "@";

        /// <summary>
        /// Localizes every label and button under a root.
        /// </summary>
        /// <param name="root">Root element (included).</param>
        /// <param name="localizer">Localizer.</param>
        public static void Localize(VisualElement root, ILocalizer localizer)
        {
            if (root == null || localizer == null)
            {
                return;
            }
            root.Query<TextElement>().ForEach(element =>
            {
                string key = element.userData as string;
                if (key == null && element.text != null && element.text.StartsWith(KeyPrefix))
                {
                    key = element.text.Substring(KeyPrefix.Length);
                    element.userData = key;
                }
                if (key != null)
                {
                    element.text = localizer.Get(key);
                }
            });
        }
    }
}
