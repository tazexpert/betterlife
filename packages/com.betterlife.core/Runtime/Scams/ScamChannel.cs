namespace BetterLife.Core.Scams
{
    /// <summary>
    /// How a scam reaches the character.
    /// </summary>
    public enum ScamChannel
    {
        /// <summary>A phone call (call-center gangs).</summary>
        Phone = 0,

        /// <summary>SMS or chat message with a link.</summary>
        Sms = 1,

        /// <summary>A website, online shop or social media.</summary>
        Online = 2,

        /// <summary>A fake romantic partner.</summary>
        Romance = 3,

        /// <summary>In person.</summary>
        InPerson = 4,

        /// <summary>Inside an online game.</summary>
        Game = 5,
    }
}
