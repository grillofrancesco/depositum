using Depositum.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Linq;

namespace Depositum
{
    public sealed partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            AggiornaInterfacciaGrafica();
        }

        /// <summary>
        /// Popola la dashboard con i dati dell'ultima rilevazione presente in DataService.
        /// Se non è presente alcuna rilevazione, nasconde la data e mostra lo stato vuoto.
        /// </summary>
        private void AggiornaInterfacciaGrafica()
        {
            var ultima = DataService.UltimaRilevazione;
            if (ultima == null || ultima.Depositi.Count == 0)
            {
                // Stato vuoto: nascondi la data nell'header
                DepositiRepeater.ItemsSource = null;
                TxtDataHeader.Visibility = Visibility.Collapsed;
                TxtTotaleValore.Text = Formati.Importo(0);
                DepositiRepeater.Visibility = Visibility.Collapsed;
                EmptyStateContainer.Visibility = Visibility.Visible;
                return;
            }

            // Presenza di dati: mostra la data nell'header e la griglia
            TxtDataHeader.Visibility = Visibility.Visible;
            TxtDataHeader.Text = Formati.Data(ultima.Data);
            EmptyStateContainer.Visibility = Visibility.Collapsed;
            DepositiRepeater.Visibility = Visibility.Visible;

            // Una card per deposito; il colore dipende dalla posizione nella rilevazione.
            DepositiRepeater.ItemsSource = ultima.Depositi
                .Select((deposito, indice) => new DepositoCard(
                    deposito.Key,
                    Formati.Importo(deposito.Value),
                    Tema.PennelloPerDeposito(indice)))
                .ToList();

            TxtTotaleValore.Text = Formati.Importo(ultima.Totale);
        }

        private void BtnRegistraDepositi_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RegistraPage));
        }

        private void BtnOsservaNelTempo_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AnalysisPage));
        }
    }
}
