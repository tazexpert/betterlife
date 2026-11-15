using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace BetterLife.Presentation.Shell
{
    /// <summary>
    /// Loads UI Toolkit assets from <c>Resources/BetterLife/UI</c> so no asset has to be assigned in the Inspector.
    /// </summary>
    public static class UiAssets
    {
        /// <summary>Resources folder of all UI assets.</summary>
        public const string Folder = "BetterLife/UI/";
        /// <summary>Runtime theme (imports Unity's default theme and the BetterLife style sheets).</summary>
        public const string Theme = "BetterLifeTheme";
        /// <summary>Shell layout.</summary>
        public const string Shell = "Shell";
        /// <summary>Top bar layout.</summary>
        public const string TopBar = "TopBar";
        /// <summary>Placeholder screen layout.</summary>
        public const string Placeholder = "Placeholder";

        /// <summary>
        /// Loads a UXML layout.
        /// </summary>
        /// <param name="name">Asset name without extension.</param>
        /// <returns>The layout.</returns>
        /// <exception cref="InvalidOperationException">The asset is missing.</exception>
        public static VisualTreeAsset Layout(string name) => Require<VisualTreeAsset>(name);

        /// <summary>
        /// Loads the runtime theme style sheet.
        /// </summary>
        /// <returns>The theme.</returns>
        /// <exception cref="InvalidOperationException">The asset is missing.</exception>
        public static ThemeStyleSheet LoadTheme() => Require<ThemeStyleSheet>(Theme);

        private static T Require<T>(string name) where T : UnityEngine.Object
        {
            T asset = Resources.Load<T>(Folder + name);
            if (asset == null)
            {
                throw new InvalidOperationException("UI asset not found in Resources: " + Folder + name + " (" + typeof(T).Name + ").");
            }
            return asset;
        }
    }
}
