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
        /// Aggiunge una nuova rilevazione in memoria e la appende a 'history.csv' in AppData,
        /// creando la cartella 'Depositum' se non ancora esistente.
        /// </summary>
        public static void AggiungiRilevazione(Rilevazione nuova)
        {
            Rilevazioni.Add(nuova);

            string? cartella = Path.GetDirectoryName(CsvFilePath);
            if (!string.IsNullOrEmpty(cartella) && !Directory.Exists(cartella))
            {
                Directory.CreateDirectory(cartella);
            }

            File.AppendAllLines(CsvFilePath, new[] { RilevazioneCsv.InRiga(nuova) });
        }
    }
}
