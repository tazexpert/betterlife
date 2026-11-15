using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content
{
    /// <summary>
    /// The list of content files and the content version, read from content/manifest.json.
    /// </summary>
    public sealed class ContentManifest
    {
        private const string VersionProperty = "contentVersion";
        private const string FilesProperty = "files";
        private const string TypeProperty = "type";
        private const string PathProperty = "path";
        private const string SchemaProperty = "$schema";
        private const int MinVersion = 1;

        /// <summary>
        /// Creates a manifest.
        /// </summary>
        /// <param name="contentVersion">Content version, at least 1.</param>
        /// <param name="files">Listed files.</param>
        public ContentManifest(int contentVersion, IEnumerable<ContentFileEntry> files)
        {
            ContentVersion = contentVersion;
            Files = (files ?? Enumerable.Empty<ContentFileEntry>()).ToList();
        }

        /// <summary>Content version.</summary>
        public int ContentVersion { get; }

        /// <summary>Listed files in order.</summary>
        public IReadOnlyList<ContentFileEntry> Files { get; }

        /// <summary>
        /// Reads and validates a manifest, collecting every problem.
        /// </summary>
        /// <param name="root">Parsed manifest object.</param>
        /// <param name="errors">Receives problems.</param>
        /// <returns>The manifest, or null when it has errors.</returns>
        public static ContentManifest Read(JObject root, List<ContentError> errors)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            int before = errors.Count;
            var context = new RuleReadContext(null);
            int version = 0;
            var files = new List<ContentFileEntry>();
            Collect(errors, () => RuleJson.RequireOnly(root, context, VersionProperty, FilesProperty, SchemaProperty));
            Collect(errors, () =>
            {
                version = RuleJson.RequireInt(root, VersionProperty, context);
                if (version < MinVersion)
                {
                    throw context.Property(VersionProperty).Error($"must be at least {MinVersion}.");
                }
            });
            Collect(errors, () =>
            {
                var (array, arrayContext) = RuleJson.RequireArrayProperty(root, FilesProperty, context);
                var seen = new HashSet<string>(StringComparer.Ordinal);
                for (int i = 0; i < array.Count; i++)
                {
                    RuleReadContext item = arrayContext.Index(i);
                    Collect(errors, () => files.Add(ReadEntry(array[i], item, seen)));
                }
            });
            return errors.Count == before ? new ContentManifest(version, files) : null;
        }

        private static ContentFileEntry ReadEntry(JToken token, RuleReadContext context, HashSet<string> seen)
        {
            JObject obj = RuleJson.RequireObject(token, context);
            RuleJson.RequireOnly(obj, context, PathProperty);
            string type = RuleJson.RequireString(obj, TypeProperty, context);
            if (!ContentId.IsValid(type))
            {
                throw context.Property(TypeProperty).Error($"'{type}' is not a valid type name.");
            }
            string path = RuleJson.RequireString(obj, PathProperty, context);
            if (!ContentPaths.IsValid(path) || path == ContentPaths.ManifestFile)
            {
                throw context.Property(PathProperty).Error($"'{path}' is not a valid content file path.");
            }
            if (!seen.Add(path))
            {
                throw context.Property(PathProperty).Error($"'{path}' is listed more than once.");
            }
            return new ContentFileEntry(type, path);
        }

        private static void Collect(List<ContentError> errors, Action action)
        {
            try
            {
                action();
            }
            catch (RuleParseException ex)
            {
                errors.Add(new ContentError(ContentPaths.ManifestFile, ex.Path, ex.Problem));
            }
        }
    }
}
