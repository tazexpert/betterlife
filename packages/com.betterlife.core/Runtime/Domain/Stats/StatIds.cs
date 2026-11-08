namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// Ids of stats that game code refers to directly. Their definitions and formulas live in content/stats.json.
    /// </summary>
    public static class StatIds
    {
        /// <summary>Core: overall physical health (derived).</summary>
        public static readonly StatId Health = new StatId("health");
        /// <summary>Core: overall mental state (derived).</summary>
        public static readonly StatId Mental = new StatId("mental");
        /// <summary>Core: overall intelligence (derived).</summary>
        public static readonly StatId Intelligence = new StatId("intelligence");
        /// <summary>Core: overall wealth (stored until the Economy task derives it).</summary>
        public static readonly StatId Wealth = new StatId("wealth");
        /// <summary>Core: overall relationships (stored until the Relationships task derives it).</summary>
        public static readonly StatId Relationships = new StatId("relationships");

        /// <summary>Sub-stat of Health.</summary>
        public static readonly StatId Fitness = new StatId("health.fitness");
        /// <summary>Sub-stat of Health.</summary>
        public static readonly StatId Nutrition = new StatId("health.nutrition");
        /// <summary>Sub-stat of Health (lower is better).</summary>
        public static readonly StatId Fatigue = new StatId("health.fatigue");
        /// <summary>Sub-stat of Health (lower is better).</summary>
        public static readonly StatId DiseaseBurden = new StatId("health.disease_burden");

        /// <summary>Sub-stat of Mental.</summary>
        public static readonly StatId Happiness = new StatId("mental.happiness");
        /// <summary>Sub-stat of Mental (lower is better).</summary>
        public static readonly StatId Stress = new StatId("mental.stress");
        /// <summary>Sub-stat of Mental.</summary>
        public static readonly StatId Confidence = new StatId("mental.confidence");
        /// <summary>Sub-stat of Mental (lower is better).</summary>
        public static readonly StatId Loneliness = new StatId("mental.loneliness");

        /// <summary>Sub-stat of Intelligence.</summary>
        public static readonly StatId Iq = new StatId("intelligence.iq");
        /// <summary>Sub-stat of Intelligence.</summary>
        public static readonly StatId Knowledge = new StatId("intelligence.knowledge");

        /// <summary>Sub-stat of Wealth, stored 0-100 and displayed as 300-900.</summary>
        public static readonly StatId CreditScore = new StatId("wealth.credit_score");

        /// <summary>Hidden stat.</summary>
        public static readonly StatId Charisma = new StatId("hidden.charisma");
        /// <summary>Hidden stat.</summary>
        public static readonly StatId Looks = new StatId("hidden.looks");
        /// <summary>Hidden stat.</summary>
        public static readonly StatId Willpower = new StatId("hidden.willpower");
    }
}
