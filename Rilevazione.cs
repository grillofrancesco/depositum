using System;
using System.Collections.Generic;
using System.Linq;

namespace codebase
{
    /// <summary>
    /// Rappresenta una singola rilevazione letta dal file CSV.
    /// Struttura:
    /// {
    ///   data: DateOnly,
    ///   depositi: Dictionary&lt;string, int&gt;,
    ///   totale: int
    /// }
    /// </summary>
    public class Rilevazione
    {
        public DateOnly Data { get; set; }
        public Dictionary<string, int> Depositi { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public int Totale { get; set; }

        public override string ToString()
        {
            string depositiStr = string.Join(", ", Depositi.Select(d => $"{d.Key}: {d.Value}"));
            return $"Data: {Data:dd/MM/yyyy} | {depositiStr} | Totale: {Totale}";
        }
    }
}
