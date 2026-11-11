using Depositum.Theme;
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
    /// ciò che li usa si aggiorna da solo. Il codice che non può usare un pennello (i grafici)
    /// legge i colori da qui e si ridisegna quando arriva <see cref="TemaCambiato"/>.
    /// </para>
    /// </summary>
    public static class Tema
    {
        private const string FileColori = "Theme/Colors.xaml";
        private const string ChiaveAccentoDiSistema = "SystemAccentColor";

        /// <summary>La palette attualmente applicata.</summary>
        public static Palette Corrente { get; private set; } = Palette.Verde;

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

            chiaviSistema[ChiaveAccentoDiSistema] = palette.SystemAccent;

            VerificaCopertura(colori, pennelliApp.Select(p => p.Chiave));
            VerificaCopertura(chiaviSistema, pennelliSistema.Select(p => p.Chiave));

            Corrente = palette;
            TemaCambiato?.Invoke(null, EventArgs.Empty);
        }

        // ---------- Depositi ----------

        /// <summary>Pennello condiviso del deposito in posizione <paramref name="indice"/>; oltre l'ultimo i colori si ripetono.</summary>
        public static SolidColorBrush PennelloPerDeposito(int indice) =>
            (SolidColorBrush)TrovaDizionarioColori()[ChiaveDeposito(indice % Palette.NumeroColoriDeposito)];

        /// <summary>Colore del deposito in posizione <paramref name="indice"/>; oltre l'ultimo i colori si ripetono.</summary>
        public static Color ColorePerDeposito(int indice) =>
            Corrente.Depositi[indice % Palette.NumeroColoriDeposito];

        // ---------- Grafici (SkiaSharp) ----------

        public static SKColor ColoreGraficoPerDeposito(int indice) => InSkColor(ColorePerDeposito(indice));

        public static SKColor GrigliaGrafico => InSkColor(Corrente.ChartGrid);

        public static SKColor EtichetteGrafico => InSkColor(Corrente.ChartLabel);

        public static SKColor InSkColor(Color colore) => new(colore.R, colore.G, colore.B, colore.A);

        // ---------- Corrispondenza tra ruoli della palette e chiavi di Colors.xaml ----------

        private static IEnumerable<(string Chiave, Color Colore)> PennelliApp(Palette p)
        {
            yield return ("CanvasBrush", p.Canvas);
            yield return ("SurfaceBrush", p.Surface);
            yield return ("RaisedSurfaceBrush", p.RaisedSurface);
            yield return ("BorderBrushSubtle", p.BorderSubtle);

            yield return ("TextPrimaryBrush", p.TextPrimary);
            yield return ("SecondaryTextBrush", p.SecondaryText);
            yield return ("MutedTextBrush", p.MutedText);
            yield return ("AccentTextBrush", p.AccentText);
            yield return ("ErrorTextBrush", p.ErrorText);
            yield return ("OnAccentBrush", p.OnAccent);

            yield return ("AccentActionBackground", p.AccentActionBackground);
            yield return ("AccentActionPointerOverBrush", p.AccentActionPointerOver);
            yield return ("AccentActionPressedBrush", p.AccentActionPressed);
            yield return ("AccentActionDisabledBrush", p.AccentActionDisabled);
            yield return ("AccentActionDisabledForegroundBrush", p.AccentActionDisabledForeground);
            yield return ("AccentActionBorder", p.AccentActionBorder);
            yield return ("AccentActionForeground", p.AccentActionForeground);

            yield return ("DestructiveSurfaceBrush", p.DestructiveSurface);
            yield return ("DestructiveBorderBrush", p.DestructiveBorder);
            yield return ("DestructiveForegroundBrush", p.DestructiveForeground);

            yield return ("ChartGridBrush", p.ChartGrid);
            yield return ("ChartLabelBrush", p.ChartLabel);

            for (int i = 0; i < p.Depositi.Count; i++)
                yield return (ChiaveDeposito(i), p.Depositi[i]);
        }

        private static IEnumerable<(string Chiave, Color Colore)> PennelliDiSistema(Palette p)
        {
            yield return ("TextControlBackground", p.InputBackground);
            yield return ("TextControlBackgroundPointerOver", p.InputBackgroundPointerOver);
            yield return ("TextControlBackgroundFocused", p.InputBackgroundFocused);
            yield return ("TextControlBorderBrush", p.InputBorder);
            yield return ("TextControlBorderBrushPointerOver", p.InputBorderPointerOver);
            yield return ("TextControlBorderBrushFocused", p.InputBorderFocused);
            yield return ("TextControlForeground", p.InputForeground);
            yield return ("TextControlForegroundPointerOver", p.InputForegroundPointerOver);
            yield return ("TextControlForegroundFocused", p.InputForegroundFocused);
            yield return ("TextControlPlaceholderForeground", p.InputPlaceholder);
            yield return ("TextControlPlaceholderForegroundPointerOver", p.InputPlaceholderPointerOver);
            yield return ("TextControlPlaceholderForegroundFocused", p.InputPlaceholderFocused);
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
