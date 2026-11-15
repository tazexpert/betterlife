using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a whole life (the payload of a save file).</summary>
    [Serializable]
    public sealed class LifeSaveDto
    {
        /// <summary>Current save format version.</summary>
        public const int CurrentVersion = 1;

        /// <summary>Save format version.</summary>
        public int SaveVersion = CurrentVersion;

        /// <summary>Content version the life was saved with.</summary>
        public int ContentVersion;

        /// <summary>Creation mode (enum name).</summary>
        public string Mode;

        /// <summary>Whether the life is Ironman.</summary>
        public bool Ironman;

        /// <summary>Slot summary.</summary>
        public SaveSummaryDto Summary = new SaveSummaryDto();

        /// <summary>The character with finances, schooling, work, relationships and habits.</summary>
        public CharacterDto Character;

        /// <summary>Clock and weekly plan.</summary>
        public TimeStateDto Time;

        /// <summary>Event history, inbox and schedules.</summary>
        public EventStateDto Events;

        /// <summary>Random streams.</summary>
        public RandomRegistryDto Random;

        /// <summary>Macro economy.</summary>
        public EconomyStateDto Economy;

        /// <summary>Socialize hours counted so far this month.</summary>
        public int PendingSocialHours;
    }
}
