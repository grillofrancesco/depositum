using Depositum.Core.Temi;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI;

namespace Depositum.Services
{
    /// <summary>
    /// Unico punto da cui l'app ottiene i colori.
    /// <para>
    /// Lo XAML usa i pennelli dichiarati in Theme/Colors.xaml tramite la loro chiave;
    /// <see cref="Applica"/> scrive in quei pennelli i colori della palette scelta, e tutto
    /// ciò che li usa si aggiorna da solo, anche con le pagine già aperte. Il codice che non
    /// può usare un pennello (i grafici) legge i colori da qui e si ridisegna quando arriva
    /// <see cref="TemaCambiato"/>.
    /// </para>
    /// </summary>
    public static class Tema
    {
        private const string FileColori = "Theme/Colors.xaml";
        private const string ChiaveAccentoDiSistema = "SystemAccentColor";

        /// <summary>La palette attualmente applicata.</summary>
        public static Palette Corrente { get; private set; } = Palette.Predefinita;

        /// <summary>Sollevato dopo ogni <see cref="Applica"/>.</summary>
        public static event EventHandler? TemaCambiato;

        /// <summary>
        /// Scrive i colori della palette nei pennelli condivisi dell'app.
        /// All'avvio va chiamato in App.OnLaunched, prima di creare la finestra
        /// (nel costruttore di App le risorse non sono ancora accessibili).
        /// </summary>
        public static void Applica(Palette palette)
        {
            if (palette.Depositi.Count != Palette.NumeroColoriDeposito)
                throw new ArgumentException(
                    $"La palette '{palette.Nome}' ha {palette.Depositi.Count} colori per i depositi: ne servono {Palette.NumeroColoriDeposito}.",
                    nameof(palette));

            ResourceDictionary colori = TrovaDizionarioColori();
            var chiaviSistema = (ResourceDictionary)colori.ThemeDictionaries["Default"];

            var pennelliApp = PennelliApp(palette).ToList();
            var pennelliSistema = PennelliDiSistema(palette).ToList();

            foreach (var (chiave, colore) in pennelliApp)
                ((SolidColorBrush)colori[chiave]).Color = colore;

            foreach (var (chiave, colore) in pennelliSistema)
                ((SolidColorBrush)chiaviSistema[chiave]).Color = colore;

            chiaviSistema[ChiaveAccentoDiSistema] = InColore(palette.SystemAccent);

            VerificaCopertura(colori, pennelliApp.Select(p => p.Chiave));
            VerificaCopertura(chiaviSistema, pennelliSistema.Select(p => p.Chiave));

            Corrente = palette;
            TemaCambiato?.Invoke(null, EventArgs.Empty);
        }

        // ---------- Depositi ----------

        /// <summary>Pennello condiviso del deposito in posizione <paramref name="indice"/>; oltre l'ultimo i colori si ripetono.</summary>
        public static SolidColorBrush PennelloPerDeposito(int indice) =>
            (SolidColorBrush)TrovaDizionarioColori()[ChiaveDeposito(indice % Palette.NumeroColoriDeposito)];

        // ---------- Grafici (SkiaSharp) ----------

        public static SKColor ColoreGraficoPerDeposito(int indice) =>
            InSkColor(Corrente.Depositi[indice % Palette.NumeroColoriDeposito]);

        public static SKColor GrigliaGrafico => InSkColor(Corrente.ChartGrid);

        /// <summary>Etichette sulle fette del grafico radiale.</summary>
        public static SKColor EtichetteGrafico => InSkColor(Corrente.ChartLabel);

        /// <summary>Etichette degli assi (date e importi).</summary>
        public static SKColor EtichetteAssi => InSkColor(Corrente.SecondaryText);

        /// <summary>Sfondo e testo del riquadro che compare passando sopra un punto del grafico.</summary>
        public static SKColor SfondoSuggerimento => InSkColor(Corrente.RaisedSurface);

        public static SKColor TestoSuggerimento => InSkColor(Corrente.TextPrimary);

        // ---------- Conversioni ----------

        public static Color InColore(ColoreRgb colore) => Color.FromArgb(255, colore.R, colore.G, colore.B);

        public static SKColor InSkColor(ColoreRgb colore) => new(colore.R, colore.G, colore.B);

        // ---------- Corrispondenza tra ruoli della palette e chiavi di Colors.xaml ----------

