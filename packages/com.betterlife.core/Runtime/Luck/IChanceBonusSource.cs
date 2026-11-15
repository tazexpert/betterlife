using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Time;

namespace BetterLife.Core.Luck
{
    /// <summary>
    /// A source of item bonus percent (for example a charm bought with IAP). The total is capped by
    /// <see cref="LuckSettings.ItemBonusCap"/> so purchases can never dominate a roll.
    /// </summary>
    public interface IChanceBonusSource
    {
        /// <summary>
        /// Returns the bonus percent for a roll.
        /// </summary>
        /// <param name="character">Character.</param>
        /// <param name="spec">The roll's spec.</param>
        /// <param name="now">Current week.</param>
        /// <returns>Bonus percent.</returns>
        float BonusFor(Character character, ChanceSpec spec, GameDate now);
    }
}
