using System.Collections.Generic;

namespace BetterLife.Core.Saving
{
    /// <summary>
    /// Where save text lives (files, memory, or a cloud service later).
    /// </summary>
    public interface ISaveStorage
    {
        /// <summary>
        /// Returns true when a slot holds data.
        /// </summary>
        /// <param name="slot">Slot name.</param>
        /// <returns>True when present.</returns>
        bool Exists(string slot);

        /// <summary>
        /// Reads a slot.
        /// </summary>
        /// <param name="slot">Slot name.</param>
        /// <returns>The text, or null when empty.</returns>
        string Read(string slot);

        /// <summary>
        /// Writes a slot, replacing it completely (never leaves a half-written slot).
        /// </summary>
        /// <param name="slot">Slot name.</param>
        /// <param name="text">Text.</param>
        void Write(string slot, string text);

        /// <summary>
        /// Deletes a slot.
        /// </summary>
        /// <param name="slot">Slot name.</param>
        /// <returns>True when something was deleted.</returns>
        bool Delete(string slot);

        /// <summary>
        /// Lists the slots that hold data.
        /// </summary>
        /// <returns>Slot names.</returns>
        IReadOnlyList<string> Slots();
    }
}
