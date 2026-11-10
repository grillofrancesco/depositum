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

        /// <summary>Importo come appare nella dashboard: <c>1.800€</c>.</summary>
        public static string Importo(int valore) => $"{valore:N0}€";

        /// <summary>Importo come appare nei grafici (assi e suggerimenti): <c>1.800 €</c>.</summary>
        public static string ImportoGrafico(double valore) => $"{valore:N0} €";
    }
}
