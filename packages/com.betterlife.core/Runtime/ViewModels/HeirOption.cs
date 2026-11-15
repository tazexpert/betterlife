namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// A child who can carry on the family line.
    /// </summary>
    public sealed class HeirOption
    {
        /// <summary>
        /// Creates an option.
        /// </summary>
        /// <param name="npcId">NPC id of the child.</param>
        /// <param name="label">Button text (continue as ...).</param>
        /// <param name="detail">Age, closeness and expected inheritance.</param>
        public HeirOption(int npcId, string label, string detail)
        {
            NpcId = npcId;
            Label = label;
            Detail = detail;
        }

        /// <summary>NPC id of the child.</summary>
        public int NpcId { get; }

        /// <summary>Button text.</summary>
        public string Label { get; }

        /// <summary>Age, closeness and expected inheritance.</summary>
        public string Detail { get; }
    }
}
