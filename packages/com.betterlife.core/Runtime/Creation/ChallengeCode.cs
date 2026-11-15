using System;
using System.Text;

namespace BetterLife.Core.Creation
{
    /// <summary>
    /// Shareable form of a 64-bit life seed: 13 Crockford Base32 characters grouped as XXXX-XXXX-XXXXX.
    /// Decoding ignores case, spaces and dashes, and reads I/L as 1 and O as 0.
    /// </summary>
    public static class ChallengeCode
    {
        private const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
        private const int Length = 13;
        private const int BitsPerChar = 5;
        private const int CharMask = 31;
        private const int FirstCharLimit = 16;
        private const int FirstGroup = 4;
        private const int SecondGroup = 8;

        /// <summary>
        /// Encodes a seed.
        /// </summary>
        /// <param name="seed">Life seed.</param>
        /// <returns>Code such as "7ZK2-M9QD-1F0XA".</returns>
        public static string Encode(ulong seed)
        {
            var chars = new char[Length];
            ulong rest = seed;
            for (int i = Length - 1; i >= 0; i--)
            {
                chars[i] = Alphabet[(int)(rest & CharMask)];
                rest >>= BitsPerChar;
            }
            var text = new string(chars);
            return text.Substring(0, FirstGroup) + "-" + text.Substring(FirstGroup, SecondGroup - FirstGroup) + "-" + text.Substring(SecondGroup);
        }

        /// <summary>
        /// Decodes a code typed or pasted by a player.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="seed">The seed when valid.</param>
        /// <returns>True when the code is valid.</returns>
        public static bool TryDecode(string code, out ulong seed)
        {
            seed = 0UL;
            if (string.IsNullOrEmpty(code))
            {
                return false;
            }

            var normalized = new StringBuilder();
            foreach (char raw in code)
            {
                if (raw == '-' || char.IsWhiteSpace(raw))
                {
                    continue;
                }
                char c = char.ToUpperInvariant(raw);
                normalized.Append(c == 'I' || c == 'L' ? '1' : c == 'O' ? '0' : c);
            }
            if (normalized.Length != Length)
            {
                return false;
            }

            ulong value = 0UL;
            for (int i = 0; i < Length; i++)
            {
                int digit = Alphabet.IndexOf(normalized[i]);
                if (digit < 0 || (i == 0 && digit >= FirstCharLimit))
                {
                    return false;
                }
                value = (value << BitsPerChar) | (uint)digit;
            }
            seed = value;
            return true;
        }
    }
}
