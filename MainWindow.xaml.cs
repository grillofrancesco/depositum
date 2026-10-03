using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Graphics;

namespace codebase
{
    public sealed partial class MainWindow : Window
    {
        /// <summary>
        /// Possibile path del file futuro.
        /// Può essere:
        /// 1. Un nome di file relativo (es. "records_test.csv" o "records.csv"), cercato nella cartella dell'eseguibile o del progetto.
        /// 2. Un path assoluto in una cartella utente (es. Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MoneyBank", "records.csv")).
        /// 3. In AppData: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MoneyBank", "records.csv").
        /// </summary>
        private string _csvFilePath = "records_test.csv";

        /// <summary>
        /// Lista che contiene gli oggetti Rilevazione estratti dal file CSV.
        /// </summary>
        private readonly List<Rilevazione> _rilevazioni = new();

        public MainWindow()
        {
            InitializeComponent();

            // STEP 2: Imposta 500x800 come dimensione della finestra all'avvio
            AppWindow.Resize(new SizeInt32(500, 800));

            // STEP 4 & 5: Lettura, parsing e visualizzazione dei dati all'avvio
            CaricaDatiAllAvvio();
        }

        /// <summary>
        /// Esegue la lettura e il parsing del file CSV, aggiornando l'interfaccia.
        /// </summary>
        private void CaricaDatiAllAvvio()
        {
            _rilevazioni.Clear();

            string percorsoRisolto = RisolviPercorsoFile(_csvFilePath);

            if (!File.Exists(percorsoRisolto))
            {
                StatusInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning;
                StatusInfoBar.Title = "File non trovato";
                StatusInfoBar.Message = $"Impossibile trovare il file CSV al percorso:\n{percorsoRisolto}";
                StatusInfoBar.IsOpen = true;
                return;
            }

            try
            {
                var righe = File.ReadAllLines(percorsoRisolto);

                foreach (var rigaGrezza in righe)
                {
                    var riga = rigaGrezza.Trim();
                    if (string.IsNullOrWhiteSpace(riga))
                        continue;

                    // Rimuove eventuale ';' finale
                    riga = riga.TrimEnd(';');

                    // Divide la riga tramite il separatore ','
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
                            if (int.TryParse(valore, NumberStyles.Integer, CultureInfo.InvariantCulture, out int totale))
                            {
                                rilevazione.Totale = totale;
                            }
                        }
                        else
                        {
                            // Ogni altro campo non "Data" o "Totale" viene salvato nella mappa dei depositi
                            if (int.TryParse(valore, NumberStyles.Integer, CultureInfo.InvariantCulture, out int importo))
                            {
                                rilevazione.Depositi[chiave] = importo;
                            }
                        }
                    }

                    _rilevazioni.Add(rilevazione);
                }

                // STEP 5: Visualizzazione a schermo
                AggiornaInterfaccia();
            }
            catch (Exception ex)
            {
                StatusInfoBar.Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error;
                StatusInfoBar.Title = "Errore durante il parsing";
                StatusInfoBar.Message = ex.Message;
                StatusInfoBar.IsOpen = true;
            }
        }

        /// <summary>
        /// Mostra a schermo l'ultima rilevazione e il dump completo della struttura dati.
        /// </summary>
        private void AggiornaInterfaccia()
        {
            if (_rilevazioni.Count == 0)
            {
                TxtUltimaData.Text = "Data: Nessun dato";
                TxtUltimoTotale.Text = "Totale: --";
                TxtUltimiDepositi.Text = "Nessuna voce presente nel file.";
                TxtDumpCompleto.Text = "Nessun record caricato.";
                return;
            }

            // 1. Dump dell'ultima rilevazione rilevata
            var ultima = _rilevazioni.Last();
            TxtUltimaData.Text = $"Data: {ultima.Data:dd/MM/yyyy}";
            TxtUltimoTotale.Text = $"Totale: {ultima.Totale:N0}";

            var sbUltimi = new StringBuilder();
            foreach (var kvp in ultima.Depositi)
            {
                sbUltimi.AppendLine($"• {kvp.Key}: {kvp.Value:N0}");
            }
            TxtUltimiDepositi.Text = sbUltimi.ToString().TrimEnd();

            // 2. Dump completo dell'intera struttura salvata
            var sbDump = new StringBuilder();
            for (int i = 0; i < _rilevazioni.Count; i++)
            {
                var r = _rilevazioni[i];
                sbDump.AppendLine($"--- Rilevazione #{i + 1} ---");
                sbDump.AppendLine($"Data: {r.Data:dd/MM/yyyy}");
                sbDump.AppendLine("Depositi:");
                foreach (var dep in r.Depositi)
                {
                    sbDump.AppendLine($"  - {dep.Key}: {dep.Value:N0}");
                }
                sbDump.AppendLine($"Totale: {r.Totale:N0}");
                if (i < _rilevazioni.Count - 1)
                {
                    sbDump.AppendLine();
                }
            }

            TxtDumpCompleto.Text = sbDump.ToString();
        }

        /// <summary>
        /// Trova il file CSV gestendo sia percorsi assoluti che percorsi relativi
        /// all'eseguibile (bin) o alla directory radice di sviluppo del progetto.
        /// </summary>
        private static string RisolviPercorsoFile(string path)
        {
            if (Path.IsPathRooted(path) && File.Exists(path))
                return path;

            // 1. Cerca nella directory corrente o BaseDirectory
            string inBase = Path.Combine(AppContext.BaseDirectory, path);
            if (File.Exists(inBase))
                return inBase;

            if (File.Exists(path))
                return Path.GetFullPath(path);

            // 2. Fallback per ambiente di sviluppo: cerca a ritroso risalendo da bin/Debug/...
            string current = AppContext.BaseDirectory;
            for (int i = 0; i < 5; i++)
            {
                string candidate = Path.Combine(current, path);
                if (File.Exists(candidate))
                    return candidate;

                var parent = Directory.GetParent(current);
                if (parent == null)
                    break;
                current = parent.FullName;
            }

            return inBase;
        }
    }
}
