using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.UI;

namespace codebase
{
    public sealed partial class AnalysisPage : Page
    {
        private enum TipoGrafico { Istogramma, Linee, Radiale }
        private enum FinestraTemporale { Tutto, UnMese, SeiMesi, UnAnno, CinqueAnni }

        private TipoGrafico _grafico = TipoGrafico.Istogramma;
        private FinestraTemporale _finestra = FinestraTemporale.Tutto;
        private bool _mostraLabelRadiale = true;
        private readonly HashSet<string> _depositiVisibili = new(StringComparer.OrdinalIgnoreCase);
        private bool _depositiInizializzati;

        private float _MAX_HEIGHT_MULTIPLIER = 1.6f;

        public AnalysisPage()
        {
            InitializeComponent();
            Loaded += (_, _) => AggiornaGrafico();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _depositiInizializzati = false;
            AggiornaGrafico();
        }

        private void BtnIndietro_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
            else
                Frame.Navigate(typeof(DashboardPage));
        }

        private void BtnGrafico_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: string nome }) return;
            _grafico = nome switch
            {
                "Linee" => TipoGrafico.Linee,
                "Radiale" => TipoGrafico.Radiale,
                _ => TipoGrafico.Istogramma
            };
            AggiornaGrafico();
        }

        private async void BtnOpzioni_Click(object sender, RoutedEventArgs e)
        {
            if (_grafico == TipoGrafico.Radiale)
            {
                var checkLabel = new CheckBox
                {
                    Content = "Mostra etichette dei depositi",
                    IsChecked = _mostraLabelRadiale
                };
                var dialogRadiale = new ContentDialog
                {
                    XamlRoot = XamlRoot,
                    Title = "Opzioni del grafico radiale",
                    Content = checkLabel,
                    CloseButtonText = "Chiudi"
                };
                await dialogRadiale.ShowAsync();
                _mostraLabelRadiale = checkLabel.IsChecked == true;
                AggiornaGrafico();
                return;
            }

            if (_grafico != TipoGrafico.Linee)
                return;

            var panel = new StackPanel { Spacing = 10, MinWidth = 260 };
            panel.Children.Add(new TextBlock { Text = "Intervallo", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            var intervallo = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch };
            intervallo.Items.Add(new ComboBoxItem { Content = "Tutto", Tag = FinestraTemporale.Tutto });
            intervallo.Items.Add(new ComboBoxItem { Content = "1 mese", Tag = FinestraTemporale.UnMese });
            intervallo.Items.Add(new ComboBoxItem { Content = "6 mesi", Tag = FinestraTemporale.SeiMesi });
            intervallo.Items.Add(new ComboBoxItem { Content = "1 anno", Tag = FinestraTemporale.UnAnno });
            intervallo.Items.Add(new ComboBoxItem { Content = "5 anni", Tag = FinestraTemporale.CinqueAnni });
            intervallo.SelectedIndex = (int)_finestra;
            intervallo.SelectionChanged += (_, _) =>
            {
                if (intervallo.SelectedItem is ComboBoxItem { Tag: FinestraTemporale selezione })
                {
                    _finestra = selezione;
                    AggiornaGrafico();
                }
            };
            panel.Children.Add(intervallo);
            panel.Children.Add(new TextBlock { Text = "Depositi visibili", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 4, 0, 0) });

            var nomi = DataService.Rilevazioni.SelectMany(r => r.Depositi.Keys)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (!_depositiInizializzati)
            {
                _depositiVisibili.UnionWith(nomi);
                _depositiInizializzati = true;
            }

            foreach (string nome in nomi)
            {
                var check = new CheckBox { Content = nome, IsChecked = _depositiVisibili.Contains(nome) };
                check.Checked += (_, _) => { _depositiVisibili.Add(nome); AggiornaGrafico(); };
                check.Unchecked += (_, _) => { _depositiVisibili.Remove(nome); AggiornaGrafico(); };
                panel.Children.Add(check);
            }

            var dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = "Opzioni del grafico a linee",
                Content = new ScrollViewer { Content = panel, MaxHeight = 420 },
                CloseButtonText = "Chiudi"
            };
            await dialog.ShowAsync();
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

            var rilevazioni = DataService.Rilevazioni.OrderBy(r => r.Data).ToList();
            if (rilevazioni.Count == 0)
            {
                TxtCanvasVuoto.Text = "Nessuna rilevazione disponibile. Registra i depositi per visualizzare i grafici.";
                return;
            }

            switch (_grafico)
            {
                case TipoGrafico.Istogramma:
                    MostraIstogramma(rilevazioni[^1]);
                    break;
                case TipoGrafico.Linee:
                    MostraLinee(rilevazioni);
                    break;
                case TipoGrafico.Radiale:
                    MostraRadiale(rilevazioni[^1]);
                    break;
            }
        }

        private void MostraIstogramma(Rilevazione ultima)
        {
            if (ultima.Depositi.Count == 0) return;
            var dati = ultima.Depositi.Select(kv => new ValoreDeposito(kv.Key, kv.Value)).ToArray();
            var series = new ColumnSeries<ValoreDeposito>
            {
                Values = dati,
                Name = "Depositi",
                Mapping = (item, index) => new(index, item.Valore),
                YToolTipLabelFormatter = point => point.Model is null ? string.Empty : $"{point.Model.Valore:N0} €",
                Fill = new SolidColorPaint(ToSkColor(PaletteDepositi.ColoriHex[0])),
                MaxBarWidth = 38
            };
            series.PointMeasured += point =>
            {
                if (point.Model is { } item && point.Visual is { } visual)
                {
                    int index = Array.FindIndex(dati, d => ReferenceEquals(d, item));
                    if (index >= 0)
                        visual.Fill = new SolidColorPaint(ToSkColor(PaletteDepositi.ColoriHex[index % PaletteDepositi.ColoriHex.Count]));
                }
            };
            CartesianChart.Series = new ISeries[] { series };
            CartesianChart.XAxes = new[] { new Axis { Labels = dati.Select(d => d.Nome).ToArray(), LabelsRotation = 0 } };
            CartesianChart.YAxes = new[] { new Axis { Labeler = value => $"{value:N0} €", 
                MinLimit = Math.Min(0, dati.Min(d => d.Valore)), 
                MaxLimit = Math.Max(1, dati.Max(d => d.Valore)) * _MAX_HEIGHT_MULTIPLIER } };
            CartesianChart.Visibility = Visibility.Visible;
            TxtCanvasVuoto.Visibility = Visibility.Collapsed;
        }

        private void MostraLinee(List<Rilevazione> rilevazioni)
        {
            // Aggiunge spazio all'interno dell'area di tracciamento (nera), senza
            // spostare o ridimensionare il controllo rispetto al wrapper.
            CartesianChart.DrawMargin = new LiveChartsCore.Measure.Margin(
                LiveChartsCore.Measure.Margin.Auto,
                LiveChartsCore.Measure.Margin.Auto,
                12,
                12);
            DateOnly? inizio = _finestra switch
            {
                FinestraTemporale.UnMese => rilevazioni[^1].Data.AddMonths(-1),
                FinestraTemporale.SeiMesi => rilevazioni[^1].Data.AddMonths(-6),
                FinestraTemporale.UnAnno => rilevazioni[^1].Data.AddYears(-1),
                FinestraTemporale.CinqueAnni => rilevazioni[^1].Data.AddYears(-5),
                _ => null
            };
            var visibili = rilevazioni.Where(r => inizio is null || r.Data >= inizio.Value).ToList();
            if (visibili.Count == 0) return;

            var nomi = rilevazioni.SelectMany(r => r.Depositi.Keys)
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (!_depositiInizializzati)
            {
                _depositiVisibili.UnionWith(nomi);
                _depositiInizializzati = true;
            }

            var series = new List<ISeries>();
            int colorIndex = 0;
            foreach (string nome in nomi)
            {
                if (!_depositiVisibili.Contains(nome)) { colorIndex++; continue; }
                var punti = visibili.Select(r => r.Depositi.TryGetValue(nome, out int valore)
                    ? new ObservablePoint(r.Data.ToDateTime(TimeOnly.MinValue).ToOADate(), valore)
                    : null).ToArray();
                if (punti.All(p => p is null)) { colorIndex++; continue; }

                string colore = PaletteDepositi.ColoriHex[colorIndex++ % PaletteDepositi.ColoriHex.Count];
                var serie = new LineSeries<ObservablePoint?>
                {
                    Name = nome,
                    Values = punti,
                    Fill = null,
                    GeometrySize = 9,
                    Stroke = new SolidColorPaint(ToSkColor(colore), 3),
                    GeometryFill = new SolidColorPaint(ToSkColor(colore)),
                    YToolTipLabelFormatter = p => p.Model is { X: double x, Y: double y }
                        ? $"{y.ToString("N0", CultureInfo.CurrentCulture)} €"
                        : nome
                };
                series.Add(serie);
            }

            var valori = visibili.SelectMany(r => r.Depositi.Where(kv => _depositiVisibili.Contains(kv.Key)).Select(kv => kv.Value)).ToList();
            if (series.Count == 0 || valori.Count == 0)
            {
                TxtCanvasVuoto.Text = "Nessun deposito selezionato per questo intervallo.";
                return;
            }

            CartesianChart.Series = series;
            CartesianChart.XAxes = new[]
            {
                new Axis
                {
                    Labeler = _ => string.Empty,
                    CustomSeparators = visibili.Select(r => r.Data.ToDateTime(TimeOnly.MinValue).ToOADate()).Distinct().ToArray()
                }
            };
            var coloreGriglia = new SolidColorPaint(new SKColor(90, 90, 90));
            CartesianChart.YAxes = new[] { new Axis { Labeler = value => $"{value:N0} €",
                SeparatorsPaint = coloreGriglia,
                ZeroPaint = new SolidColorPaint(new SKColor(90, 90, 90)),
                MinLimit = Math.Min(0, valori.Min()), 
                MaxLimit = Math.Max(1, valori.Max() * _MAX_HEIGHT_MULTIPLIER) } };
            CartesianChart.Visibility = Visibility.Visible;
            TxtCanvasVuoto.Visibility = Visibility.Collapsed;
        }

        private void MostraRadiale(Rilevazione ultima)
        {
            if (ultima.Depositi.Count == 0 || ultima.Depositi.Values.Sum() <= 0) return;
            int colorIndex = 0;
            PieChart.Series = ultima.Depositi.Select(kv => (ISeries)new PieSeries<int>
            {
                Name = kv.Key,
                Values = new[] { kv.Value },
                Fill = new SolidColorPaint(ToSkColor(PaletteDepositi.ColoriHex[colorIndex++ % PaletteDepositi.ColoriHex.Count])),
                Pushout = 2,
                DataLabelsFormatter = _ => _mostraLabelRadiale ? kv.Key : string.Empty,
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsSize = 14,
                ToolTipLabelFormatter = point => $"{point.Coordinate.PrimaryValue:N0} € ({(point.Coordinate.PrimaryValue / ultima.Depositi.Values.Sum()):P1})"
            }).ToArray();
            PieChart.Visibility = Visibility.Visible;
            TxtCanvasVuoto.Visibility = Visibility.Collapsed;
        }

        private static SKColor ToSkColor(string hex) => SKColor.Parse(hex);

        private sealed record ValoreDeposito(string Nome, int Valore);
    }
}
