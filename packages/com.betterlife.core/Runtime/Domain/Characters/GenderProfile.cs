using System;

namespace BetterLife.Core.Domain.Characters
{
    /// <summary>
    /// Biological sex, gender identity and speech style. Identity and speech style are player choices.
    /// </summary>
    public sealed class GenderProfile
    {
        /// <summary>
        /// Creates a profile.
        /// </summary>
        /// <param name="sex">Sex assigned at birth.</param>
        /// <param name="identity">Gender identity.</param>
        /// <param name="speechStyle">Speech style used for gendered text.</param>
        public GenderProfile(BiologicalSex sex, GenderIdentity identity, SpeechStyle speechStyle)
        {
            Sex = sex;
            Identity = identity;
            SpeechStyle = speechStyle;
        }

        /// <summary>Sex assigned at birth.</summary>
        public BiologicalSex Sex { get; }

        /// <summary>Gender identity.</summary>
        public GenderIdentity Identity { get; }

        /// <summary>Speech style used for gendered text.</summary>
        public SpeechStyle SpeechStyle { get; }

        /// <summary>
        /// Creates a profile whose identity matches the sex and whose speech style follows the identity.
        /// </summary>
        /// <param name="sex">Sex assigned at birth.</param>
        /// <returns>The default profile.</returns>
        public static GenderProfile DefaultFor(BiologicalSex sex)
        {
            GenderIdentity identity = sex == BiologicalSex.Female ? GenderIdentity.Woman : GenderIdentity.Man;
            return new GenderProfile(sex, identity, DefaultSpeechStyle(identity));
        }

        /// <summary>
        /// Returns the speech style suggested for an identity.
        /// </summary>
        /// <param name="identity">Gender identity.</param>
        /// <returns>Suggested speech style.</returns>
        public static SpeechStyle DefaultSpeechStyle(GenderIdentity identity)
        {
            switch (identity)
            {
                case GenderIdentity.Woman:
                    return SpeechStyle.Feminine;
                case GenderIdentity.Man:
                    return SpeechStyle.Masculine;
                case GenderIdentity.NonBinary:
                    return SpeechStyle.Neutral;
                default:
                    throw new ArgumentOutOfRangeException(nameof(identity), identity, "Unknown gender identity.");
            }
        }

        /// <summary>
        /// Returns a copy with a new identity. The speech style follows the new identity.
        /// </summary>
        /// <param name="identity">New identity.</param>
        /// <returns>The updated profile.</returns>
        public GenderProfile WithIdentity(GenderIdentity identity)
        {
            return new GenderProfile(Sex, identity, DefaultSpeechStyle(identity));
        }

        /// <summary>
        /// Returns a copy with a different speech style.
        /// </summary>
        /// <param name="speechStyle">New speech style.</param>
        /// <returns>The updated profile.</returns>
        public GenderProfile WithSpeechStyle(SpeechStyle speechStyle)
        {
            return new GenderProfile(Sex, Identity, speechStyle);
        }
    }
}
