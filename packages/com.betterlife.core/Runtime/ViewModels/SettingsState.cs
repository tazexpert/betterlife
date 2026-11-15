using System.Collections.Generic;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// The settings screen.
    /// </summary>
    public sealed class SettingsState
    {
        /// <summary>
        /// Creates the state.
        /// </summary>
        /// <param name="languages">Languages (Id = code, Label = the language's own name, Amount unused, IsAvailable = not the current one).</param>
        /// <param name="viceContent">Whether vice content is on.</param>
        /// <param name="viceToggleText">Button text that switches vice content.</param>
        /// <param name="info">Undo left today, achievements, content version.</param>
        public SettingsState(IReadOnlyList<AmountChoice> languages, bool viceContent, string viceToggleText, IReadOnlyList<ValueRow> info)
        {
            Languages = languages;
            ViceContent = viceContent;
            ViceToggleText = viceToggleText;
            Info = info;
        }

        /// <summary>Languages.</summary>
        public IReadOnlyList<AmountChoice> Languages { get; }

        /// <summary>Whether vice content is on.</summary>
        public bool ViceContent { get; }

        /// <summary>Button text that switches vice content.</summary>
        public string ViceToggleText { get; }

        /// <summary>Undo left today, achievements, content version.</summary>
        public IReadOnlyList<ValueRow> Info { get; }
    }
}
