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
        /// <summary>Astrology tables (exactly one file).</summary>
        public const string Astrology = "astrology";
        /// <summary>Chance and karma settings (exactly one file).</summary>
        public const string LuckSettings = "luck_settings";
        /// <summary>Barrier reached when every content-registered rule type is available (takes no files).</summary>
        public const string RuleTypes = "rule_types";
        /// <summary>Life events (any number of files).</summary>
        public const string Events = "events";
        /// <summary>Event system settings (exactly one file).</summary>
        public const string EventSettings = "event_settings";
        /// <summary>Countries and cities.</summary>
        public const string Countries = "countries";
        /// <summary>Family backgrounds.</summary>
        public const string FamilyBackgrounds = "family_backgrounds";
        /// <summary>Parent personalities.</summary>
        public const string ParentPersonalities = "parent_personalities";
        /// <summary>Genetics settings (exactly one file).</summary>
        public const string Genetics = "genetics";
        /// <summary>Random name pools.</summary>
        public const string Names = "names";
        /// <summary>Character creation settings (exactly one file).</summary>
        public const string CreationSettings = "creation_settings";
        /// <summary>Life stages.</summary>
        public const string LifeStages = "life_stages";
        /// <summary>Mortality settings (exactly one file).</summary>
        public const string Mortality = "mortality";
        /// <summary>Death causes.</summary>
        public const string DeathCauses = "death_causes";
        /// <summary>Macro economy settings and business-cycle phases (single file).</summary>
        public const string Economy = "economy";
        /// <summary>Investable assets.</summary>
        public const string Assets = "assets";
        /// <summary>Loan products.</summary>
        public const string DebtProducts = "debt_products";
        /// <summary>Lifestyle tiers (monthly living costs).</summary>
        public const string LifestyleTiers = "lifestyle_tiers";
        /// <summary>Personal finance settings (single file).</summary>
        public const string FinanceSettings = "finance_settings";
        /// <summary>Education programs and institutions.</summary>
        public const string EducationPrograms = "education_programs";
        /// <summary>Education rules (single file).</summary>
        public const string EducationSettings = "education_settings";
        /// <summary>Career tracks.</summary>
        public const string Careers = "careers";
        /// <summary>Career rules (single file).</summary>
        public const string CareerSettings = "career_settings";
        /// <summary>Combined character catalogs (built from the types above, takes no files).</summary>
        public const string CharacterCatalogs = "character_catalogs";
    }
}
