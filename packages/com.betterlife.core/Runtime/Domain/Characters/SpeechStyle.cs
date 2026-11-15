namespace BetterLife.Core.Domain.Characters
{
    /// <summary>
    /// Selects gendered localization variants (Thai polite particles and pronouns).
    /// </summary>
    public enum SpeechStyle
    {
        /// <summary>Masculine forms, for example "krub" and "phom".</summary>
        Masculine = 0,
        /// <summary>Feminine forms, for example "ka" and "chan".</summary>
        Feminine = 1,
        /// <summary>Neutral forms that avoid gendered particles where possible.</summary>
        Neutral = 2,
    }
}
