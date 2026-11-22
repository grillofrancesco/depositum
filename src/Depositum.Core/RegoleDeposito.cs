using System;
using System.Linq;

namespace Depositum.Core
{
    /// <summary>Motivi per cui un deposito non è valido. Il messaggio da mostrare lo sceglie l'app, nella lingua dell'interfaccia.</summary>
    public enum ErroreDeposito
    {
        NomeVuoto,
        NomeTroppoLungo,
        NomeConCaratteriVietati,
        NomeRiservato,
        ImportoNonValido
    }

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
        public static ErroreDeposito? ErroreNome(string nome)
        {
            nome = nome.Trim();

            if (nome.Length == 0)
                return ErroreDeposito.NomeVuoto;

            if (nome.Length > LunghezzaMassimaNome)
                return ErroreDeposito.NomeTroppoLungo;

            if (nome.IndexOfAny(CaratteriVietatiNelNome) >= 0)
                return ErroreDeposito.NomeConCaratteriVietati;

            if (NomiRiservati.Any(riservato => string.Equals(riservato, nome, StringComparison.OrdinalIgnoreCase)))
                return ErroreDeposito.NomeRiservato;

            return null;
        }

        public static bool NomeValido(string nome) => ErroreNome(nome) is null;

        /// <summary>L'importo è un numero intero compreso tra il minimo e il massimo.</summary>
        public static bool ImportoValido(double valore) =>
            valore >= ImportoMinimo && valore <= ImportoMassimo && valore % 1 == 0;

        /// <summary>
        /// Controlla nome e importo di un nuovo deposito, nell'ordine: prima il nome, poi l'importo.
        /// Restituisce il primo errore trovato, oppure <c>null</c> se è tutto valido.
        /// </summary>
        public static ErroreDeposito? Valida(string nome, double importo)
        {
            ErroreDeposito? erroreNome = ErroreNome(nome);
            if (erroreNome != null)
                return erroreNome;

            if (!ImportoValido(importo))
                return ErroreDeposito.ImportoNonValido;

            return null;
        }
    }
}
