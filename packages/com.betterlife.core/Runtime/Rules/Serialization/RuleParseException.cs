using System;

namespace BetterLife.Core.Rules.Serialization
{
    /// <summary>
    /// Thrown when rule JSON is invalid. The message starts with the JSON path of the problem.
    /// </summary>
    public sealed class RuleParseException : Exception
    {
        /// <summary>
        /// Creates the exception.
        /// </summary>
        /// <param name="path">JSON path, for example "$.events[3].effects[1]".</param>
        /// <param name="problem">What is wrong.</param>
        /// <param name="inner">Underlying exception, or null.</param>
        public RuleParseException(string path, string problem, Exception inner = null)
            : base($"{path}: {problem}", inner)
        {
            Path = path;
            Problem = problem;
        }

        /// <summary>JSON path of the problem.</summary>
        public string Path { get; }

        /// <summary>What is wrong.</summary>
        public string Problem { get; }
    }
}
