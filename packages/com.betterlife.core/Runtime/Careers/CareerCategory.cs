namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Broad kind of career.
    /// </summary>
    public enum CareerCategory
    {
        /// <summary>Private company employee.</summary>
        Corporate = 0,

        /// <summary>Civil servant (no layoffs, pension).</summary>
        Government = 1,

        /// <summary>Self-employed; income follows performance.</summary>
        Freelance = 2,

        /// <summary>Business owner; income follows the economy.</summary>
        Business = 3,

        /// <summary>Volatile, risky careers (sports, entertainment...).</summary>
        HighRisk = 4,

        /// <summary>Illegal work with a risk of arrest.</summary>
        Illegal = 5,

        /// <summary>Hourly unskilled work.</summary>
        Labor = 6,
    }
}
