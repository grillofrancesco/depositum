using Depositum.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Depositum
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

        /// <summary>
        /// Accetta nella lista solo importi validi: un valore non valido viene scartato
        /// e la casella torna a mostrare l'ultimo importo buono.
        /// </summary>
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
            if (RegoleDeposito.ImportoValido(valore))
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
            TxtDataInput.Text = Formati.Data(DateOnly.FromDateTime(DateTime.Today));

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
                    MostraMessaggio(InfoBarSeverity.Error, "Errore durante la rimozione", ex.Message);
                }
            }
        }

        /// <summary>
        /// Apre il dialogo per inserire un nuovo deposito e ne applica il risultato alla lista.
        /// Se esiste già un deposito con lo stesso nome, ne aggiorna l'importo.
        /// </summary>
        private async void BtnAggiungiDeposito_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var xamlRoot = (sender as FrameworkElement)?.XamlRoot ?? this.XamlRoot;
                if (xamlRoot == null) return;

                var dialog = new AggiungiDepositoDialog { XamlRoot = xamlRoot };
                if (await dialog.ShowAsync() != ContentDialogResult.Secondary)
                    return;

                string nome = dialog.NomeDeposito;
                int valoreIntero = dialog.Importo;

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
            catch (Exception ex)
            {
                MostraMessaggio(InfoBarSeverity.Error, "Errore durante l'apertura del dialogo", ex.Message);
            }
        }

        /// <summary>
        /// Annulla l'operazione e torna alla schermata principale senza salvare.
        /// </summary>
        private void BtnAnnulla_Click(object sender, RoutedEventArgs e)
        {
            Frame.TornaIndietro();
        }

        /// <summary>
        /// Valida la data e tutti i depositi, calcola il totale, salva la rilevazione e torna alla schermata principale.
        /// </summary>
        private void BtnConferma_Click(object sender, RoutedEventArgs e)
        {
            string dataStr = TxtDataInput.Text.Trim();

            if (!Formati.ProvaALeggereData(dataStr, out var dataParsed))
            {
                MostraMessaggio(InfoBarSeverity.Error, "Data non valida", "Inserisci una data valida nel formato gg/mm/aaaa.");
                return;
            }

            if (_depositi.Count == 0)
            {
                MostraMessaggio(InfoBarSeverity.Warning, "Nessun deposito", "Aggiungi almeno un deposito prima di confermare.");
                return;
            }

            // Validazione su tutti gli elementi della lista prima del salvataggio definitivo
            foreach (var d in _depositi)
            {
                if (!RegoleDeposito.NomeValido(d.Nome))
                {
                    MostraMessaggio(InfoBarSeverity.Error, "Nome deposito non valido",
                        $"Il deposito \"{d.Nome}\" deve avere un nome compreso tra 1 e {RegoleDeposito.LunghezzaMassimaNome} caratteri.");
                    return;
                }

                if (!RegoleDeposito.ImportoValido(d.Valore))
                {
                    MostraMessaggio(InfoBarSeverity.Error, "Importo non valido",
                        $"L'importo del deposito \"{d.Nome}\" deve essere un intero compreso tra {RegoleDeposito.ImportoMinimo} e {Formati.ImportoGrafico(RegoleDeposito.ImportoMassimo)}.");
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
            Frame.TornaIndietro();
        }

        private void MostraMessaggio(InfoBarSeverity gravita, string titolo, string messaggio)
        {
            StatusInfoBar.Severity = gravita;
            StatusInfoBar.Title = titolo;
            StatusInfoBar.Message = messaggio;
            StatusInfoBar.IsOpen = true;
        }
    }
}
