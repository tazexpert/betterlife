using System;
using System.Collections.Generic;
using BetterLife.Core.Localization;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Maps screen ids to presenter factories. Unregistered screens get a placeholder, so a new screen is added by
    /// registering one factory in the composition root.
    /// </summary>
    public sealed class ScreenRegistry
    {
        private readonly Dictionary<string, Func<IScreenPresenter>> _factories = new Dictionary<string, Func<IScreenPresenter>>();
        private readonly ILocalizer _localizer;

        /// <summary>
        /// Creates an empty registry.
        /// </summary>
        /// <param name="localizer">Localizer for placeholders.</param>
        public ScreenRegistry(ILocalizer localizer)
        {
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        /// <summary>
        /// Registers the factory of a screen.
        /// </summary>
        /// <param name="screenId">Screen id.</param>
        /// <param name="factory">Creates the presenter.</param>
        /// <exception cref="ArgumentException">The id is already registered.</exception>
        public void Register(string screenId, Func<IScreenPresenter> factory)
        {
            if (string.IsNullOrEmpty(screenId))
            {
                throw new ArgumentException("Screen id is required.", nameof(screenId));
            }
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }
            if (_factories.ContainsKey(screenId))
            {
                throw new ArgumentException("Screen already registered: " + screenId, nameof(screenId));
            }
            _factories.Add(screenId, factory);
        }

        /// <summary>
        /// Creates the presenter of a screen, or a placeholder.
        /// </summary>
        /// <param name="screenId">Screen id.</param>
        /// <returns>A new presenter.</returns>
        public IScreenPresenter Create(string screenId)
        {
            return _factories.TryGetValue(screenId, out Func<IScreenPresenter> factory)
                ? factory()
                : new PlaceholderScreenPresenter(_localizer);
        }
    }
}
