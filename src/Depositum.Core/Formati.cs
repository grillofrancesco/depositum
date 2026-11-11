using System;
using System.Globalization;

namespace Depositum.Core
{
    /// <summary>
    /// Come l'app scrive e legge date e importi. Tutte le conversioni passano da qui.
    /// </summary>
    public static class Formati
    {
        private const string FormatoData = "dd/MM/yyyy";

        private static readonly string[] FormatiDataAccettati = { FormatoData, "d/M/yyyy", "yyyy-MM-dd" };

        /// <summary>Data come viene mostrata e salvata: gg/mm/aaaa.</summary>
        public static string Data(DateOnly data) => data.ToString(FormatoData);

        /// <summary>
        /// Interpreta una data scritta dall'utente o letta dal file: prima i formati
        /// gg/mm/aaaa, g/m/aaaa e aaaa-mm-gg, poi qualunque data valida per la lingua corrente.
        /// </summary>
        public static bool ProvaALeggereData(string testo, out DateOnly data)
        {
            return DateOnly.TryParseExact(testo, FormatiDataAccettati, CultureInfo.InvariantCulture, DateTimeStyles.None, out data)
                || DateOnly.TryParse(testo, out data);
        }

        /// <summary>Importo come appare in tutta l'app: <c>1.800€</c>, senza spazio prima del simbolo.</summary>
        public static string Importo(long valore) => $"{valore:N0}€";

        /// <summary>
        /// Come <see cref="Importo(long)"/>, per i valori dei grafici (assi e suggerimenti),
        /// che possono non essere interi: vengono arrotondati all'unità.
        /// </summary>
        public static string Importo(double valore) => $"{valore:N0}€";
    }
}
