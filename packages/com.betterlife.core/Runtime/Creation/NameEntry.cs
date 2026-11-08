using System;

namespace BetterLife.Core.Creation
{
    /// <summary>
    /// One name in two scripts (native script, for example Thai, and Latin).
    /// </summary>
    public sealed class NameEntry
    {
        /// <summary>
        /// Creates an entry.
        /// </summary>
        /// <param name="native">Name in the country's script.</param>
        /// <param name="latin">Name in Latin script.</param>
        public NameEntry(string native, string latin)
        {
            if (string.IsNullOrWhiteSpace(native) || string.IsNullOrWhiteSpace(latin))
            {
                throw new ArgumentException("Both scripts are required.");
            }
            Native = native.Trim();
            Latin = latin.Trim();
        }

        /// <summary>Name in the country's script.</summary>
        public string Native { get; }

        /// <summary>Name in Latin script.</summary>
        public string Latin { get; }

        /// <summary>
        /// Returns the name in a script.
        /// </summary>
        /// <param name="script">Script.</param>
        /// <returns>The name.</returns>
        public string In(NameScript script) => script == NameScript.Latin ? Latin : Native;
    }
}
