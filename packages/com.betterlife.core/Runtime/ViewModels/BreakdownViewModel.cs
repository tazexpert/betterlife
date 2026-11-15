using System;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Localization;
using BetterLife.Core.Sessions;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Builds the stat breakdown: the formula of a derived stat, or the recent changes of a stored stat by source.
    /// </summary>
    public static class BreakdownViewModel
    {
        /// <summary>Heading of a formula breakdown.</summary>
        public const string FormulaKey = "breakdown.formula";
        /// <summary>Heading of a recent changes breakdown (placeholder <c>{weeks}</c>).</summary>
        public const string RecentKey = "breakdown.recent";
        /// <summary>Text when nothing changed recently.</summary>
        public const string EmptyKey = "breakdown.empty";
        /// <summary>Detail of a formula line (placeholders <c>{input}</c>, <c>{weight}</c>).</summary>
        public const string TermKey = "breakdown.term";
        /// <summary>Format of signed contributions.</summary>
        public const string ContributionFormat = "+0.0;-0.0;0.0";
        /// <summary>Format of inputs and weights.</summary>
        public const string NumberFormat = "0.##";

        /// <summary>
        /// Builds the breakdown of a stat.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="statId">Stat id (must exist in the catalog).</param>
        /// <param name="localizer">Localizer.</param>
        /// <returns>The state.</returns>
        public static BreakdownState Build(LifeSession session, StatId statId, ILocalizer localizer)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (localizer == null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }
            Character character = session.Character;
            StatDefinition definition = character.Catalogs.Stats.GetDefinition(statId);
            StatBreakdown breakdown = character.Explain(statId, session.TimeState.Clock.Now);
            string heading = breakdown.Kind == StatBreakdownKind.Formula
                ? localizer.Get(FormulaKey)
                : localizer.Format(RecentKey, ("weeks", character.ChangeLog.RetentionWeeks.ToString(CultureInfo.InvariantCulture)));
            var lines = breakdown.Lines
                .OrderByDescending(l => Math.Abs(l.Contribution))
                .Select(l => new BreakdownLineState(localizer.Get(l.LabelKey), Detail(l, localizer), l.Contribution,
                    l.Contribution.ToString(ContributionFormat, CultureInfo.InvariantCulture)))
                .ToList();
            return new BreakdownState(localizer.Get(definition.NameKey), breakdown.Value.ToString(DashboardViewModel.ValueFormat, CultureInfo.InvariantCulture),
                heading, lines, localizer.Get(EmptyKey));
        }

        private static string Detail(StatBreakdownLine line, ILocalizer localizer)
        {
            if (!line.InputValue.HasValue || !line.Weight.HasValue)
            {
                return string.Empty;
            }
            return localizer.Format(TermKey,
                ("input", line.InputValue.Value.ToString(NumberFormat, CultureInfo.InvariantCulture)),
                ("weight", line.Weight.Value.ToString(NumberFormat, CultureInfo.InvariantCulture)));
        }
    }
}
