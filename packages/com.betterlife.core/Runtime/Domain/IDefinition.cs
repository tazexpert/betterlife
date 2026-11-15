namespace BetterLife.Core.Domain
{
    /// <summary>
    /// A content definition identified by a unique content id.
    /// </summary>
    public interface IDefinition
    {
        /// <summary>The unique content id.</summary>
        string Id { get; }
    }
}
