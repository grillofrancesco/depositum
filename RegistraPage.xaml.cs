using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
        /// Apre un dialogo modale per inserire un nuovo deposito (Nome e Valore).
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
                    PlaceholderText = "Nome deposito"
                };

                var numValore = new NumberBox
                {
                    PlaceholderText = "Importo iniziale",
                    Value = 0,
                    SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Hidden,
                    HorizontalContentAlignment = HorizontalAlignment.Right
                };

                sp.Children.Add(new TextBlock { Text = "Nome deposito:", FontWeight = FontWeights.SemiBold });
                sp.Children.Add(txtNome);
                sp.Children.Add(new TextBlock { Text = "Importo (€):", FontWeight = FontWeights.SemiBold });
                sp.Children.Add(numValore);

                dialog.Content = sp;

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Secondary)
                {
                    string nome = txtNome.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(nome))
                    {
                        var esistente = _depositi.FirstOrDefault(d => string.Equals(d.Nome, nome, StringComparison.OrdinalIgnoreCase));
                        if (esistente != null)
                        {
                            esistente.Valore = double.IsNaN(numValore.Value) ? 0 : numValore.Value;
                        }
                        else
                        {
                            double valore = double.IsNaN(numValore.Value) ? 0 : numValore.Value;
                            _depositi.Add(new DepositoEditItem
                            {
                                Nome = nome,
                                Valore = valore
                            });
                        }
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
        /// Valida la data, calcola il totale, salva la rilevazione in AppData e torna alla schermata principale.
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

            // Calcolo automatico del Totale come somma di tutti i depositi
            int totaleCalcolato = _depositi.Sum(d => (int)Math.Round(d.Valore));

            var nuovaRilevazione = new Rilevazione
            {
                Data = dataParsed,
                Totale = totaleCalcolato
            };

            foreach (var d in _depositi)
            {
                nuovaRilevazione.Depositi[d.Nome] = (int)Math.Round(d.Valore);
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
