namespace BetterLife.Core.Navigation
{
    /// <summary>
    /// Ids of the shell's screens.
    /// </summary>
    public static class ScreenIds
    {
        /// <summary>Dashboard.</summary>
        public const string Home = "home";
        /// <summary>Weekly time plan.</summary>
        public const string Life = "life";
        /// <summary>Finance.</summary>
        public const string Money = "money";
        /// <summary>Relationships.</summary>
        public const string People = "people";
        /// <summary>Education and career.</summary>
        public const string Work = "work";
        /// <summary>Everything else (astrology, timeline, habits, saves, settings).</summary>
        public const string More = "more";
        /// <summary>Inbox (pushed on the Home tab).</summary>
        public const string Inbox = "inbox";
        /// <summary>Event dialog (modal; argument = pending event instance id).</summary>
        public const string EventDialog = "event";
        /// <summary>Stat breakdown (modal; argument = stat id).</summary>
        public const string Breakdown = "breakdown";
        /// <summary>One person (pushed on the People tab; argument = NPC id).</summary>
        public const string Person = "person";
        /// <summary>Astrology and karma (pushed on the More tab).</summary>
        public const string Astrology = "astrology";
        /// <summary>Life timeline (pushed on the More tab).</summary>
        public const string Timeline = "timeline";
        /// <summary>Invest and borrow (pushed on the Money tab).</summary>
        public const string Invest = "invest";
    }
}
