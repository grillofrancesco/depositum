using System;
using System.IO;

namespace Depositum.Services
{
    /// <summary>
    /// Piccole preferenze dell'utente (tema, lingua), una per file di testo accanto a history.dpstm.
    /// Sono comodità, non dati da proteggere: se un file manca o non si legge si usa il valore
    /// predefinito e l'app parte comunque.
    /// </summary>
    internal static class Preferenze
    {
        private static string Percorso(string nome) =>
            Path.Combine(DataService.CartellaDati, nome);

        /// <summary>Contenuto del file, senza spazi ai lati; <c>null</c> se manca o non si legge.</summary>
        public static string? Leggi(string nome)
        {
            try
            {
                string percorso = Percorso(nome);
                return File.Exists(percorso) ? File.ReadAllText(percorso).Trim() : null;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return null;
            }
        }

        /// <summary>Scrive il valore. Restituisce <c>false</c> se non è stato possibile.</summary>
        public static bool Scrivi(string nome, string valore)
        {
            try
            {
                string percorso = Percorso(nome);
                Directory.CreateDirectory(Path.GetDirectoryName(percorso)!);
                File.WriteAllText(percorso, valore);
                return true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
