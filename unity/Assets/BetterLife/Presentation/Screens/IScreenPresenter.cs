using System;
using BetterLife.Core.Navigation;
using UnityEngine.UIElements;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// A screen: owns its visual tree and fills it from the game state.
    /// </summary>
    public interface IScreenPresenter : IDisposable
    {
        /// <summary>Root element added to the shell's screen container or modal layer.</summary>
        VisualElement Root { get; }

        /// <summary>
        /// Called when the screen becomes visible (the argument may differ between calls).
        /// </summary>
        /// <param name="request">Request that opened the screen.</param>
        void Show(ScreenRequest request);

        /// <summary>
        /// Re-reads the game state (after time passes or a decision is made).
        /// </summary>
        void Refresh();
    }
}
