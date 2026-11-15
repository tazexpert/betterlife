using System;
using System.Collections.Generic;
using BetterLife.Core.Content;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Education;
using BetterLife.Core.Domain;
using BetterLife.Core.Events;
using BetterLife.Core.Randomness;
using BetterLife.Core.Relationships;
using BetterLife.Core.Sessions;

namespace BetterLife.Core.Legacy
{
    /// <summary>
    /// Continues the family line: lists the heirs of a finished life and starts the life of the chosen one.
    /// </summary>
    public sealed class LegacyService
    {
        private readonly IContentRepository _content;
        private readonly HeirCreator _creator;

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="content">Validated content.</param>
        public LegacyService(IContentRepository content)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _creator = new HeirCreator(content.Get<CharacterCatalogs>(), content.Get<DefinitionCatalog<EducationProgramDefinition>>(), content.Get<LegacySettings>());
        }

        /// <summary>
        /// Living children of a finished life.
        /// </summary>
        /// <param name="deceased">A finished life.</param>
        /// <returns>Relationships with the heirs.</returns>
        public IReadOnlyList<Relationship> Heirs(LifeSession deceased) => HeirCreator.Heirs(deceased);

        /// <summary>
        /// Starts the heir's life. The caller disposes the old life afterwards.
        /// </summary>
        /// <param name="deceased">A finished life.</param>
        /// <param name="heirNpcId">NPC id of a living child.</param>
        /// <param name="bus">Event bus for the new life.</param>
        /// <param name="report">How the estate was settled.</param>
        /// <returns>The heir's running life.</returns>
        public LifeSession ContinueAs(LifeSession deceased, int heirNpcId, IEventBus bus, out InheritanceReport report)
        {
            Character heir = _creator.Create(deceased, heirNpcId, out report, out RandomRegistry random);
            return LifeSessionFactory.CreateHeir(_content, heir, random, deceased, bus);
        }
    }
}
