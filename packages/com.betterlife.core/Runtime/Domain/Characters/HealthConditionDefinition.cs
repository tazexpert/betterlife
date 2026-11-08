namespace BetterLife.Core.Domain.Characters
{
    /// <summary>
    /// An illness or injury type, loaded from content/health_conditions.json.
    /// The Life Stages task adds duration and bodily effects.
    /// </summary>
    public sealed class HealthConditionDefinition : IDefinition
    {
        /// <summary>
        /// Creates a definition.
        /// </summary>
        /// <param name="id">Condition id, for example "flu".</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="descKey">Localization key of the description.</param>
        public HealthConditionDefinition(string id, string nameKey, string descKey)
        {
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            DescKey = ContentId.RequireLocKey(descKey, nameof(descKey));
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Localization key of the description.</summary>
        public string DescKey { get; }
    }
}
