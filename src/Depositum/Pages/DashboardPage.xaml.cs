using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace Depositum
{
    public sealed partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            Loaded += (s, e) => AggiornaInterfacciaGrafica();
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
            DepositiGrid.Children.Clear();
            DepositiGrid.RowDefinitions.Clear();

            var ultima = DataService.UltimaRilevazione;
            if (ultima == null || ultima.Depositi.Count == 0)
            {
                // Stato vuoto: nascondi la data nell'header
                TxtDataHeader.Visibility = Visibility.Collapsed;
                TxtTotaleValore.Text = "0€";
                DepositiGrid.Visibility = Visibility.Collapsed;
                EmptyStateContainer.Visibility = Visibility.Visible;
                return;
            }

            // Presenza di dati: mostra la data nell'header e la griglia
            TxtDataHeader.Visibility = Visibility.Visible;
            TxtDataHeader.Text = ultima.Data.ToString("dd/MM/yyyy");
            EmptyStateContainer.Visibility = Visibility.Collapsed;
            DepositiGrid.Visibility = Visibility.Visible;

            int index = 0;
            foreach (var deposito in ultima.Depositi)
            {
                int riga = index / 2;
                int colonna = index % 2;

                while (DepositiGrid.RowDefinitions.Count <= riga)
                {
                    DepositiGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }

                var box = CreaBoxDeposito(deposito.Key, deposito.Value, PaletteDepositi.Pennello(index));
                Grid.SetRow(box, riga);
                Grid.SetColumn(box, colonna);
                DepositiGrid.Children.Add(box);

                index++;
            }

            TxtTotaleValore.Text = $"{ultima.Totale:N0}€";
        }

        private static Border CreaBoxDeposito(string nome, int importo, Brush sfondo)
        {
            var coloreTesto = (Brush)Application.Current.Resources["OnAccentBrush"];

            var border = new Border
            {
                Height = 88,
                CornerRadius = new CornerRadius(12),
                Background = sfondo,
                Padding = new Thickness(14, 10, 14, 10)
            };

            var gridInterno = new Grid();

            var txtNome = new TextBlock
            {
                Text = nome,
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = coloreTesto,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            var txtImporto = new TextBlock
            {
                Text = $"{importo:N0}€",
                FontSize = 32,
                FontWeight = FontWeights.Bold,
                Foreground = coloreTesto,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            gridInterno.Children.Add(txtNome);
            gridInterno.Children.Add(txtImporto);

            border.Child = gridInterno;
            return border;
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
