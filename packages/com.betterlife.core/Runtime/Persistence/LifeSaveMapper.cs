using System;
using BetterLife.Core.Sessions;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// Captures a running <see cref="LifeSession"/> as a <see cref="LifeSaveDto"/>
    /// (loading goes through <see cref="LifeSessionFactory.Load"/>).
    /// </summary>
    public static class LifeSaveMapper
    {
        /// <summary>
        /// Captures a life.
        /// </summary>
        /// <param name="session">Running session.</param>
        /// <param name="contentVersion">Version of the content in use.</param>
        /// <returns>The DTO.</returns>
        public static LifeSaveDto ToDto(LifeSession session, int contentVersion)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            var now = session.TimeState.Clock.Now;
            return new LifeSaveDto
            {
                SaveVersion = LifeSaveDto.CurrentVersion,
                ContentVersion = contentVersion,
                Mode = session.Mode.ToString(),
                Ironman = session.IsIronman,
                Summary = new SaveSummaryDto
                {
                    GivenName = session.Character.Name.GivenName,
                    FamilyName = session.Character.Name.FamilyName,
                    Week = now.TotalWeeks,
                    AgeYears = session.Character.AgeAt(now),
                    Alive = session.Character.IsAlive,
                },
                Character = CharacterDtoMapper.ToDto(session.Character),
                Time = TimeStateDtoMapper.ToDto(session.TimeState),
                Events = EventStateDtoMapper.ToDto(session.EventState),
                Random = FoundationDtoMapper.ToDto(session.Random),
                Economy = EconomyStateDtoMapper.ToDto(session.Economy.State),
                PendingSocialHours = session.Relationships.PendingSocialHours,
            };
        }
    }
}
