using System.Collections.Generic;

namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// Strategy that computes a derived stat from other stats. New formula types are added as new classes.
    /// </summary>
    public interface IStatFormula
    {
        /// <summary>The derived stat.</summary>
        StatId Target { get; }

        /// <summary>Stats the formula reads.</summary>
        IReadOnlyList<StatId> Inputs { get; }

        /// <summary>
        /// Computes the derived value.
        /// </summary>
        /// <param name="stats">Source of input values.</param>
        /// <returns>Value from 0 to 100.</returns>
        float Evaluate(IStatReader stats);

        /// <summary>
        /// Explains the derived value line by line.
        /// </summary>
        /// <param name="stats">Source of input values.</param>
        /// <param name="catalog">Catalog used to resolve input labels.</param>
        /// <returns>The breakdown.</returns>
        StatBreakdown Explain(IStatReader stats, StatCatalog catalog);
    }
}
