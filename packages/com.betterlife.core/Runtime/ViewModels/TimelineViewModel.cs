using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Careers;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.World;
using BetterLife.Core.Education;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Life;
using BetterLife.Core.Localization;
using BetterLife.Core.Relationships;
using BetterLife.Core.Scams;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Builds the life timeline from saved state only (event history, credentials, jobs, relationships, scams, death),
    /// newest first.
    /// </summary>
    public static class TimelineViewModel
    {
        /// <summary>Most entries returned.</summary>
        public const int MaxEntries = 200;
        /// <summary>Birth entry (placeholder <c>{city}</c>).</summary>
        public const string BornKey = "timeline.born";
        /// <summary>Graduation (placeholder <c>{program}</c>).</summary>
        public const string GraduatedKey = "timeline.graduated";
        /// <summary>Job start (placeholder <c>{track}</c>).</summary>
        public const string JobStartedKey = "timeline.job_started";
        /// <summary>Job end key prefix (<c>timeline.job_end.fired</c>...; placeholder <c>{track}</c>).</summary>
        public const string JobEndPrefix = "timeline.job_end.";
        /// <summary>Relationship start key prefix (<c>timeline.became.spouse</c>...; placeholder <c>{name}</c>).</summary>
        public const string BecamePrefix = "timeline.became.";
        /// <summary>Child born (placeholder <c>{name}</c>).</summary>
        public const string ChildBornKey = "timeline.child_born";
        /// <summary>Someone close died (placeholders <c>{name}</c>, <c>{type}</c>).</summary>
        public const string LossKey = "timeline.loss";
        /// <summary>Scammed (placeholders <c>{scam}</c>, <c>{lost}</c>).</summary>
        public const string ScammedKey = "timeline.scammed";
        /// <summary>The character's death (placeholder <c>{cause}</c>).</summary>
        public const string DiedKey = "timeline.died";
        /// <summary>Event answer detail (placeholder <c>{choice}</c>).</summary>
        public const string ChoseKey = "timeline.chose";
        /// <summary>Event answer that failed (placeholder <c>{choice}</c>).</summary>
        public const string ChoseFailedKey = "timeline.chose_failed";

        private static readonly RelationshipType[] Loved = { RelationshipType.Partner, RelationshipType.Spouse, RelationshipType.Ex };

        /// <summary>
        /// Builds the timeline.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="money">Money formatter.</param>
        /// <returns>Entries, newest first (at most <see cref="MaxEntries"/>).</returns>
        public static IReadOnlyList<TimelineEntry> Build(LifeSession session, ILocalizer localizer, MoneyFormatter money)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (localizer == null || money == null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }
            Character character = session.Character;
            var entries = new List<(GameDate Week, TimelineEntryKind Kind, string Title, string Detail)>();
            GameDate birth = character.Birth.Date.ToGameDate();
            entries.Add((birth, TimelineEntryKind.Birth, localizer.Format(BornKey, ("city", CityName(character, localizer))), string.Empty));

            foreach (EventHistoryEntry entry in session.Events.State.History)
            {
                if (!session.Events.Events.TryGet(entry.EventId, out LifeEventDefinition definition))
                {
                    continue;
                }
                string detail = string.Empty;
                EventChoice choice = entry.ChoiceId == null ? null : definition.FindChoice(entry.ChoiceId);
                if (choice != null)
                {
                    detail = localizer.Format(entry.Succeeded ? ChoseKey : ChoseFailedKey, ("choice", localizer.Get(choice.TextKey)));
                }
                entries.Add((entry.Week, TimelineEntryKind.Event, localizer.Get(definition.TitleKey), detail));
            }

            foreach (Credential credential in character.Education.Credentials)
            {
                entries.Add((credential.GraduatedOn, TimelineEntryKind.Education,
                    localizer.Format(GraduatedKey, ("program", ProgramName(session, credential.ProgramId, localizer))), string.Empty));
            }

            foreach (PastJob job in character.Career.History)
            {
                string track = TrackName(session, job.TrackId, localizer);
                entries.Add((job.StartedOn, TimelineEntryKind.Career, localizer.Format(JobStartedKey, ("track", track)), string.Empty));
                entries.Add((job.EndedOn, TimelineEntryKind.Career, localizer.Format(SessionText.EnumKey(JobEndPrefix, job.Reason), ("track", track)), string.Empty));
            }
            if (character.Career.Current != null)
            {
                entries.Add((character.Career.Current.StartedOn, TimelineEntryKind.Career,
                    localizer.Format(JobStartedKey, ("track", TrackName(session, character.Career.Current.TrackId, localizer))), string.Empty));
            }

            foreach (Relationship relationship in character.Relationships.All)
            {
                Npc npc = character.Relationships.Npc(relationship.NpcId);
                if (npc == null)
                {
                    continue;
                }
                string name = npc.Name.GivenName + " " + npc.Name.FamilyName;
                if (relationship.Type == RelationshipType.Child && npc.BornOn >= birth)
                {
                    entries.Add((npc.BornOn, TimelineEntryKind.Relationship, localizer.Format(ChildBornKey, ("name", name)), string.Empty));
                }
                else if (Loved.Contains(relationship.Type))
                {
                    entries.Add((relationship.Since, TimelineEntryKind.Relationship,
                        localizer.Format(BecamePrefix + RelationshipNames.NameOf(relationship.Type), ("name", name)), string.Empty));
                }
                if (npc.DiedOn.HasValue && relationship.Type != RelationshipType.Contact)
                {
                    entries.Add((npc.DiedOn.Value, TimelineEntryKind.Loss,
                        localizer.Format(LossKey, ("name", name), ("type", PeopleViewModel.TypeName(relationship.Type, localizer))), string.Empty));
                }
            }

            foreach (ScamIncident incident in character.Scams.Incidents)
            {
                string scam = session.Scams.Scams.TryGet(incident.ScamId, out ScamDefinition definition) ? localizer.Get(definition.NameKey) : incident.ScamId;
                entries.Add((incident.Week, TimelineEntryKind.Scam,
                    localizer.Format(ScammedKey, ("scam", scam), ("lost", SessionText.Money(session, money, incident.Lost))), string.Empty));
            }

            if (character.Death != null)
            {
                string cause = session.Deaths.Causes.TryGet(character.Death.CauseId, out DeathCauseDefinition definition)
                    ? localizer.Get(definition.NameKey)
                    : character.Death.CauseId;
                entries.Add((character.Death.Week, TimelineEntryKind.Death, localizer.Format(DiedKey, ("cause", cause)), string.Empty));
            }

            // Stable order: newest first, and within a week in the order the entries were collected (birth before events).
            return entries
                .Select((e, index) => (Entry: e, Index: index))
                .OrderByDescending(p => p.Entry.Week)
                .ThenByDescending(p => p.Index)
                .Take(MaxEntries)
                .Select(p => new TimelineEntry(p.Entry.Week, p.Entry.Kind, GameDateText.MonthYear(p.Entry.Week, localizer),
                    SessionText.Age(session, p.Entry.Week, localizer), p.Entry.Title, p.Entry.Detail))
                .ToList();
        }

        private static string CityName(Character character, ILocalizer localizer)
        {
            Residence home = character.Residence;
            if (home == null || !character.Catalogs.Countries.TryGet(home.CountryId, out CountryDefinition country))
            {
                return string.Empty;
            }
            CityDefinition city = country.FindCity(home.CityId);
            return localizer.Get(city?.NameKey ?? country.NameKey);
        }

        private static string ProgramName(LifeSession session, string programId, ILocalizer localizer)
        {
            return session.Education.Programs.TryGet(programId, out EducationProgramDefinition program) ? localizer.Get(program.NameKey) : programId;
        }

        private static string TrackName(LifeSession session, string trackId, ILocalizer localizer)
        {
            return session.Career.Tracks.TryGet(trackId, out CareerTrackDefinition track) ? localizer.Get(track.NameKey) : trackId;
        }
    }
}
