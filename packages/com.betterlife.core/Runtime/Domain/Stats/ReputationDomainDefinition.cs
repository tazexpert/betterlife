namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// A social circle in which a character can be known, loaded from content/reputation_domains.json.
    /// </summary>
    public sealed class ReputationDomainDefinition : IDefinition
    {
        /// <summary>
        /// Creates a domain definition.
        /// </summary>
        /// <param name="id">Domain id, for example "business".</param>
        /// <param name="nameKey">Localization key of the name.</param>
        public ReputationDomainDefinition(string id, string nameKey)
        {
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }
    }
}
