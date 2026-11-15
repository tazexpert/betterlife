namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// One person in the People list.
    /// </summary>
    public sealed class PersonRow
    {
        /// <summary>
        /// Creates a row.
        /// </summary>
        /// <param name="npcId">NPC id.</param>
        /// <param name="name">Full name.</param>
        /// <param name="detail">Relationship type and age (or "passed away").</param>
        /// <param name="value">Closeness -100..100.</param>
        /// <param name="valueText">Closeness text.</param>
        /// <param name="isAlive">Whether the person is alive.</param>
        public PersonRow(int npcId, string name, string detail, float value, string valueText, bool isAlive)
        {
            NpcId = npcId;
            Name = name;
            Detail = detail;
            Value = value;
            ValueText = valueText;
            IsAlive = isAlive;
        }

        /// <summary>NPC id.</summary>
        public int NpcId { get; }

        /// <summary>Full name.</summary>
        public string Name { get; }

        /// <summary>Relationship type and age.</summary>
        public string Detail { get; }

        /// <summary>Closeness -100..100.</summary>
        public float Value { get; }

        /// <summary>Closeness text.</summary>
        public string ValueText { get; }

        /// <summary>Whether the person is alive.</summary>
        public bool IsAlive { get; }
    }
}
