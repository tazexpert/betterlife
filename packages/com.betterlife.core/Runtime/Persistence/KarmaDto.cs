using System;
using System.Collections.Generic;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a karma ledger.</summary>
    [Serializable]
    public sealed class KarmaDto
    {
        /// <summary>Good karma.</summary>
        public float Good;

        /// <summary>Bad karma.</summary>
        public float Bad;

        /// <summary>Pending debts.</summary>
        public List<KarmicDebtDto> Debts = new List<KarmicDebtDto>();
    }
}
