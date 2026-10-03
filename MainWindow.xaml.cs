using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Windows.Graphics;
using Windows.UI;

namespace codebase
{
    public sealed partial class MainWindow : Window
    {
        /// <summary>
        /// Possibile path del file CSV futuro.
        /// </summary>
        private string _csvFilePath = "records_test.csv";

        /// <summary>
        /// Lista in memoria di tutte le rilevazioni salvate.
        /// </summary>
        private readonly List<Rilevazione> _rilevazioni = new();

        /// <summary>
        /// Palette di 8 colori moderni, puliti e desaturati (non troppo accesi/saturi).
        /// </summary>
        private static readonly string[] PaletteColori = new[]
        {
            "#1A6B6B", // Teal desaturato (come nel mockup)
            "#206535", // Forest Green desaturato (come nel mockup)
            "#2B4A6F", // Slate Blue elegante
            "#733F35", // Terracotta / Ruggine morbido
            "#58395B", // Prugna / Melanzana profondo
            "#1C4E5E", // Blu petrolio
            "#644F2E", // Bronzo / Cuoio caldo
            "#3A444C"  // Ardesia antracite
        };

        public MainWindow()
        {
            InitializeComponent();

            // STEP 2: Imposta 500x800 come dimensione della finestra all'avvio
            AppWindow.Resize(new SizeInt32(500, 600));

            // Lettura e popolamento grafico all'avvio
            CaricaDatiAllAvvio();
        }

        /// <summary>
        /// Legge il file CSV, esegue il parsing dinamico e aggiorna l'interfaccia.
        /// </summary>
        private void CaricaDatiAllAvvio()
        {
            _rilevazioni.Clear();

            string percorsoRisolto = RisolviPercorsoFile(_csvFilePath);

            if (!File.Exists(percorsoRisolto))
            {
                StatusInfoBar.Severity = InfoBarSeverity.Warning;
                StatusInfoBar.Title = "File CSV non trovato";
                StatusInfoBar.Message = $"Percorso cercato: {percorsoRisolto}";
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

                    // Divide per virgola
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
                            // Campo della mappa depositi (lunghezza e nomi dinamici)
                            if (int.TryParse(valore, NumberStyles.Integer, CultureInfo.InvariantCulture, out int importo))
                            {
                                rilevazione.Depositi[chiave] = importo;
                            }
                        }
                    }

                    _rilevazioni.Add(rilevazione);
                }

                AggiornaInterfacciaGrafica();
            }
            catch (Exception ex)
            {
                StatusInfoBar.Severity = InfoBarSeverity.Error;
                StatusInfoBar.Title = "Errore durante il parsing del file";
                StatusInfoBar.Message = ex.Message;
                StatusInfoBar.IsOpen = true;
            }
        }

        /// <summary>
        /// Popola l'interfaccia con i dati dell'ultima rilevazione secondo il mockup grafico.
        /// </summary>
        private void AggiornaInterfacciaGrafica()
        {
            DepositiGrid.Children.Clear();
            DepositiGrid.RowDefinitions.Clear();

            var ultima = _rilevazioni.LastOrDefault();
            if (ultima == null)
            {
                TxtDataHeader.Text = "00/00/0000";
                TxtTotaleValore.Text = "0€";
                return;
            }

            // 1. Data nell'header
            TxtDataHeader.Text = ultima.Data.ToString("dd/MM/yyyy");

            // 2. Griglia dei depositi a 2 colonne con palette rotativa
            int index = 0;
            foreach (var deposito in ultima.Depositi)
            {
                int riga = index / 2;
                int colonna = index % 2;

                // Aggiunge una riga alla griglia se non ancora presente
                while (DepositiGrid.RowDefinitions.Count <= riga)
                {
                    DepositiGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }

                // Colore del box dalla palette a rotazione (massimo 8 colori desaturati)
                string hexColore = PaletteColori[index % PaletteColori.Length];
                Color colore = HexToColor(hexColore);

                var box = CreaBoxDeposito(deposito.Key, deposito.Value, colore);

                Grid.SetRow(box, riga);
                Grid.SetColumn(box, colonna);
                DepositiGrid.Children.Add(box);

                index++;
            }

            // 3. Valore del Totale
            TxtTotaleValore.Text = $"{ultima.Totale:N0}€";
        }

        /// <summary>
        /// Crea un box visivo arrotondato per un deposito, conforme al mockup:
        /// nome in alto a sinistra, valore grande in basso a destra.
        /// </summary>
        private static Border CreaBoxDeposito(string nome, int importo, Color coloreSfondo)
        {
            var border = new Border
            {
                Height = 88,
                CornerRadius = new CornerRadius(12),
                Background = new SolidColorBrush(coloreSfondo),
                Padding = new Thickness(14, 10, 14, 10)
            };

            var gridInterno = new Grid();

            // Nome del deposito (in alto a sinistra)
            var txtNome = new TextBlock
            {
                Text = nome,
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            // Importo (in basso a destra)
            var txtImporto = new TextBlock
            {
                Text = $"{importo:N0}€",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            gridInterno.Children.Add(txtNome);
            gridInterno.Children.Add(txtImporto);

            border.Child = gridInterno;
            return border;
        }

        /// <summary>
        /// Converte una stringa esadecimale #RRGGBB in Color Windows.UI.
        /// </summary>
        private static Color HexToColor(string hex)
        {
            hex = hex.TrimStart('#');
            byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            return Color.FromArgb(255, r, g, b);
        }

        /// <summary>
        /// Risolve il percorso del file gestendo ambienti di sviluppo e di rilascio.
        /// </summary>
        private static string RisolviPercorsoFile(string path)
        {
            if (Path.IsPathRooted(path) && File.Exists(path))
                return path;

            string inBase = Path.Combine(AppContext.BaseDirectory, path);
            if (File.Exists(inBase))
                return inBase;

            if (File.Exists(path))
                return Path.GetFullPath(path);

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
