using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Base loader for content types that must have exactly one file (settings and tables).
    /// </summary>
    public abstract class SingleDocumentLoader : IContentTypeLoader
    {
        /// <inheritdoc />
        public abstract string TypeName { get; }

        /// <inheritdoc />
        public abstract IReadOnlyList<string> DependsOn { get; }

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
            if (documents.Count != 1)
            {
                context.ReportError(documents.ElementAtOrDefault(1), string.Empty, $"Exactly one '{TypeName}' file is required, found {documents.Count}.");
                return;
            }

            ContentDocument document = documents[0];
            context.Guard(document, RuleReadContext.RootPath, () => Read(document.Root, context.ReadContext(), context));
        }

        /// <summary>
        /// Reads the document and adds its products to the context.
        /// </summary>
        /// <param name="root">Root object.</param>
        /// <param name="context">Read context of the root.</param>
        /// <param name="content">Load context.</param>
        protected abstract void Read(JObject root, RuleReadContext context, ContentLoadContext content);
    }
}
