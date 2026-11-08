using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json;

namespace BetterLife.Core.Content
{
    /// <summary>
    /// Loads all content: reads the manifest, parses every file, runs loaders in dependency order and
    /// collects every error. Loaders whose dependencies failed are skipped with an explanatory error.
    /// </summary>
    public sealed class ContentPipeline
    {
        private readonly IContentSource _source;
        private readonly RuleRegistry _rules;
        private readonly List<IContentTypeLoader> _orderedLoaders;

        /// <summary>
        /// Creates a pipeline. Throws when loader names repeat, a dependency is unknown, or dependencies form a cycle.
        /// </summary>
        /// <param name="source">Where files are read from.</param>
        /// <param name="rules">Rule registry passed to loaders.</param>
        /// <param name="loaders">One loader per content type.</param>
        public ContentPipeline(IContentSource source, RuleRegistry rules, IEnumerable<IContentTypeLoader> loaders)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            if (loaders == null)
            {
                throw new ArgumentNullException(nameof(loaders));
            }
            _orderedLoaders = SortByDependencies(loaders.ToList());
        }

        /// <summary>Loaders in the order they run.</summary>
        public IReadOnlyList<IContentTypeLoader> Loaders => _orderedLoaders;

        /// <summary>
        /// Loads everything.
        /// </summary>
        /// <returns>The repository or all errors.</returns>
        public ContentLoadResult Load()
        {
            var errors = new List<ContentError>();
            ContentManifest manifest = ReadManifest(errors);
            if (manifest == null)
            {
                return new ContentLoadResult(null, errors);
            }

            Dictionary<string, List<ContentDocument>> documents = ReadDocuments(manifest, errors);
            var products = new Dictionary<Type, object>();
            var context = new ContentLoadContext(products, errors, _rules);
            var failed = new HashSet<string>(StringComparer.Ordinal);

            foreach (IContentTypeLoader loader in _orderedLoaders)
            {
                string blocker = loader.DependsOn.FirstOrDefault(failed.Contains);
                if (blocker != null)
                {
                    failed.Add(loader.TypeName);
                    errors.Add(new ContentError(string.Empty, string.Empty, $"Skipped '{loader.TypeName}' because '{blocker}' has errors."));
                    continue;
                }

                List<ContentDocument> ofType = documents.TryGetValue(loader.TypeName, out List<ContentDocument> list) ? list : new List<ContentDocument>();
                int before = errors.Count;
                loader.Load(ofType, context);
                if (errors.Count > before || ofType.Count < CountListed(manifest, loader.TypeName))
                {
                    failed.Add(loader.TypeName);
                }
            }

            IContentRepository repository = errors.Count == 0 ? new ContentRepository(manifest.ContentVersion, products) : null;
            return new ContentLoadResult(repository, errors);
        }

        private ContentManifest ReadManifest(List<ContentError> errors)
        {
            string text;
            try
            {
                text = _source.ReadText(ContentPaths.ManifestFile);
            }
            catch (FileNotFoundException)
            {
                errors.Add(new ContentError(ContentPaths.ManifestFile, string.Empty, "Manifest not found."));
                return null;
            }

            ContentManifest manifest;
            try
            {
                manifest = ContentManifest.Read(ContentJson.ParseObject(text), errors);
            }
            catch (JsonException ex)
            {
                errors.Add(new ContentError(ContentPaths.ManifestFile, string.Empty, "Invalid JSON: " + ex.Message));
                return null;
            }
            if (manifest == null)
            {
                return null;
            }

            var byName = _orderedLoaders.ToDictionary(l => l.TypeName, StringComparer.Ordinal);
            for (int i = 0; i < manifest.Files.Count; i++)
            {
                string type = manifest.Files[i].Type;
                string path = $"$.files[{i}].type";
                if (!byName.TryGetValue(type, out IContentTypeLoader loader))
                {
                    errors.Add(new ContentError(ContentPaths.ManifestFile, path, $"Unknown content type '{type}'."));
                }
                else if (!loader.AcceptsFiles)
                {
                    errors.Add(new ContentError(ContentPaths.ManifestFile, path, $"Content type '{type}' does not take files."));
                }
            }
            return errors.Count == 0 ? manifest : null;
        }

        private Dictionary<string, List<ContentDocument>> ReadDocuments(ContentManifest manifest, List<ContentError> errors)
        {
            var documents = new Dictionary<string, List<ContentDocument>>(StringComparer.Ordinal);
            foreach (ContentFileEntry entry in manifest.Files)
            {
                try
                {
                    var document = new ContentDocument(entry.Path, ContentJson.ParseObject(_source.ReadText(entry.Path)));
                    if (!documents.TryGetValue(entry.Type, out List<ContentDocument> list))
                    {
                        list = new List<ContentDocument>();
                        documents.Add(entry.Type, list);
                    }
                    list.Add(document);
                }
                catch (FileNotFoundException)
                {
                    errors.Add(new ContentError(entry.Path, string.Empty, "File listed in the manifest was not found."));
                }
                catch (JsonException ex)
                {
                    errors.Add(new ContentError(entry.Path, string.Empty, "Invalid JSON: " + ex.Message));
                }
            }
            return documents;
        }

        private static int CountListed(ContentManifest manifest, string type)
        {
            return manifest.Files.Count(f => f.Type == type);
        }

        private static List<IContentTypeLoader> SortByDependencies(List<IContentTypeLoader> loaders)
        {
            if (loaders.Any(l => l == null))
            {
                throw new ArgumentException("Loaders must not contain null.", nameof(loaders));
            }

            var byName = new Dictionary<string, IContentTypeLoader>(StringComparer.Ordinal);
            foreach (IContentTypeLoader loader in loaders)
            {
                if (byName.ContainsKey(loader.TypeName))
                {
                    throw new ArgumentException($"Two loaders handle '{loader.TypeName}'.", nameof(loaders));
                }
                byName.Add(loader.TypeName, loader);
            }

            var ordered = new List<IContentTypeLoader>();
            var state = new Dictionary<string, bool>(StringComparer.Ordinal);
            foreach (IContentTypeLoader loader in loaders)
            {
                Visit(loader, byName, state, ordered);
            }
            return ordered;
        }

        // state: false = visiting (on the current path), true = done.
        private static void Visit(IContentTypeLoader loader, Dictionary<string, IContentTypeLoader> byName, Dictionary<string, bool> state, List<IContentTypeLoader> ordered)
        {
            if (state.TryGetValue(loader.TypeName, out bool done))
            {
                if (!done)
                {
                    throw new ArgumentException($"Content loaders have a dependency cycle through '{loader.TypeName}'.");
                }
                return;
            }

            state[loader.TypeName] = false;
            foreach (string dependency in loader.DependsOn)
            {
                if (!byName.TryGetValue(dependency, out IContentTypeLoader next))
                {
                    throw new ArgumentException($"Loader '{loader.TypeName}' depends on unknown type '{dependency}'.");
                }
                Visit(next, byName, state, ordered);
            }
            state[loader.TypeName] = true;
            ordered.Add(loader);
        }
    }
}
