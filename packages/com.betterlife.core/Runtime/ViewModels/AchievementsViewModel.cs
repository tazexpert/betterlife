using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Achievements;
using BetterLife.Core.Localization;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Builds the achievement list: unlocked first, hidden ones kept secret until unlocked.
    /// </summary>
    public static class AchievementsViewModel
    {
        /// <summary>Progress text (placeholders <c>{count}</c>, <c>{total}</c>).</summary>
        public const string ProgressKey = "achievements.progress";
        /// <summary>Name of a secret achievement.</summary>
        public const string SecretNameKey = "achievements.secret.name";
        /// <summary>Description of a secret achievement.</summary>
        public const string SecretDescKey = "achievements.secret.desc";
        /// <summary>Unlock detail (placeholders <c>{name}</c>, <c>{generation}</c>).</summary>
        public const string UnlockedByKey = "achievements.unlocked_by";

        /// <summary>
        /// Builds the list.
        /// </summary>
        /// <param name="tracker">Achievement tracker.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="progress">Progress text.</param>
        /// <returns>Rows, unlocked first, then in catalog order.</returns>
        public static IReadOnlyList<AchievementRow> Build(AchievementTracker tracker, ILocalizer localizer, out string progress)
        {
            if (tracker == null)
            {
                throw new ArgumentNullException(nameof(tracker));
            }
            if (localizer == null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }
            List<AchievementRow> rows = tracker.Achievements.All.Select(a =>
            {
                AchievementUnlock unlock = tracker.Profile.Find(a.Id);
                bool open = unlock != null;
                bool secret = a.Hidden && !open;
                string detail = open
                    ? localizer.Format(UnlockedByKey, ("name", unlock.Name ?? string.Empty), ("generation", unlock.Generation.ToString(CultureInfo.InvariantCulture)))
                    : string.Empty;
                return new AchievementRow(a.Id, localizer.Get(secret ? SecretNameKey : a.NameKey), localizer.Get(secret ? SecretDescKey : a.DescKey), open, detail);
            }).ToList();
            int count = rows.Count(r => r.IsUnlocked);
            progress = localizer.Format(ProgressKey, ("count", count.ToString(CultureInfo.InvariantCulture)), ("total", rows.Count.ToString(CultureInfo.InvariantCulture)));
            return rows.Where(r => r.IsUnlocked).Concat(rows.Where(r => !r.IsUnlocked)).ToList();
        }
    }
}
