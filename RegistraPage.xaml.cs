using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace codebase
{
    public sealed partial class RegistraPage : Page
    {
        private readonly ObservableCollection<DepositoEditItem> _depositi = new();

        public RegistraPage()
        {
            InitializeComponent();
            ItemsDepositi.ItemsSource = _depositi;

            Loaded += (s, e) => InizializzaPagina();
        }

        private void NumberBoxValore_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (sender.DataContext is not DepositoEditItem item)
                return;

            if (!double.IsFinite(args.NewValue))
            {
                sender.Value = item.Valore;
                return;
            }

            int valore = (int)Math.Round(args.NewValue, MidpointRounding.AwayFromZero);
            if (valore >= 1 && valore <= 999_999_999)
            {
                item.Valore = valore;
                if (sender.Value != valore) sender.Value = valore;
            }
            else
            {
                sender.Value = item.Valore;
            }
        }

        private void InizializzaPagina()
        {
            // 1. Data precompilata con la data odierna
            TxtDataInput.Text = DateTime.Today.ToString("dd/MM/yyyy");

            // 2. Precompila la lista con i depositi dell'ultima rilevazione (se esiste)
            _depositi.Clear();
            var ultima = DataService.UltimaRilevazione;
            if (ultima != null && ultima.Depositi != null)
            {
                foreach (var dep in ultima.Depositi)
                {
                    _depositi.Add(new DepositoEditItem
                    {
                        Nome = dep.Key,
                        Valore = dep.Value
                    });
                }
            }
        }

        /// <summary>
        /// Rimuove un deposito dalla lista previa richiesta di conferma tramite ContentDialog.
        /// Ordine tasti: Annulla (sinistra) / Rimuovi (destra).
        /// </summary>
        private async void BtnRimuovi_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is DepositoEditItem item)
            {
                try
                {
                    var xamlRoot = btn.XamlRoot ?? this.XamlRoot;
                    if (xamlRoot == null) return;

                    var dialog = new ContentDialog
                    {
                        XamlRoot = xamlRoot,
                        Title = "Rimuovi deposito",
                        Content = $"Sei sicuro di voler rimuovere il deposito \"{item.Nome}\"?",
                        PrimaryButtonText = "Annulla",
                        SecondaryButtonText = "Rimuovi",
                        DefaultButton = ContentDialogButton.Secondary
                    };

                    if (Application.Current.Resources.TryGetValue("DefaultContentDialogStyle", out var styleObj) && styleObj is Style style)
                    {
                        dialog.Style = style;
                    }

                    var result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Secondary)
                    {
                        _depositi.Remove(item);
                    }
                }
                catch (Exception ex)
                {
                    StatusInfoBar.Severity = InfoBarSeverity.Error;
                    StatusInfoBar.Title = "Errore durante la rimozione";
                    StatusInfoBar.Message = ex.Message;
                    StatusInfoBar.IsOpen = true;
                }
            }
        }

        /// <summary>
        /// Apre un dialogo modale per inserire un nuovo deposito (Nome e Valore intero).
        /// Controlli:
        /// - Nome: non vuoto e massimo 30 caratteri.
        /// - Valore: numero intero con 0 &lt; v &lt; 1.000.000.000.
        /// Ordine tasti: Annulla (sinistra) / Aggiungi (destra).
        /// </summary>
        private async void BtnAggiungiDeposito_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var xamlRoot = (sender as FrameworkElement)?.XamlRoot ?? this.XamlRoot;
                if (xamlRoot == null) return;

                var dialog = new ContentDialog
                {
                    XamlRoot = xamlRoot,
                    Title = "Aggiungi Deposito",
                    PrimaryButtonText = "Annulla",
                    SecondaryButtonText = "Aggiungi",
                    DefaultButton = ContentDialogButton.Secondary
                };

                if (Application.Current.Resources.TryGetValue("DefaultContentDialogStyle", out var styleObj) && styleObj is Style style)
                {
                    dialog.Style = style;
                }

                var sp = new StackPanel { Spacing = 10, Width = 300 };

                var txtNome = new TextBox
                {
                    PlaceholderText = "Nome deposito (max 30 caratteri)",
                    MaxLength = 30
                };

                var numValore = new NumberBox
                {
                    PlaceholderText = "Importo iniziale (intero)",
                    Value = 0,
                    SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Hidden,
                    HorizontalContentAlignment = HorizontalAlignment.Right,
                    AcceptsExpression = false
                };

                var txtErrore = new TextBlock
                {
                    FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe UI"),
                    Foreground = new SolidColorBrush(Colors.Salmon),
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap,
                    Visibility = Visibility.Collapsed
                };

                sp.Children.Add(new TextBlock { Text = "Nome deposito:", FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe UI"), FontWeight = FontWeights.SemiBold });
                sp.Children.Add(txtNome);
                sp.Children.Add(new TextBlock { Text = "Importo (€, intero):", FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe UI"), FontWeight = FontWeights.SemiBold });
                sp.Children.Add(numValore);
                sp.Children.Add(txtErrore);

                dialog.Content = sp;

                // Validazione al click su 'Aggiungi' (SecondaryButton)
                dialog.SecondaryButtonClick += (d, args) =>
                {
                    string nome = txtNome.Text.Trim();

                    // Controllo nome vuoto o lunghezza > 30 caratteri
                    if (string.IsNullOrWhiteSpace(nome))
                    {
                        txtErrore.Text = "Il nome del deposito non può essere vuoto.";
                        txtErrore.Visibility = Visibility.Visible;
                        args.Cancel = true;
                        return;
                    }

                    if (nome.Length > 30)
                    {
                        txtErrore.Text = "La lunghezza del nome non deve superare 30 caratteri.";
                        txtErrore.Visibility = Visibility.Visible;
                        args.Cancel = true;
                        return;
                    }

                    // Controllo che il valore sia un numero intero e compreso tra 0 e 1.000.000.000 (esclusi)
                    double val = numValore.Value;
                    if (double.IsNaN(val) || val <= 0 || val >= 1_000_000_000 || val % 1 != 0)
                    {
                        txtErrore.Text = "L'importo deve essere un numero intero compreso tra 1 e 1mlrd";
                        txtErrore.Visibility = Visibility.Visible;
                        args.Cancel = true;
                        return;
                    }

                    txtErrore.Visibility = Visibility.Collapsed;
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Secondary)
                {
                    string nome = txtNome.Text.Trim();
                    int valoreIntero = (int)numValore.Value;

                    var esistente = _depositi.FirstOrDefault(d => string.Equals(d.Nome, nome, StringComparison.OrdinalIgnoreCase));
                    if (esistente != null)
                    {
                        esistente.Valore = valoreIntero;
                    }
                    else
                    {
                        _depositi.Add(new DepositoEditItem
                        {
                            Nome = nome,
                            Valore = valoreIntero
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                StatusInfoBar.Severity = InfoBarSeverity.Error;
                StatusInfoBar.Title = "Errore durante l'apertura del dialogo";
                StatusInfoBar.Message = ex.Message;
                StatusInfoBar.IsOpen = true;
            }
        }

        /// <summary>
        /// Annulla l'operazione e torna alla schermata principale senza salvare.
        /// </summary>
        private void BtnAnnulla_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                Frame.Navigate(typeof(DashboardPage));
            }
        }

        /// <summary>
        /// Valida la data e tutti i depositi, calcola il totale, salva la rilevazione e torna alla schermata principale.
        /// </summary>
        private void BtnConferma_Click(object sender, RoutedEventArgs e)
        {
            string dataStr = TxtDataInput.Text.Trim();

            if (!DateOnly.TryParseExact(dataStr, new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataParsed) &&
                !DateOnly.TryParse(dataStr, out dataParsed))
            {
                StatusInfoBar.Severity = InfoBarSeverity.Error;
                StatusInfoBar.Title = "Data non valida";
                StatusInfoBar.Message = "Inserisci una data valida nel formato gg/mm/aaaa.";
                StatusInfoBar.IsOpen = true;
                return;
            }

            if (_depositi.Count == 0)
            {
                StatusInfoBar.Severity = InfoBarSeverity.Warning;
                StatusInfoBar.Title = "Nessun deposito";
                StatusInfoBar.Message = "Aggiungi almeno un deposito prima di confermare.";
                StatusInfoBar.IsOpen = true;
                return;
            }

            // Validazione su tutti gli elementi della lista prima del salvataggio definitivo
            foreach (var d in _depositi)
            {
                string nome = d.Nome.Trim();
                if (string.IsNullOrWhiteSpace(nome) || nome.Length > 30)
                {
                    StatusInfoBar.Severity = InfoBarSeverity.Error;
                    StatusInfoBar.Title = "Nome deposito non valido";
                    StatusInfoBar.Message = $"Il deposito \"{d.Nome}\" deve avere un nome compreso tra 1 e 30 caratteri.";
                    StatusInfoBar.IsOpen = true;
                    return;
                }

                if (d.Valore <= 0 || d.Valore >= 1_000_000_000)
                {
                    StatusInfoBar.Severity = InfoBarSeverity.Error;
                    StatusInfoBar.Title = "Importo non valido";
                    StatusInfoBar.Message = $"L'importo del deposito \"{d.Nome}\" deve essere un intero compreso tra 1 e 999.999.999 €.";
                    StatusInfoBar.IsOpen = true;
                    return;
                }
            }

            // Calcolo automatico del Totale come somma diretta di interi (senza Math.Round)
            int totaleCalcolato = _depositi.Sum(d => d.Valore);

            var nuovaRilevazione = new Rilevazione
            {
                Data = dataParsed,
                Totale = totaleCalcolato
            };

            foreach (var d in _depositi)
            {
                nuovaRilevazione.Depositi[d.Nome] = d.Valore;
            }

            // Salvataggio su file CSV in AppData e in memoria
            DataService.AggiungiRilevazione(nuovaRilevazione);

            // Ritorno alla dashboard principale
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                Frame.Navigate(typeof(DashboardPage));
            }
        }
    }
}
