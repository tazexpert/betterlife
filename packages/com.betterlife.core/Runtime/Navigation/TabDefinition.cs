using System;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Navigation
{
    /// <summary>
    /// One bottom navigation tab.
    /// </summary>
    public sealed class TabDefinition
    {
        /// <summary>
        /// Creates the tab.
        /// </summary>
        /// <param name="id">Tab id.</param>
        /// <param name="labelKey">Localization key of the label.</param>
        /// <param name="iconClass">USS class of the icon.</param>
        /// <param name="rootScreenId">First screen of the tab.</param>
        public TabDefinition(string id, string labelKey, string iconClass, string rootScreenId)
        {
            Id = ContentId.Require(id, nameof(id));
            LabelKey = ContentId.RequireLocKey(labelKey, nameof(labelKey));
            IconClass = iconClass ?? throw new ArgumentNullException(nameof(iconClass));
            RootScreenId = ContentId.Require(rootScreenId, nameof(rootScreenId));
        }

        /// <summary>Tab id.</summary>
        public string Id { get; }

        /// <summary>Localization key of the label.</summary>
        public string LabelKey { get; }

        /// <summary>USS class of the icon.</summary>
        public string IconClass { get; }

        /// <summary>First screen of the tab.</summary>
        public string RootScreenId { get; }
    }
}
