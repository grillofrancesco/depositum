using Depositum.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Depositum
{
    /// <summary>
    /// Servizio centralizzato per il caricamento e il salvataggio delle rilevazioni.
    /// Il file di salvataggio è unicamente 'Depositum/history.csv' nella cartella AppData locale dell'utente.
    /// L'interpretazione delle singole righe è in <see cref="RilevazioneCsv"/>.
    /// </summary>
    public static class DataService
    {
        public static string CsvFilePath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Depositum",
            "history.csv"
        );

        public static List<Rilevazione> Rilevazioni { get; } = new();

        public static Rilevazione? UltimaRilevazione => Rilevazioni.LastOrDefault();

        /// <summary>
        /// Legge e interpreta il file CSV in AppData. Se non esiste, lascia la lista vuota.
        /// </summary>
        /// <exception cref="IOException">Il file esiste ma non si riesce a leggere (per esempio è in uso).</exception>
        /// <exception cref="UnauthorizedAccessException">Manca il permesso di leggere il file.</exception>
        public static void CaricaDati()
        {
            Rilevazioni.Clear();

            Debug.WriteLine(CsvFilePath);

            if (!File.Exists(CsvFilePath))
                return;

            var righe = File.ReadAllLines(CsvFilePath);

            foreach (var rigaGrezza in righe)
            {
                var rilevazione = RilevazioneCsv.DaRiga(rigaGrezza);
                if (rilevazione != null)
                    Rilevazioni.Add(rilevazione);
            }
        }

        /// <summary>
        /// Appende una nuova rilevazione a 'history.csv' in AppData, creando la cartella
        /// 'Depositum' se non ancora esistente, e poi la aggiunge a quelle in memoria.
        /// Se la scrittura fallisce la rilevazione non viene aggiunta nemmeno in memoria.
        /// </summary>
        /// <exception cref="IOException">Il file non si riesce a scrivere (per esempio è in uso).</exception>
        /// <exception cref="UnauthorizedAccessException">Manca il permesso di scrivere il file.</exception>
        public static void AggiungiRilevazione(Rilevazione nuova)
        {
            string? cartella = Path.GetDirectoryName(CsvFilePath);
            if (!string.IsNullOrEmpty(cartella) && !Directory.Exists(cartella))
            {
                Directory.CreateDirectory(cartella);
            }

            File.AppendAllLines(CsvFilePath, new[] { RilevazioneCsv.InRiga(nuova) });

            Rilevazioni.Add(nuova);
        }
    }
}
