using Depositum.Core;
using Depositum.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Depositum.Charts
{
    /// <summary>Tipi di grafico offerti dalla pagina di analisi.</summary>
    public enum TipoGrafico { Istogramma, Linee, Radiale }

    /// <summary>
    /// Costruisce i tre grafici della pagina di analisi a partire da dati già pronti.
    /// Qui c'è solo l'aspetto (serie, assi, colori presi da <see cref="Tema"/>); la scelta di
    /// cosa mostrare è in AnalysisPage e i calcoli sono in <see cref="DatiGrafici"/>.
    /// </summary>
    internal static class CostruttoreGrafici
    {
        /// <summary>Una barra per ogni deposito della rilevazione (che deve averne almeno uno).</summary>
        /// <param name="indiciColore">Indice di colore di ogni deposito, da <see cref="Storico.IndiciColore"/>.</param>
        public static void Istogramma(CartesianChart grafico, Rilevazione rilevazione, IReadOnlyDictionary<string, int> indiciColore)
        {
            var dati = rilevazione.Depositi.Select(kv => new ValoreDeposito(kv.Key, kv.Value)).ToArray();
            var series = new ColumnSeries<ValoreDeposito>
            {
                Values = dati,
                Name = "Depositi",
                Mapping = (item, index) => new(index, item.Valore),
                YToolTipLabelFormatter = point => point.Model is null ? string.Empty : Formati.Importo(point.Model.Valore),
                Fill = new SolidColorPaint(Tema.ColoreGraficoPerDeposito(indiciColore[dati[0].Nome])),
                MaxBarWidth = 38
            };
            // La serie ha un solo colore: ogni barra riceve il proprio quando viene misurata.
            series.PointMeasured += point =>
            {
                if (point.Model is { } item && point.Visual is { } visual)
                {
                    int index = Array.FindIndex(dati, d => ReferenceEquals(d, item));
                    if (index >= 0)
                        visual.Fill = new SolidColorPaint(Tema.ColoreGraficoPerDeposito(indiciColore[item.Nome]));
                }
            };

            var (minimo, massimo) = DatiGrafici.LimitiIstogramma(dati.Select(d => d.Valore).ToArray());

            grafico.Series = new ISeries[] { series };
            grafico.XAxes = new[] { new Axis { Labels = dati.Select(d => d.Nome).ToArray(), LabelsRotation = 0 } };
            grafico.YAxes = new[] { new Axis { Labeler = Formati.Importo, MinLimit = minimo, MaxLimit = massimo } };
        }

        /// <summary>Una linea per ogni deposito, con le date sull'asse orizzontale.</summary>
        public static void Linee(CartesianChart grafico, DatiLinee dati)
        {
            double[] dateOa = dati.Date.Select(InAscissa).ToArray();

            var series = new List<ISeries>();
            foreach (SerieDeposito serieDeposito in dati.Serie)
            {
                string nome = serieDeposito.Nome;
                var punti = serieDeposito.Valori
                    .Select((valore, indice) => valore is int v ? new ObservablePoint(dateOa[indice], v) : null)
                    .ToArray();

                SKColor colore = Tema.ColoreGraficoPerDeposito(serieDeposito.IndiceColore);
                series.Add(new LineSeries<ObservablePoint?>
                {
                    Name = nome,
                    Values = punti,
                    Fill = null,
                    GeometrySize = 9,
                    Stroke = new SolidColorPaint(colore, 3),
                    GeometryFill = new SolidColorPaint(colore),
                    YToolTipLabelFormatter = p => p.Model is { X: double, Y: double y }
                        ? Formati.Importo(y)
                        : nome
                });
            }

            grafico.Series = series;
            grafico.XAxes = new[]
            {
                new Axis
                {
                    Labeler = _ => string.Empty,
                    CustomSeparators = dateOa.Distinct().ToArray()
                }
            };
            grafico.YAxes = new[]
            {
                new Axis
                {
                    Labeler = Formati.Importo,
                    SeparatorsPaint = new SolidColorPaint(Tema.GrigliaGrafico),
                    ZeroPaint = new SolidColorPaint(Tema.GrigliaGrafico),
                    MinLimit = dati.MinimoAsse,
                    MaxLimit = dati.MassimoAsse
                }
            };
            // Riserva 12 px dentro l'area del grafico a destra e in basso.
            // I lati degli assi restano automatici per mantenere l'allineamento.
            grafico.DrawMargin = new LiveChartsCore.Measure.Margin(
                LiveChartsCore.Measure.Margin.Auto,
                LiveChartsCore.Measure.Margin.Auto,
                12,
                24);
        }

        /// <summary>Una fetta per ogni deposito della rilevazione (la cui somma deve essere positiva).</summary>
        /// <param name="indiciColore">Indice di colore di ogni deposito, da <see cref="Storico.IndiciColore"/>.</param>
        public static void Radiale(PieChart grafico, Rilevazione rilevazione, bool mostraEtichette, IReadOnlyDictionary<string, int> indiciColore)
        {
            long sumTotale = rilevazione.SommaDepositi();
            // Il grafico vuole un solo nome di font: il primo della famiglia con grazie.
            string fontSerif = ((FontFamily)Application.Current.Resources["SerifFontFamily"]).Source.Split(',')[0].Trim();

            grafico.Series = rilevazione.Depositi.Select(kv => (ISeries)new PieSeries<int>
            {
                Name = kv.Key,
                Values = new[] { kv.Value },
                Fill = new SolidColorPaint(Tema.ColoreGraficoPerDeposito(indiciColore[kv.Key])),
                Pushout = 5,
                HoverPushout = 15,
                ShowDataLabels = mostraEtichette,
                DataLabelsFormatter = _ => mostraEtichette ? kv.Key : string.Empty,
                DataLabelsPaint = new SolidColorPaint(Tema.EtichetteGrafico)
                {
                    SKTypeface = SKTypeface.FromFamilyName(fontSerif, SKFontStyle.Bold)
                },
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsSize = 22,
                ToolTipLabelFormatter = point => $"{Formati.Importo(point.Coordinate.PrimaryValue)} ({(point.Coordinate.PrimaryValue / sumTotale):P1})"
            }).ToArray();
        }

        /// <summary>Posizione di una data sull'asse orizzontale.</summary>
        private static double InAscissa(DateOnly data) => data.ToDateTime(TimeOnly.MinValue).ToOADate();

        private sealed record ValoreDeposito(string Nome, int Valore);
    }
}
