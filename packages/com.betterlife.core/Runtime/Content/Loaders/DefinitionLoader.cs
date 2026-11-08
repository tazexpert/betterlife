using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Base loader for content shaped as { "&lt;items&gt;": [ { "id": ... }, ... ] } that becomes a
    /// <see cref="DefinitionCatalog{T}"/>. Handles iteration, per-item errors and duplicate ids across files.
    /// </summary>
    /// <typeparam name="T">Definition type.</typeparam>
    public abstract class DefinitionLoader<T> : IContentTypeLoader where T : class, IDefinition
    {
        private static readonly string[] NoDependencies = new string[0];

        /// <inheritdoc />
        public abstract string TypeName { get; }

        /// <inheritdoc />
        public virtual IReadOnlyList<string> DependsOn => NoDependencies;

        /// <inheritdoc />
        public bool AcceptsFiles => true;

        /// <summary>Name of the root array property, for example "traits".</summary>
        protected abstract string ItemsProperty { get; }

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

            var definitions = new List<T>();
            var firstSeenIn = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (ContentDocument document in documents)
            {
                RuleReadContext root = context.ReadContext();
                JArray items = null;
                context.Guard(document, root.Path, () =>
                {
                    RuleJson.RequireOnly(document.Root, root, ItemsProperty, ContentReading.SchemaProperty);
                    items = RuleJson.RequireArrayProperty(document.Root, ItemsProperty, root).Array;
                });
                if (items == null)
                {
                    continue;
                }

                RuleReadContext arrayContext = root.Property(ItemsProperty);
                for (int i = 0; i < items.Count; i++)
                {
                    RuleReadContext itemContext = arrayContext.Index(i);
                    JToken token = items[i];
                    context.Guard(document, itemContext.Path, () =>
                    {
                        T definition = ReadItem(RuleJson.RequireObject(token, itemContext), itemContext, context);
                        if (firstSeenIn.TryGetValue(definition.Id, out string otherFile))
                        {
                            throw itemContext.Property("id").Error($"duplicate id '{definition.Id}' (first defined in {otherFile}).");
                        }
                        firstSeenIn.Add(definition.Id, document.Path);
                        definitions.Add(definition);
                    });
                }
            }
            context.Add(new DefinitionCatalog<T>(definitions));
        }

        /// <summary>
        /// Reads one definition. Throw <see cref="RuleParseException"/> (via the read context) or
        /// <see cref="ArgumentException"/> for invalid data.
        /// </summary>
        /// <param name="item">The item object.</param>
        /// <param name="context">Read context of the item.</param>
        /// <param name="content">Load context, for earlier products.</param>
        /// <returns>The definition.</returns>
        protected abstract T ReadItem(JObject item, RuleReadContext context, ContentLoadContext content);
    }
}
