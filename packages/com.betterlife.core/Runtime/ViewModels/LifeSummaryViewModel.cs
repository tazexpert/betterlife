using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Achievements;
using BetterLife.Core.Careers;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Flags;
using BetterLife.Core.Education;
using BetterLife.Core.Legacy;
using BetterLife.Core.Life;
using BetterLife.Core.Localization;
using BetterLife.Core.Relationships;
using BetterLife.Core.Sessions;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Builds the end-of-life summary from the life's real data: the eulogy (death cause, schooling, career, family,
    /// estate and every flag with a eulogy line), key numbers, achievements of this life and the possible heirs.
    /// </summary>
    public static class LifeSummaryViewModel
    {
        /// <summary>Subtitle (placeholders <c>{born}</c>, <c>{died}</c>, <c>{age}</c>).</summary>
        public const string SubtitleKey = "summary.subtitle";
        /// <summary>Opening sentence (placeholders <c>{name}</c>, <c>{city}</c>).</summary>
        public const string OpeningKey = "summary.opening";
        /// <summary>Education sentence (placeholder <c>{program}</c>).</summary>
        public const string EducationKey = "summary.education";
        /// <summary>Career sentence (placeholders <c>{title}</c>, <c>{track}</c>).</summary>
        public const string CareerKey = "summary.career";
        /// <summary>Marriage sentence (placeholder <c>{name}</c>).</summary>
        public const string MarriedKey = "summary.married";
        /// <summary>Children sentence (placeholder <c>{count}</c>).</summary>
        public const string ChildrenKey = "summary.children";
        /// <summary>Estate sentence (placeholder <c>{value}</c>).</summary>
        public const string EstateKey = "summary.estate";
        /// <summary>Debt sentence (placeholder <c>{value}</c>).</summary>
        public const string DebtKey = "summary.debt";
        /// <summary>Fact labels.</summary>
        public const string CauseKey = "summary.fact.cause";
        /// <summary>Net worth fact.</summary>
        public const string NetWorthKey = "summary.fact.net_worth";
        /// <summary>Generation fact (placeholder <c>{value}</c>).</summary>
        public const string GenerationKey = "summary.fact.generation";
        /// <summary>Heir button (placeholder <c>{name}</c>).</summary>
        public const string HeirKey = "summary.heir";
        /// <summary>Heir detail (placeholders <c>{age}</c>, <c>{value}</c> = closeness, <c>{inheritance}</c>).</summary>
        public const string HeirDetailKey = "summary.heir.detail";
        /// <summary>Share text heading (placeholder <c>{name}</c>).</summary>
        public const string ShareKey = "summary.share";

        /// <summary>
        /// Builds the summary.
        /// </summary>
        /// <param name="session">The finished life.</param>
        /// <param name="achievements">Tracker (achievements of this life), or null.</param>
        /// <param name="settings">Inheritance rules (expected share per heir).</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="money">Money formatter.</param>
        /// <returns>The state, or null while the character is alive.</returns>
        public static LifeSummaryState Build(LifeSession session, AchievementTracker achievements, LegacySettings settings, ILocalizer localizer, MoneyFormatter money)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (settings == null || localizer == null || money == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            Character c = session.Character;
            if (c.Death == null)
            {
                return null;
            }
            string name = c.Name.GivenName + " " + c.Name.FamilyName;
            string subtitle = localizer.Format(SubtitleKey, ("born", c.Birth.Date.Year.ToString(CultureInfo.InvariantCulture)),
                ("died", c.Death.Week.Year.ToString(CultureInfo.InvariantCulture)), ("age", c.Death.AgeYears.ToString(CultureInfo.InvariantCulture)));
            DeathCauseDefinition cause = session.Deaths.Causes.TryGet(c.Death.CauseId, out DeathCauseDefinition found) ? found : null;

            var eulogy = new List<string> { localizer.Format(OpeningKey, ("name", c.Name.GivenName), ("city", City(c, localizer))) };
            if (cause?.EulogyKey != null)
            {
                eulogy.Add(localizer.Get(cause.EulogyKey));
            }
            Credential degree = c.Education.Highest(session.Education.Programs);
            if (degree != null && session.Education.Programs.TryGet(degree.ProgramId, out EducationProgramDefinition program))
            {
                eulogy.Add(localizer.Format(EducationKey, ("program", localizer.Get(program.NameKey))));
            }
            (CareerTrackDefinition track, CareerLevelDefinition level) = BestJob(session);
            if (track != null)
            {
                eulogy.Add(localizer.Format(CareerKey, ("title", localizer.Get(level.TitleKey)), ("track", localizer.Get(track.NameKey))));
            }
            Relationship spouse = c.Relationships.All.FirstOrDefault(r => r.Type == RelationshipType.Spouse);
            Npc partner = spouse == null ? null : c.Relationships.Npc(spouse.NpcId);
            if (partner != null)
            {
                eulogy.Add(localizer.Format(MarriedKey, ("name", partner.Name.GivenName)));
            }
            int children = c.Relationships.All.Count(r => r.Type == RelationshipType.Child);
            if (children > 0)
            {
                eulogy.Add(localizer.Format(ChildrenKey, ("count", children.ToString(CultureInfo.InvariantCulture))));
            }
            decimal netWorth = c.NetWorth;
            eulogy.Add(localizer.Format(netWorth >= 0m ? EstateKey : DebtKey, ("value", SessionText.Money(session, money, Math.Abs(netWorth)))));
            foreach (FlagEntry flag in c.Flags.Entries.Where(f => f.Value > 0).OrderBy(f => f.SetOn))
            {
                if (c.Catalogs.Flags.TryGet(flag.FlagId, out FlagDefinition definition) && definition.EulogyKey != null)
                {
                    eulogy.Add(localizer.Get(definition.EulogyKey));
                }
            }

            var facts = new List<ValueRow>
            {
                new ValueRow(localizer.Get(CauseKey), cause == null ? c.Death.CauseId : localizer.Get(cause.NameKey)),
                new ValueRow(localizer.Get(NetWorthKey), SessionText.Money(session, money, netWorth), string.Empty, ValueRow.ToneOf(netWorth)),
                new ValueRow(localizer.Format(GenerationKey, ("value", c.Generation.ToString(CultureInfo.InvariantCulture))), string.Empty),
            };

            List<string> unlocked = achievements == null ? new List<string>() : achievements.Profile.Unlocked
                .Where(u => u.Name == name && u.Generation == c.Generation)
                .Select(u => achievements.Achievements.TryGet(u.Id, out AchievementDefinition a) ? localizer.Get(a.NameKey) : u.Id)
                .ToList();

            IReadOnlyList<Relationship> heirs = HeirCreator.Heirs(session);
            decimal share = heirs.Count == 0 ? 0m
                : InheritanceReport.Settle(c.Wallet.Cash + c.Finances.Portfolio.TotalValue, c.Finances.Debts.TotalBalance, heirs.Count, settings).Received;
            List<HeirOption> options = heirs
                .Select(r => (Relationship: r, Npc: c.Relationships.Npc(r.NpcId)))
                .Where(p => p.Npc != null)
                .OrderBy(p => p.Npc.BornOn)
                .Select(p => new HeirOption(p.Npc.Id, localizer.Format(HeirKey, ("name", p.Npc.Name.GivenName)), localizer.Format(HeirDetailKey,
                    ("age", p.Npc.AgeAt(c.Death.Week).ToString(CultureInfo.InvariantCulture)),
                    ("value", p.Relationship.Value.ToString(PeopleViewModel.NumberFormat, CultureInfo.InvariantCulture)),
                    ("inheritance", SessionText.Money(session, money, share)))))
                .ToList();

            string shareText = string.Join("\n", new[] { localizer.Format(ShareKey, ("name", name)), subtitle }.Concat(eulogy));
            return new LifeSummaryState(name, subtitle, eulogy, facts, unlocked, options, shareText);
        }

        private static (CareerTrackDefinition, CareerLevelDefinition) BestJob(LifeSession session)
        {
            CharacterCareer career = session.Character.Career;
            var jobs = career.History.Select(j => (j.TrackId, j.LevelIndex)).ToList();
            if (career.Current != null)
            {
                jobs.Add((career.Current.TrackId, career.Current.LevelIndex));
            }
            var best = jobs
                .Where(j => session.Career.Tracks.Contains(j.TrackId))
                .Select(j =>
                {
                    CareerTrackDefinition track = session.Career.Tracks.Get(j.TrackId);
                    return (Track: track, Level: track.Levels[Math.Min(j.LevelIndex, track.Levels.Count - 1)]);
                })
                .OrderByDescending(p => p.Level.MonthlySalary)
                .ToList();
            return best.Count == 0 ? (null, null) : (best[0].Track, best[0].Level);
        }

        private static string City(Character character, ILocalizer localizer)
        {
            if (character.Residence == null || !character.Catalogs.Countries.TryGet(character.Residence.CountryId, out Domain.World.CountryDefinition country))
            {
                return string.Empty;
            }
            Domain.World.CityDefinition city = country.FindCity(character.Residence.CityId);
            return localizer.Get(city?.NameKey ?? country.NameKey);
        }
    }
}
