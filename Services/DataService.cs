using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Depositum
{
    /// <summary>
    /// Servizio centralizzato per il caricamento, parsing e salvataggio dei dati delle rilevazioni.
    /// Il file di salvataggio è unicamente 'Depositum/history.csv' nella cartella AppData locale dell'utente.
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
                var riga = rigaGrezza.Trim();
                if (string.IsNullOrWhiteSpace(riga))
                    continue;

                riga = riga.TrimEnd(';');
                var campi = riga.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var rilevazione = new Rilevazione();

                foreach (var campo in campi)
                {
                    int colonIndex = campo.IndexOf(':');
                    if (colonIndex <= 0)
                        continue;

                    string chiave = campo.Substring(0, colonIndex).Trim();
                    string valore = campo.Substring(colonIndex + 1).Trim().TrimEnd(';');

                    if (string.Equals(chiave, "Data", StringComparison.OrdinalIgnoreCase))
                    {
                        if (DateOnly.TryParseExact(valore, new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
                        {
                            rilevazione.Data = data;
                        }
                        else if (DateOnly.TryParse(valore, out var dataFallback))
                        {
                            rilevazione.Data = dataFallback;
                        }
                    }
                    else if (string.Equals(chiave, "Totale", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(valore, NumberStyles.Integer, CultureInfo.InvariantCulture, out int tot))
                        {
                            rilevazione.Totale = tot;
                        }
                    }
                    else
                    {
                        if (int.TryParse(valore, NumberStyles.Integer, CultureInfo.InvariantCulture, out int importo))
                        {
                            rilevazione.Depositi[chiave] = importo;
                        }
                    }
                }

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

            var sb = new StringBuilder();
            sb.Append($"Data:{nuova.Data:dd/MM/yyyy}");
            foreach (var dep in nuova.Depositi)
            {
                sb.Append($",{dep.Key}:{dep.Value}");
            }
            sb.Append($",Totale:{nuova.Totale}");

            File.AppendAllLines(CsvFilePath, new[] { sb.ToString() });
        }
    }
}
