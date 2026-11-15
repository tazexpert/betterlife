namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// One button of an offer: an amount, a share or a method, with whether it can be chosen now.
    /// </summary>
    public sealed class AmountChoice
    {
        /// <summary>
        /// Creates a choice.
        /// </summary>
        /// <param name="id">What the button acts on besides the offer (method id...), or null.</param>
        /// <param name="label">Button text.</param>
        /// <param name="amount">Real money amount or share (0..1) the action uses; 0 when not used.</param>
        /// <param name="isAvailable">Whether it can be chosen now.</param>
        /// <param name="detail">Line under the button (terms, or why it is unavailable), or empty.</param>
        public AmountChoice(string id, string label, decimal amount, bool isAvailable, string detail = "")
        {
            Id = id;
            Label = label;
            Amount = amount;
            IsAvailable = isAvailable;
            Detail = detail ?? string.Empty;
        }

        /// <summary>What the button acts on besides the offer, or null.</summary>
        public string Id { get; }

        /// <summary>Button text.</summary>
        public string Label { get; }

        /// <summary>Real money amount or share the action uses.</summary>
        public decimal Amount { get; }

        /// <summary>Whether it can be chosen now.</summary>
        public bool IsAvailable { get; }

        /// <summary>Line under the button, or empty.</summary>
        public string Detail { get; }
    }
}
