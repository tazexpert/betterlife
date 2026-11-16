using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Avatar pictures loaded from Resources (paths come from the Core view models) and shown as element backgrounds.
    /// Loaded textures are cached; a missing picture hides the element instead of failing.
    /// </summary>
    public static class AvatarImage
    {
        /// <summary>Base class of every avatar.</summary>
        public const string AvatarClass = "avatar";
        /// <summary>Small avatar in list rows.</summary>
        public const string SmallClass = "avatar--small";
        /// <summary>Medium avatar on the home screen.</summary>
        public const string MediumClass = "avatar--medium";
        /// <summary>Large avatar on a person's page.</summary>
        public const string LargeClass = "avatar--large";

        private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();

        /// <summary>
        /// Creates an avatar element.
        /// </summary>
        /// <param name="path">Resources path without extension, or null.</param>
        /// <param name="sizeClass">Size class (small, medium or large).</param>
        /// <returns>The element.</returns>
        public static VisualElement Create(string path, string sizeClass)
        {
            var element = new VisualElement();
            element.AddToClassList(AvatarClass);
            element.AddToClassList(sizeClass);
            Set(element, path);
            return element;
        }

        /// <summary>
        /// Shows a picture on an element, or hides the element when there is none.
        /// </summary>
        /// <param name="element">Avatar element.</param>
        /// <param name="path">Resources path without extension, or null.</param>
        public static void Set(VisualElement element, string path)
        {
            Texture2D texture = Load(path);
            element.style.display = texture == null ? DisplayStyle.None : DisplayStyle.Flex;
            if (texture != null)
            {
                element.style.backgroundImage = new StyleBackground(texture);
            }
        }

        private static Texture2D Load(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            if (!Cache.TryGetValue(path, out Texture2D texture))
            {
                texture = Resources.Load<Texture2D>(path);
                if (texture == null)
                {
                    Debug.LogWarning("[BetterLife] Avatar picture not found in Resources: " + path);
                }
                Cache[path] = texture;
            }
            return texture;
        }
    }
}
