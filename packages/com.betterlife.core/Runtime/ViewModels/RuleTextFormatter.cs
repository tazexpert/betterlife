using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Karma;
using BetterLife.Core.Localization;
using BetterLife.Core.Rules;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Turns rule texts (effect previews and results) into display text: arguments named <c>...Key</c> are localized,
    /// a <c>min</c>/<c>max</c> pair becomes one <c>{value}</c> range, stat-like amounts get a sign and money is shown at
    /// today's prices.
    /// </summary>
    public sealed class RuleTextFormatter
    {
        /// <summary>Separator of a range ("5–10").</summary>
        public const string RangeSeparator = "–";

        private const string KeySuffix = "Key";
        private const string Plus = "+";

        private static readonly HashSet<string> SignedKeys = new HashSet<string>
        {
            RuleTextKeys.EffectStat, RuleTextKeys.EffectSkill, KarmaTextKeys.EffectMerit,
        };

        private static readonly HashSet<string> MoneyKeys = new HashSet<string>
        {
            RuleTextKeys.EffectCashGain, RuleTextKeys.EffectCashPay, RuleTextKeys.EffectCashShortfall,
        };

        /// <summary>
        /// Where a raw id in <c>{subject}</c> finds its display name, per rule text ("{0}" = the id). The first key
        /// that has text wins; otherwise the id is shown as it is.
        /// </summary>
        private static readonly Dictionary<string, string[]> SubjectNames = new Dictionary<string, string[]>
        {
            [RuleTextKeys.CondRelationshipCount] = new[] { "relationship.type.{0}" },
            [Relationships.MeetEffect.MetKey] = new[] { "relationship.type.{0}" },
            [Relationships.MeetEffect.ChanceKey] = new[] { "relationship.type.{0}" },
            [RuleTextKeys.CondRelationshipValue] = new[] { "relationship.type.{0}" },
            [RuleTextKeys.CondReputation] = new[] { "reputation.{0}.name" },
            [RuleTextKeys.CondHealthCondition] = new[] { "condition.{0}.name" },
            [RuleTextKeys.EffectHealthConditionAdd] = new[] { "condition.{0}.name" },
            [RuleTextKeys.EffectHealthConditionRemove] = new[] { "condition.{0}.name" },
            [Astrology.FortuneCondition.TextKey] = new[] { "fortune.area.{0}" },
            [RuleTextKeys.CondCareerCategory] = new[] { "career.category.{0}" },
            [RuleTextKeys.CondCareerTrack] = new[] { "career.{0}.name" },
            [RuleTextKeys.CondTotalDebtKind] = new[] { "debt.kind.{0}" },
            [KarmaTextKeys.CondKarma] = new[] { "karma.{0}" },
            [RuleTextKeys.CondScam] = new[] { "scam.measure.{0}" },
            [RuleTextKeys.CondVice] = new[] { "vice.{0}.name" },
            [Domain.World.LocationCondition.TextKey] = new[] { "city.{0}.name", "country.{0}.name" },
        };

        private readonly ILocalizer _localizer;
        private readonly MoneyFormatter _money;
        private readonly Func<decimal, decimal> _toNominal;

        /// <summary>
        /// Creates a formatter.
        /// </summary>
        /// <param name="localizer">Localizer.</param>
        /// <param name="money">Money formatter.</param>
        /// <param name="toNominal">Converts real (base-year) money to today's prices; null keeps amounts as they are.</param>
        public RuleTextFormatter(ILocalizer localizer, MoneyFormatter money, Func<decimal, decimal> toNominal = null)
        {
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _money = money ?? throw new ArgumentNullException(nameof(money));
            _toNominal = toNominal ?? (v => v);
        }

        /// <summary>
        /// Formats one rule text.
        /// </summary>
        /// <param name="text">Rule text.</param>
        /// <returns>Display text.</returns>
        public string Format(RuleText text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            var args = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> pair in text.Args)
            {
                args[pair.Key] = pair.Key.EndsWith(KeySuffix, StringComparison.Ordinal) ? _localizer.Get(pair.Value) : pair.Value;
            }
            if (args.TryGetValue(RuleTextKeys.ArgSubject, out string subject) && subject != null && SubjectNames.TryGetValue(text.Key, out string[] patterns))
            {
                args[RuleTextKeys.ArgSubject] = SubjectName(subject, patterns);
            }

            bool money = MoneyKeys.Contains(text.Key);
            bool signed = SignedKeys.Contains(text.Key);
            if (args.TryGetValue(RuleTextKeys.ArgValue, out string value))
            {
                args[RuleTextKeys.ArgValue] = Amount(value, money, signed);
            }
            else if (args.TryGetValue(RuleTextKeys.ArgMin, out string min) && args.TryGetValue(RuleTextKeys.ArgMax, out string max))
            {
                args[RuleTextKeys.ArgValue] = Range(min, max, money, signed);
            }
            return _localizer.Format(text.Key, args.Select(a => (a.Key, a.Value)).ToArray());
        }

        /// <summary>
        /// Formats several rule texts.
        /// </summary>
        /// <param name="texts">Rule texts (null gives none).</param>
        /// <returns>Display lines.</returns>
        public IReadOnlyList<string> FormatAll(IEnumerable<RuleText> texts)
        {
            return texts == null ? new string[0] : texts.Select(Format).ToList();
        }

        private string SubjectName(string id, string[] patterns)
        {
            foreach (string pattern in patterns)
            {
                string key = string.Format(CultureInfo.InvariantCulture, pattern, id);
                string name = _localizer.Get(key);
                if (name != key)
                {
                    return name;
                }
            }
            return id;
        }

        private string Amount(string raw, bool money, bool signed)
        {
            if (!decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal number))
            {
                return raw;
            }
            if (money)
            {
                return _money.Full(_toNominal(Math.Abs(number)));
            }
            return signed && number > 0m ? Plus + raw : raw;
        }

        private string Range(string min, string max, bool money, bool signed)
        {
            if (!decimal.TryParse(min, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal low)
                || !decimal.TryParse(max, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal high))
            {
                return min + RangeSeparator + max;
            }
            if (money)
            {
                decimal a = Math.Abs(low);
                decimal b = Math.Abs(high);
                return _money.Full(_toNominal(Math.Min(a, b))) + RangeSeparator + _money.Full(_toNominal(Math.Max(a, b)));
            }
            return Amount(min, false, signed) + RangeSeparator + Amount(max, false, signed);
        }
    }
}
