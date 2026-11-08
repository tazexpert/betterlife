namespace BetterLife.Core.Content
{
    /// <summary>
    /// Where content files come from (a folder, Unity Resources, memory). Paths are relative and use '/'.
    /// </summary>
    public interface IContentSource
    {
        /// <summary>
        /// Reads a whole file as text.
        /// </summary>
        /// <param name="relativePath">Path relative to the content root, for example "events/career_001.json".</param>
        /// <returns>The file text.</returns>
        /// <exception cref="System.IO.FileNotFoundException">When the file does not exist.</exception>
        string ReadText(string relativePath);
    }
}
