using System;
using System.Collections.Generic;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Helpers for single-file settings documents.
    /// </summary>
    public static class SettingsReading
    {
        /// <summary>
        /// Reads a required id that must exist.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the object.</param>
        /// <param name="exists">True for known ids.</param>
        /// <param name="what">What the id names, for the error.</param>
        /// <returns>The id.</returns>
        public static string RequireKnown(JObject obj, string name, RuleReadContext context, Func<string, bool> exists, string what)
        {
            if (exists == null)
            {
                throw new ArgumentNullException(nameof(exists));
            }
            string id = RuleJson.RequireId(obj, name, context);
            if (!exists(id))
            {
                throw context.Property(name).Error($"unknown {what} '{id}'.");
            }
            return id;
        }

        /// <summary>
        /// Fails when a stat the code reads is missing from the stat catalog.
        /// </summary>
        /// <param name="stats">Stat catalog.</param>
        /// <param name="required">Stats the system reads.</param>
        /// <param name="context">Read context for the error.</param>
        /// <param name="system">System name for the error.</param>
        public static void RequireStats(StatCatalog stats, IEnumerable<StatId> required, RuleReadContext context, string system)
        {
            if (stats == null || required == null || context == null)
            {
                throw new ArgumentNullException(nameof(stats));
            }
            foreach (StatId stat in required)
            {
                if (!stats.Contains(stat))
                {
                    throw context.Error($"{system} needs the stat '{stat}'.");
                }
            }
        }

        /// <summary>
        /// Reads a required object property with only the allowed keys.
        /// </summary>
        /// <param name="parent">Parent object.</param>
        /// <param name="name">Property name.</param>
        /// <param name="context">Read context of the parent.</param>
        /// <param name="allowed">Allowed keys.</param>
        /// <returns>The object and its read context.</returns>
        public static (JObject Section, RuleReadContext Context) Section(JObject parent, string name, RuleReadContext context, params string[] allowed)
        {
            var (token, sectionContext) = RuleJson.RequireProperty(parent, name, context);
            JObject section = RuleJson.RequireObject(token, sectionContext);
            RuleJson.RequireOnly(section, sectionContext, allowed);
            return (section, sectionContext);
        }
    }
}
