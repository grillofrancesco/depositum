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

        /// <summary>Il nome, tolti gli spazi ai lati, non è vuoto e non supera la lunghezza massima.</summary>
        public static bool NomeValido(string nome)
        {
            nome = nome.Trim();
            return nome.Length > 0 && nome.Length <= LunghezzaMassimaNome;
        }

        /// <summary>L'importo è un numero intero compreso tra il minimo e il massimo.</summary>
        public static bool ImportoValido(double valore) =>
            valore >= ImportoMinimo && valore <= ImportoMassimo && valore % 1 == 0;

        /// <summary>
        /// Controlla nome e importo di un nuovo deposito.
        /// Restituisce il messaggio d'errore da mostrare, oppure <c>null</c> se è tutto valido.
        /// </summary>
        public static string? Valida(string nome, double importo)
        {
            nome = nome.Trim();

            if (nome.Length == 0)
                return "Il nome del deposito non può essere vuoto.";

            if (nome.Length > LunghezzaMassimaNome)
                return $"La lunghezza del nome non deve superare {LunghezzaMassimaNome} caratteri.";

            if (!ImportoValido(importo))
                return "L'importo deve essere un numero intero compreso tra 1 e 1mlrd";

            return null;
        }
    }
}
