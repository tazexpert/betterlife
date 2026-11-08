using BetterLife.Core.Domain;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Life
{
    /// <summary>
    /// A cause of death, loaded from content/death_causes.json. Natural deaths pick a cause by weight.
    /// </summary>
    public sealed class DeathCauseDefinition : IDefinition
    {
        /// <summary>
        /// Creates a cause.
        /// </summary>
        /// <param name="id">Cause id.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="eulogyKey">Localization key used in the eulogy, or null.</param>
        /// <param name="condition">Who the cause can apply to naturally, or null for anyone.</param>
        /// <param name="weight">Weight among eligible causes.</param>
        public DeathCauseDefinition(string id, string nameKey, string eulogyKey, ICondition condition, EventWeight weight)
        {
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            EulogyKey = eulogyKey == null ? null : ContentId.RequireLocKey(eulogyKey, nameof(eulogyKey));
            Condition = condition;
            Weight = weight ?? throw new System.ArgumentNullException(nameof(weight));
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Eulogy localization key, or null.</summary>
        public string EulogyKey { get; }

        /// <summary>Eligibility for natural death, or null.</summary>
        public ICondition Condition { get; }

        /// <summary>Weight among eligible causes.</summary>
        public EventWeight Weight { get; }
    }
}
