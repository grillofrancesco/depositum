using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Globalization;
using Windows.UI;

namespace codebase
{
    public sealed partial class DashboardPage : Page
    {
        private static readonly string[] PaletteColori = new[]
        {
            "#1A6B6B", // Teal desaturato
            "#206535", // Forest Green desaturato
            "#2B4A6F", // Slate Blue elegante
            "#733F35", // Terracotta / Ruggine morbido
            "#58395B", // Prugna / Melanzana profondo
            "#1C4E5E", // Blu petrolio
            "#644F2E", // Bronzo / Cuoio caldo
            "#3A444C"  // Ardesia antracite
        };

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

                string hexColore = PaletteColori[index % PaletteColori.Length];
                Color colore = HexToColor(hexColore);

                var box = CreaBoxDeposito(deposito.Key, deposito.Value, colore);
                Grid.SetRow(box, riga);
                Grid.SetColumn(box, colonna);
                DepositiGrid.Children.Add(box);

                index++;
            }

            TxtTotaleValore.Text = $"{ultima.Totale:N0}€";
        }

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

            var txtNome = new TextBlock
            {
                Text = nome,
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

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

        private static Color HexToColor(string hex)
        {
            hex = hex.TrimStart('#');
            byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            return Color.FromArgb(255, r, g, b);
        }

        private void BtnRegistraDepositi_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RegistraPage));
        }

        private void BtnOsservaNelTempo_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder per funzionalità future
        }
    }
}
