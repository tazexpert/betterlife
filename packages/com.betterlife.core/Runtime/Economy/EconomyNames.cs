using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// JSON names of the economy enums.
    /// </summary>
    public static class EconomyNames
    {
        /// <summary>Asset classes by JSON name.</summary>
        public static readonly IReadOnlyDictionary<string, AssetClass> AssetClasses = new Dictionary<string, AssetClass>
        {
            ["deposit"] = AssetClass.Deposit,
            ["bond"] = AssetClass.Bond,
            ["fund"] = AssetClass.Fund,
            ["stock"] = AssetClass.Stock,
            ["real_estate"] = AssetClass.RealEstate,
            ["vehicle"] = AssetClass.Vehicle,
            ["crypto"] = AssetClass.Crypto,
        };

        /// <summary>Debt kinds by JSON name.</summary>
        public static readonly IReadOnlyDictionary<string, DebtKind> DebtKinds = new Dictionary<string, DebtKind>
        {
            ["credit_card"] = DebtKind.CreditCard,
            ["personal"] = DebtKind.Personal,
            ["mortgage"] = DebtKind.Mortgage,
            ["car"] = DebtKind.Car,
            ["student"] = DebtKind.Student,
            ["informal"] = DebtKind.Informal,
        };

        /// <summary>
        /// Returns the JSON name of a debt kind.
        /// </summary>
        /// <param name="kind">Kind.</param>
        /// <returns>The name.</returns>
        public static string NameOf(DebtKind kind) => DebtKinds.First(p => p.Value == kind).Key;

        /// <summary>
        /// Returns the JSON name of an asset class.
        /// </summary>
        /// <param name="assetClass">Class.</param>
        /// <returns>The name.</returns>
        public static string NameOf(AssetClass assetClass) => AssetClasses.First(p => p.Value == assetClass).Key;
    }
}
