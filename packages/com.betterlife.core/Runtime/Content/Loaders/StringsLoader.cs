using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Localization;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/strings/*.json (one file per language) into <see cref="StringTables"/>.
    /// </summary>
    public sealed class StringsLoader : IContentTypeLoader
    {
        private static readonly string[] NoDependencies = new string[0];

        /// <inheritdoc />
        public string TypeName => ContentTypes.Strings;

        /// <inheritdoc />
        public IReadOnlyList<string> DependsOn => NoDependencies;

        /// <inheritdoc />
        public bool AcceptsFiles => true;

        /// <inheritdoc />
        public void Load(IReadOnlyList<ContentDocument> documents, ContentLoadContext context)
        {
            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents));
            }
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            var tables = new List<StringTable>();
            foreach (ContentDocument document in documents)
            {
                RuleReadContext root = context.ReadContext();
                context.Guard(document, root.Path, () =>
                {
                    RuleJson.RequireOnly(document.Root, root, "language", "strings", ContentReading.SchemaProperty);
                    string language = RuleJson.RequireId(document.Root, "language", root);
                    if (tables.Any(t => t.Language == language))
                    {
                        throw root.Property("language").Error($"language '{language}' has more than one file.");
                    }
                    var (token, stringsContext) = RuleJson.RequireProperty(document.Root, "strings", root);
                    JObject strings = RuleJson.RequireObject(token, stringsContext);
                    var texts = new Dictionary<string, string>(StringComparer.Ordinal);
                    foreach (JProperty property in strings.Properties())
                    {
                        if (property.Value.Type != JTokenType.String)
                        {
                            throw stringsContext.Property(property.Name).Error("text must be a string.");
                        }
                        texts[property.Name] = (string)property.Value;
                    }
                    tables.Add(new StringTable(language, texts));
                });
            }
            context.Guard(documents.FirstOrDefault(), RuleReadContext.RootPath, () => context.Add(new StringTables(tables)));
        }
    }
}
