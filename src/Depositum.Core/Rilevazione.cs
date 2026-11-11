using System;
using System.Collections.Generic;
using System.Linq;

namespace Depositum.Core
{
    /// <summary>
    /// Rappresenta una singola rilevazione letta dal file CSV.
    /// Struttura:
    /// {
    ///   data: DateOnly,
    ///   depositi: Dictionary&lt;string, int&gt;,
    ///   totale: long
    /// }
    /// </summary>
    public class Rilevazione
    {
        public DateOnly Data { get; set; }
        public Dictionary<string, int> Depositi { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// Somma dei depositi così come è stata salvata. È un <c>long</c> perché più depositi
        /// vicini al massimo consentito superano il limite di un <c>int</c>.
        /// </summary>
        public long Totale { get; set; }

        /// <summary>Somma degli importi dei depositi, senza limite di grandezza.</summary>
        public long SommaDepositi() => Depositi.Values.Sum(valore => (long)valore);

        public override string ToString()
        {
            string depositiStr = string.Join(", ", Depositi.Select(d => $"{d.Key}: {d.Value}"));
            return $"Data: {Formati.Data(Data)} | {depositiStr} | Totale: {Totale}";
        }
    }
}
