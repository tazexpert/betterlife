namespace BetterLife.Core.Rules
{
    /// <summary>
    /// Comparison used by numeric conditions. JSON symbols: == != &lt; &lt;= &gt; &gt;=.
    /// </summary>
    public enum ComparisonOperator
    {
        /// <summary>Equal (==).</summary>
        Equal = 0,
        /// <summary>Not equal (!=).</summary>
        NotEqual = 1,
        /// <summary>Less than (&lt;).</summary>
        Less = 2,
        /// <summary>Less than or equal (&lt;=).</summary>
        LessOrEqual = 3,
        /// <summary>Greater than (&gt;).</summary>
        Greater = 4,
        /// <summary>Greater than or equal (&gt;=).</summary>
        GreaterOrEqual = 5,
    }
}
