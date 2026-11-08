using System.Collections.Generic;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Takes no files. Every loader that parses rules depends on this barrier, so all rule types registered by
    /// content (fortune, karma.bad, karma.merit, ...) exist before any rule is parsed. Loaders that register rule
    /// types are listed here.
    /// </summary>
    public sealed class RuleTypesBarrier : IContentTypeLoader
    {
        private static readonly string[] Dependencies =
        {
            ContentTypes.CharacterCatalogs, ContentTypes.Astrology, ContentTypes.LuckSettings, ContentTypes.EducationPrograms,
        };

        /// <inheritdoc />
        public string TypeName => ContentTypes.RuleTypes;

        /// <inheritdoc />
        public IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        public bool AcceptsFiles => false;

        /// <inheritdoc />
        public void Load(IReadOnlyList<ContentDocument> documents, ContentLoadContext context)
        {
        }
    }
}
