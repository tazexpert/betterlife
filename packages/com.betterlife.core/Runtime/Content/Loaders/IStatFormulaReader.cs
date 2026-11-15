using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Reads one formula "type" from stats.json. New formula kinds (for example Wealth in the Economy task)
    /// are new readers passed to <see cref="StatsLoader"/>.
    /// </summary>
    public interface IStatFormulaReader
    {
        /// <summary>The formula "type" value.</summary>
        string Type { get; }

        /// <summary>
        /// Reads a formula.
        /// </summary>
        /// <param name="json">The formula object.</param>
        /// <param name="context">Read context of the object.</param>
        /// <returns>The formula.</returns>
        IStatFormula Read(JObject json, RuleReadContext context);
    }
}