        private static IEnumerable<(string Chiave, Color Colore)> PennelliApp(Palette p)
        {
            yield return ("CanvasBrush", InColore(p.Canvas));
            yield return ("SurfaceBrush", InColore(p.Surface));
            yield return ("RaisedSurfaceBrush", InColore(p.RaisedSurface));
            yield return ("BorderBrushSubtle", InColore(p.BorderSubtle));

            yield return ("TextPrimaryBrush", InColore(p.TextPrimary));
            yield return ("SecondaryTextBrush", InColore(p.SecondaryText));
            yield return ("MutedTextBrush", InColore(p.MutedText));
            yield return ("AccentTextBrush", InColore(p.AccentText));
            yield return ("ErrorTextBrush", InColore(p.ErrorText));
            yield return ("OnAccentBrush", InColore(p.OnAccent));

            yield return ("AccentActionBackground", InColore(p.AccentActionBackground));
            yield return ("AccentActionPointerOverBrush", InColore(p.AccentActionPointerOver));
            yield return ("AccentActionPressedBrush", InColore(p.AccentActionPressed));
            yield return ("AccentActionDisabledBrush", InColore(p.AccentActionDisabled));
            yield return ("AccentActionDisabledForegroundBrush", InColore(p.AccentActionDisabledForeground));
            yield return ("AccentActionBorder", InColore(p.AccentActionBorder));
            yield return ("AccentActionForeground", InColore(p.AccentActionForeground));

            yield return ("DestructiveSurfaceBrush", InColore(p.DestructiveSurface));
            yield return ("DestructiveBorderBrush", InColore(p.DestructiveBorder));
            yield return ("DestructiveForegroundBrush", InColore(p.DestructiveForeground));

            yield return ("ChartGridBrush", InColore(p.ChartGrid));
            yield return ("ChartLabelBrush", InColore(p.ChartLabel));

            for (int i = 0; i < p.Depositi.Count; i++)
                yield return (ChiaveDeposito(i), InColore(p.Depositi[i]));
        }

        private static IEnumerable<(string Chiave, Color Colore)> PennelliDiSistema(Palette p)
        {
            yield return ("TextControlBackground", InColore(p.InputBackground));
            yield return ("TextControlBackgroundPointerOver", InColore(p.InputBackgroundPointerOver));
            yield return ("TextControlBackgroundFocused", InColore(p.InputBackgroundFocused));
            yield return ("TextControlBorderBrush", InColore(p.InputBorder));
            yield return ("TextControlBorderBrushPointerOver", InColore(p.InputBorderPointerOver));
            yield return ("TextControlBorderBrushFocused", InColore(p.InputBorderFocused));
            yield return ("TextControlForeground", InColore(p.InputForeground));
            yield return ("TextControlForegroundPointerOver", InColore(p.InputForegroundPointerOver));
            yield return ("TextControlForegroundFocused", InColore(p.InputForegroundFocused));
            yield return ("TextControlPlaceholderForeground", InColore(p.InputPlaceholder));
            yield return ("TextControlPlaceholderForegroundPointerOver", InColore(p.InputPlaceholderPointerOver));
            yield return ("TextControlPlaceholderForegroundFocused", InColore(p.InputPlaceholderFocused));

        }

        /// <summary>Chiave del pennello del deposito con indice a base zero (0 → "Deposito1Brush").</summary>
        private static string ChiaveDeposito(int indice) => $"Deposito{indice + 1}Brush";

        private static ResourceDictionary? _dizionarioColori;

        private static ResourceDictionary TrovaDizionarioColori()
        {
            return _dizionarioColori ??= Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString.EndsWith(FileColori, StringComparison.OrdinalIgnoreCase) == true)
                ?? throw new InvalidOperationException($"{FileColori} non è tra le risorse dell'applicazione (vedi App.xaml).");
        }

        /// <summary>
        /// In debug segnala i pennelli dichiarati in Colors.xaml a cui la palette non assegna
        /// un colore: resterebbero trasparenti.
        /// </summary>
        [Conditional("DEBUG")]
        private static void VerificaCopertura(ResourceDictionary dizionario, IEnumerable<string> chiaviScritte)
        {
            var scritte = new HashSet<string>(chiaviScritte);
            var dimenticate = dizionario
                .Where(voce => voce.Value is SolidColorBrush && voce.Key is string chiave && !scritte.Contains(chiave))
                .Select(voce => (string)voce.Key)
                .ToList();

            if (dimenticate.Count > 0)
                throw new InvalidOperationException(
                    "Pennelli di Colors.xaml senza un colore nella palette: " + string.Join(", ", dimenticate) +
                    ". Aggiungere il ruolo a Palette e alla corrispondenza in Tema.");
        }
    }
}
