using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// Derived stat = sum of weighted inputs. Weights must sum to 1 so the result stays within 0-100.
    /// </summary>
    public sealed class WeightedStatFormula : IStatFormula
    {
        /// <summary>Allowed deviation of the weight sum from 1.</summary>
        public const float WeightSumTolerance = 0.001f;

        private readonly List<StatFormulaTerm> _terms;

        /// <summary>
        /// Creates a weighted formula.
        /// </summary>
        /// <param name="target">Derived stat.</param>
        /// <param name="terms">Inputs with weights summing to 1; each input at most once.</param>
        public WeightedStatFormula(StatId target, IEnumerable<StatFormulaTerm> terms)
        {
            if (terms == null)
            {
                throw new ArgumentNullException(nameof(terms));
            }

            _terms = terms.ToList();
            if (_terms.Count == 0 || _terms.Any(t => t == null))
            {
                throw new ArgumentException("Formula needs at least one non-null term.", nameof(terms));
            }
            if (_terms.Select(t => t.Source).Distinct().Count() != _terms.Count)
            {
                throw new ArgumentException("Each input stat may appear only once.", nameof(terms));
            }
            if (_terms.Any(t => t.Source == target))
            {
                throw new ArgumentException("A formula cannot read its own target.", nameof(terms));
            }

            float sum = _terms.Sum(t => t.Weight);
            if (Math.Abs(sum - 1f) > WeightSumTolerance)
            {
                throw new ArgumentException($"Weights must sum to 1 but sum to {sum}.", nameof(terms));
            }

            Target = target;
            Inputs = _terms.Select(t => t.Source).ToList();
        }

        /// <inheritdoc />
        public StatId Target { get; }

        /// <inheritdoc />
        public IReadOnlyList<StatId> Inputs { get; }

        /// <summary>The weighted terms.</summary>
        public IReadOnlyList<StatFormulaTerm> Terms => _terms;

        /// <inheritdoc />
        public float Evaluate(IStatReader stats)
        {
            if (stats == null)
            {
                throw new ArgumentNullException(nameof(stats));
            }
            return StatBounds.Clamp(_terms.Sum(t => t.Contribution(stats.GetValue(t.Source))));
        }

        /// <inheritdoc />
        public StatBreakdown Explain(IStatReader stats, StatCatalog catalog)
        {
            if (stats == null)
            {
                throw new ArgumentNullException(nameof(stats));
            }
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            var lines = _terms
                .Select(t =>
                {
                    float input = stats.GetValue(t.Source);
                    return new StatBreakdownLine(catalog.GetDefinition(t.Source).NameKey, t.Contribution(input), t.Source, input, t.Weight);
                })
                .ToList();
            return new StatBreakdown(Target, Evaluate(stats), StatBreakdownKind.Formula, lines);
        }
    }
}
