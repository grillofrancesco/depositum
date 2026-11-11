using System;
using System.Linq;

namespace Depositum.Core
{
    /// <summary>
    /// Regole di validità di un deposito: nome e importo.
    /// </summary>
    public static class RegoleDeposito
    {
        public const int LunghezzaMassimaNome = 30;
        public const int ImportoMinimo = 1;
        public const int ImportoMassimo = 999_999_999;

        // Nel file ogni riga è "Data:…,Nome:importo,…,Totale:…": virgola e due punti separano
        // i campi, "Data" e "Totale" sono chiavi. Un nome che li contiene non si rileggerebbe.
        private static readonly char[] CaratteriVietatiNelNome = { ',', ':' };
        private static readonly string[] NomiRiservati = { "Data", "Totale" };

        /// <summary>
        /// Motivo per cui il nome non è accettabile, oppure <c>null</c> se va bene.
        /// Gli spazi ai lati non contano.
        /// </summary>
        public static string? ErroreNome(string nome)
        {
            nome = nome.Trim();

            if (nome.Length == 0)
                return "Il nome del deposito non può essere vuoto.";

            if (nome.Length > LunghezzaMassimaNome)
                return $"La lunghezza del nome non deve superare {LunghezzaMassimaNome} caratteri.";

            if (nome.IndexOfAny(CaratteriVietatiNelNome) >= 0)
                return "Il nome del deposito non può contenere virgole o due punti.";

            if (NomiRiservati.Any(riservato => string.Equals(riservato, nome, StringComparison.OrdinalIgnoreCase)))
                return "\"Data\" e \"Totale\" non si possono usare come nome di un deposito.";

            return null;
        }

        public static bool NomeValido(string nome) => ErroreNome(nome) is null;

        /// <summary>L'importo è un numero intero compreso tra il minimo e il massimo.</summary>
        public static bool ImportoValido(double valore) =>
            valore >= ImportoMinimo && valore <= ImportoMassimo && valore % 1 == 0;

        /// <summary>
        /// Controlla nome e importo di un nuovo deposito.
        /// Restituisce il messaggio d'errore da mostrare, oppure <c>null</c> se è tutto valido.
        /// </summary>
        public static string? Valida(string nome, double importo)
        {
            string? erroreNome = ErroreNome(nome);
            if (erroreNome != null)
                return erroreNome;

            if (!ImportoValido(importo))
                return "L'importo deve essere un numero intero compreso tra 1 e 1mlrd";

            return null;
        }
    }
}
