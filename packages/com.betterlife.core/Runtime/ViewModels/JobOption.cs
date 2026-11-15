namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// A career the character can apply to now.
    /// </summary>
    public sealed class JobOption
    {
        /// <summary>
        /// Creates an option.
        /// </summary>
        /// <param name="trackId">Career track id.</param>
        /// <param name="title">Career name.</param>
        /// <param name="detail">Hiring chance, starting salary, hours.</param>
        public JobOption(string trackId, string title, string detail)
        {
            TrackId = trackId;
            Title = title;
            Detail = detail;
        }

        /// <summary>Career track id.</summary>
        public string TrackId { get; }

        /// <summary>Career name.</summary>
        public string Title { get; }

        /// <summary>Hiring chance, starting salary, hours.</summary>
        public string Detail { get; }
    }
}
