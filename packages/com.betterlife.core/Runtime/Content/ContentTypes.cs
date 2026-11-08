namespace BetterLife.Core.Content
{
    /// <summary>
    /// Content type names used in the manifest. Each has a schema at docs/schemas/&lt;type&gt;.schema.json.
    /// </summary>
    public static class ContentTypes
    {
        /// <summary>Stat definitions and formulas.</summary>
        public const string Stats = "stats";
        /// <summary>Trait definitions.</summary>
        public const string Traits = "traits";
        /// <summary>Skill definitions.</summary>
        public const string Skills = "skills";
        /// <summary>Story flag declarations.</summary>
        public const string Flags = "flags";
        /// <summary>Reputation domains.</summary>
        public const string ReputationDomains = "reputation_domains";
        /// <summary>Health condition types.</summary>
        public const string HealthConditions = "health_conditions";
        /// <summary>Time budget activities.</summary>
        public const string Activities = "activities";
        /// <summary>Global time settings (exactly one file).</summary>
        public const string TimeSettings = "time_settings";
        /// <summary>Combined character catalogs (built from the types above, takes no files).</summary>
        public const string CharacterCatalogs = "character_catalogs";
    }
}
