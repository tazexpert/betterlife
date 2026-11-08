namespace BetterLife.Core.Luck
{
    /// <summary>
    /// FinalChance = Base + Stat + Luck + Karma + Item, clamped. Every term is kept so the UI can show it.
    /// </summary>
    public sealed class ChanceBreakdown
    {
        /// <summary>
        /// Creates a breakdown (all values in percent).
        /// </summary>
        /// <param name="basePercent">Base.</param>
        /// <param name="stat">Stat term.</param>
        /// <param name="luck">Fortune term.</param>
        /// <param name="karma">Karma term.</param>
        /// <param name="item">Item term (already capped).</param>
        /// <param name="final">Clamped final chance.</param>
        public ChanceBreakdown(float basePercent, float stat, float luck, float karma, float item, float final)
        {
            Base = basePercent;
            Stat = stat;
            Luck = luck;
            Karma = karma;
            Item = item;
            Final = final;
        }

        /// <summary>Base percent.</summary>
        public float Base { get; }

        /// <summary>Stat term.</summary>
        public float Stat { get; }

        /// <summary>Fortune term.</summary>
        public float Luck { get; }

        /// <summary>Karma term.</summary>
        public float Karma { get; }

        /// <summary>Item term.</summary>
        public float Item { get; }

        /// <summary>Sum before clamping.</summary>
        public float Raw => Base + Stat + Luck + Karma + Item;

        /// <summary>Final chance after clamping.</summary>
        public float Final { get; }

        /// <summary>True when the clamp changed the value.</summary>
        public bool IsClamped => Final != Raw;
    }
}
