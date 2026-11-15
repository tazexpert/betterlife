using System;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Money conventions: every amount is real baht of the base year (2026), rounded to satang.
    /// Nominal prices of other eras are shown by multiplying with <see cref="EconomyState.PriceLevel"/>.
    /// </summary>
    public static class Money
    {
        /// <summary>Decimal places (satang).</summary>
        public const int Decimals = 2;

        /// <summary>Months per year.</summary>
        public const int MonthsPerYear = 12;

        /// <summary>
        /// Rounds an amount to satang.
        /// </summary>
        /// <param name="amount">Amount.</param>
        /// <returns>Rounded amount.</returns>
        public static decimal Round(decimal amount) => Math.Round(amount, Decimals, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Converts an annual rate to the equivalent monthly rate.
        /// </summary>
        /// <param name="annual">Annual rate (for example 0.035).</param>
        /// <returns>Monthly rate.</returns>
        public static double Monthly(double annual) => Math.Pow(1.0 + annual, 1.0 / MonthsPerYear) - 1.0;
    }
}
