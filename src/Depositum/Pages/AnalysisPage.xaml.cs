using Depositum.Charts;
using Depositum.Core;
using Depositum.Dialogs;
using Depositum.Services;
using Depositum.Utilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Depositum.Pages
{
    /// <summary>
    /// Pagina dei grafici. Tiene le scelte dell'utente (grafico, intervallo, depositi visibili)
    /// e decide cosa mostrare; i calcoli sono in <see cref="DatiGrafici"/>, l'aspetto dei grafici
    /// in <see cref="CostruttoreGrafici"/>, le opzioni nei dialoghi della cartella Dialogs.
    /// </summary>
    public sealed partial class AnalysisPage : Page
    {
        private TipoGrafico _grafico = TipoGrafico.Istogramma;
        private FinestraTemporale _finestra = FinestraTemporale.Tutto;
        private bool _mostraEtichetteRadiale = true;
        private readonly HashSet<string> _depositiVisibili = new(StringComparer.OrdinalIgnoreCase);
        private bool _depositiInizializzati;

        public AnalysisPage()
        {
            InitializeComponent();
            Loaded += (_, _) => Tema.TemaCambiato += Tema_TemaCambiato;
            Unloaded += (_, _) => Tema.TemaCambiato -= Tema_TemaCambiato;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _depositiInizializzati = false;
            AggiornaGrafico();
        }

        // I colori dei grafici non sono pennelli XAML: al cambio di tema vanno ridisegnati.
        private void Tema_TemaCambiato(object? sender, EventArgs e) => AggiornaGrafico();

        private void BtnIndietro_Click(object sender, RoutedEventArgs e)
        {
            Frame.TornaIndietro();
        }

        private void BtnGrafico_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: TipoGrafico tipo }) return;
            _grafico = tipo;
            AggiornaGrafico();
        }

        private async void BtnOpzioni_Click(object sender, RoutedEventArgs e)
        {
            switch (_grafico)
            {
                case TipoGrafico.Radiale:
                    var opzioniRadiale = new OpzioniRadialeDialog
                    {
                        XamlRoot = XamlRoot,
                        MostraEtichette = _mostraEtichetteRadiale
                    };
                    await opzioniRadiale.ShowAsync();
                    _mostraEtichetteRadiale = opzioniRadiale.MostraEtichette;
                    AggiornaGrafico();
                    break;

                case TipoGrafico.Linee:
                    var nomi = DatiGrafici.NomiDepositi(DataService.Rilevazioni);
                    InizializzaDepositiVisibili(nomi);

                    var opzioniLinee = new OpzioniLineeDialog(_finestra, nomi, _depositiVisibili) { XamlRoot = XamlRoot };
                    // Il grafico dietro al dialogo si aggiorna a ogni scelta.
                    opzioniLinee.OpzioniCambiate += (_, _) =>
                    {
                        _finestra = opzioniLinee.Finestra;
                        AggiornaGrafico();
                    };
                    await opzioniLinee.ShowAsync();
                    break;
            }
        }

        private void AggiornaGrafico()
        {
            BtnIstogramma.IsEnabled = _grafico != TipoGrafico.Istogramma;
            BtnLinee.IsEnabled = _grafico != TipoGrafico.Linee;
            BtnRadiale.IsEnabled = _grafico != TipoGrafico.Radiale;
            BtnOpzioni.Visibility = Visibility.Visible;
            BtnOpzioni.IsEnabled = _grafico is TipoGrafico.Linee or TipoGrafico.Radiale;
            CartesianChart.Visibility = Visibility.Collapsed;
            CartesianChart.DrawMargin = null;
            PieChart.Visibility = Visibility.Collapsed;
            TxtCanvasVuoto.Visibility = Visibility.Visible;

            var rilevazioni = Storico.OrdinatePerData(DataService.Rilevazioni);
            if (rilevazioni.Count == 0)
            {
                TxtCanvasVuoto.Text = "Nessuna rilevazione disponibile. Registra i depositi per visualizzare i grafici.";
                return;
            }

            var indiciColore = Storico.IndiciColore(rilevazioni);
            switch (_grafico)
            {
                case TipoGrafico.Istogramma:
                    MostraIstogramma(rilevazioni[^1], indiciColore);
                    break;
                case TipoGrafico.Linee:
                    MostraLinee(rilevazioni);
                    break;
                case TipoGrafico.Radiale:
                    MostraRadiale(rilevazioni[^1], indiciColore);
                    break;
            }
        }

        private void MostraIstogramma(Rilevazione ultima, IReadOnlyDictionary<string, int> indiciColore)
        {
            if (ultima.Depositi.Count == 0) return;

            CostruttoreGrafici.Istogramma(CartesianChart, ultima, indiciColore);
            Mostra(CartesianChart);
        }

        private void MostraLinee(List<Rilevazione> rilevazioni)
        {
            InizializzaDepositiVisibili(DatiGrafici.NomiDepositi(rilevazioni));

            DatiLinee? dati = DatiGrafici.PreparaLinee(rilevazioni, _finestra, _depositiVisibili);
            if (dati is null)
            {
                TxtCanvasVuoto.Text = "Nessun deposito selezionato per questo intervallo.";
                return;
            }

            CostruttoreGrafici.Linee(CartesianChart, dati);
            Mostra(CartesianChart);
        }

        private void MostraRadiale(Rilevazione ultima, IReadOnlyDictionary<string, int> indiciColore)
        {
            if (ultima.Depositi.Count == 0 || ultima.SommaDepositi() <= 0) return;

            CostruttoreGrafici.Radiale(PieChart, ultima, _mostraEtichetteRadiale, indiciColore);
            Mostra(PieChart);
        }

        /// <summary>Rende visibile il grafico indicato al posto del messaggio "nessun dato".</summary>
        private void Mostra(UIElement grafico)
        {
            grafico.Visibility = Visibility.Visible;
            TxtCanvasVuoto.Visibility = Visibility.Collapsed;
        }

        /// <summary>Alla prima occasione dopo l'apertura della pagina, tutti i depositi sono visibili.</summary>
        private void InizializzaDepositiVisibili(IEnumerable<string> nomi)
        {
            if (_depositiInizializzati) return;

            _depositiVisibili.UnionWith(nomi);
            _depositiInizializzati = true;
        }
    }
}
