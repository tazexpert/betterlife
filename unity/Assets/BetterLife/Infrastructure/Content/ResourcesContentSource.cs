using System;
using System.IO;
using BetterLife.Core.Content;
using UnityEngine;

namespace BetterLife.Infrastructure.Content
{
    /// <summary>
    /// Reads content copied into Resources at build time. Works synchronously on every platform.
    /// </summary>
    public sealed class ResourcesContentSource : IContentSource
    {
        /// <inheritdoc />
        public string ReadText(string relativePath)
        {
            if (!ContentPaths.IsValid(relativePath))
            {
                throw new ArgumentException($"'{relativePath}' is not a valid content path.", nameof(relativePath));
            }

            // Resources.Load paths have no file extension.
            string withoutExtension = relativePath.Substring(0, relativePath.Length - ContentPaths.JsonExtension.Length);
            TextAsset asset = Resources.Load<TextAsset>(ContentLocations.ResourcesFolder + "/" + withoutExtension);
            if (asset == null)
            {
                throw new FileNotFoundException($"Content file '{relativePath}' is not in Resources/{ContentLocations.ResourcesFolder}.");
            }

            string text = asset.text;
            Resources.UnloadAsset(asset);
            return text;
        }
    }
}
