namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Kind of debt.
    /// </summary>
    public enum DebtKind
    {
        /// <summary>Revolving credit card.</summary>
        CreditCard = 0,
        /// <summary>Unsecured personal loan.</summary>
        Personal = 1,
        /// <summary>Home loan secured by property.</summary>
        Mortgage = 2,
        /// <summary>Car loan secured by a vehicle.</summary>
        Car = 3,
        /// <summary>Student loan (กยศ.).</summary>
        Student = 4,
        /// <summary>Informal loan shark debt (หนี้นอกระบบ).</summary>
        Informal = 5,
    }
}
