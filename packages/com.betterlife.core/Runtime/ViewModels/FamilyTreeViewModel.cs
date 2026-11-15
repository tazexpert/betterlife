using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Legacy;
using BetterLife.Core.Life;
using BetterLife.Core.Localization;
using BetterLife.Core.Sessions;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Builds the family tree: every ancestor (oldest first) and the current character.
    /// </summary>
    public static class FamilyTreeViewModel
    {
        /// <summary>Row label (placeholders <c>{generation}</c>, <c>{name}</c>).</summary>
        public const string RowKey = "tree.row";
        /// <summary>Lifespan of an ancestor (placeholders <c>{born}</c>, <c>{died}</c>).</summary>
        public const string LifespanKey = "tree.lifespan";
        /// <summary>Lifespan of the living character (placeholder <c>{born}</c>).</summary>
        public const string AliveKey = "tree.alive";
        /// <summary>Ancestor detail (placeholders <c>{cause}</c>, <c>{age}</c>, <c>{value}</c>).</summary>
        public const string DetailKey = "tree.detail";
        /// <summary>Detail of the current character.</summary>
        public const string YouKey = "tree.you";

        /// <summary>
        /// Builds the tree.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="money">Money formatter.</param>
        /// <returns>Rows, founder first.</returns>
        public static IReadOnlyList<ValueRow> Build(LifeSession session, ILocalizer localizer, MoneyFormatter money)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (localizer == null || money == null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }
            List<ValueRow> rows = session.Lineage.Ancestors.Select(a => new ValueRow(
                Row(a.Generation, a.GivenName + " " + a.FamilyName, localizer),
                localizer.Format(LifespanKey, ("born", a.BornOn.Year.ToString(CultureInfo.InvariantCulture)), ("died", a.DiedOn.Year.ToString(CultureInfo.InvariantCulture))),
                localizer.Format(DetailKey, ("cause", CauseName(session, a.CauseId, localizer)), ("age", a.AgeYears.ToString(CultureInfo.InvariantCulture)),
                    ("value", money.Full(a.NetWorth))),
                ValueRow.ToneOf(a.NetWorth))).ToList();
            var c = session.Character;
            rows.Add(new ValueRow(Row(c.Generation, c.Name.GivenName + " " + c.Name.FamilyName, localizer),
                c.Death == null
                    ? localizer.Format(AliveKey, ("born", c.Birth.Date.Year.ToString(CultureInfo.InvariantCulture)))
                    : localizer.Format(LifespanKey, ("born", c.Birth.Date.Year.ToString(CultureInfo.InvariantCulture)), ("died", c.Death.Week.Year.ToString(CultureInfo.InvariantCulture))),
                localizer.Get(YouKey), RowTone.Good));
            return rows;
        }

        private static string Row(int generation, string name, ILocalizer localizer)
        {
            return localizer.Format(RowKey, ("generation", generation.ToString(CultureInfo.InvariantCulture)), ("name", name));
        }

        private static string CauseName(LifeSession session, string causeId, ILocalizer localizer)
        {
            return causeId != null && session.Deaths.Causes.TryGet(causeId, out DeathCauseDefinition cause) ? localizer.Get(cause.NameKey) : causeId ?? string.Empty;
        }
    }
}
