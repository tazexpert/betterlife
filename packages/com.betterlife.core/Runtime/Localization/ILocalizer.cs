using BetterLife.Core.Rules;

namespace BetterLife.Core.Localization
{
    /// <summary>
    /// Turns localization keys into player-facing text in the current language.
    /// </summary>
    public interface ILocalizer
    {
        /// <summary>Current language code (for example "en" or "th").</summary>
        string Language { get; }

        /// <summary>
        /// Returns the text of a key, or the key itself when it has no text.
        /// </summary>
        /// <param name="key">Localization key.</param>
        /// <returns>Text.</returns>
        string Get(string key);

        /// <summary>
        /// Returns the text of a key with "{name}" placeholders replaced.
        /// </summary>
        /// <param name="key">Localization key.</param>
        /// <param name="args">Placeholder values.</param>
        /// <returns>Text.</returns>
        string Format(string key, params (string Name, string Value)[] args);

        /// <summary>
        /// Returns the text of a rule description (key plus arguments).
        /// </summary>
        /// <param name="text">Rule text.</param>
        /// <returns>Text.</returns>
        string Format(RuleText text);
    }
}
