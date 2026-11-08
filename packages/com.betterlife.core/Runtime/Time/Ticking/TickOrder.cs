namespace BetterLife.Core.Time.Ticking
{
    /// <summary>
    /// Standard phase order from the design: allocation, decay, delayed consequences, events, NPCs, death, records.
    /// Gaps leave room for phases added later.
    /// </summary>
    public static class TickOrder
    {
        /// <summary>Apply the time budget.</summary>
        public const int TimeAllocation = 100;
        /// <summary>Natural stat drift, skill decay, flag expiry.</summary>
        public const int NaturalDecay = 200;
        /// <summary>Scheduled consequences of earlier choices.</summary>
        public const int DelayedConsequences = 300;
        /// <summary>Random life events.</summary>
        public const int EventRoll = 400;
        /// <summary>NPC simulation.</summary>
        public const int NpcUpdate = 500;
        /// <summary>Death check.</summary>
        public const int DeathCheck = 600;
        /// <summary>History and bookkeeping after everything else.</summary>
        public const int Record = 900;
    }
}
