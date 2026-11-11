using System;
using System.Collections.Generic;
using System.Globalization;
using Windows.UI;

namespace Depositum.Theme
{
    /// <summary>
    /// Una palette: un nome e un colore per ogni ruolo dell'interfaccia.
    /// Per aggiungere un tema basta definire un'altra palette qui sotto: pagine e stili
    /// non cambiano, perché usano i ruoli e non i colori. La applica <see cref="Services.Tema"/>.
    /// </summary>
    public sealed record Palette
    {
        /// <summary>Numero di colori dei depositi che ogni palette deve fornire.</summary>
        public const int NumeroColoriDeposito = 8;

        public required string Nome { get; init; }

        // Sfondi e bordi
        public required Color Canvas { get; init; }
        public required Color Surface { get; init; }
        public required Color RaisedSurface { get; init; }
        public required Color BorderSubtle { get; init; }

        // Testi
        public required Color TextPrimary { get; init; }
        public required Color SecondaryText { get; init; }
        public required Color MutedText { get; init; }
        public required Color AccentText { get; init; }
        public required Color ErrorText { get; init; }
        /// <summary>Testo e icone sopra superfici colorate: pulsanti grandi, card dei depositi.</summary>
        public required Color OnAccent { get; init; }

        // Pulsante d'azione principale
        public required Color AccentActionBackground { get; init; }
        public required Color AccentActionPointerOver { get; init; }
        public required Color AccentActionPressed { get; init; }
        public required Color AccentActionDisabled { get; init; }
        public required Color AccentActionDisabledForeground { get; init; }
        public required Color AccentActionBorder { get; init; }
        public required Color AccentActionForeground { get; init; }

        // Azione distruttiva (rimozione di un deposito)
        public required Color DestructiveSurface { get; init; }
        public required Color DestructiveBorder { get; init; }
        public required Color DestructiveForeground { get; init; }

        // Campi di testo (TextBox, NumberBox)
        public required Color InputBackground { get; init; }
        public required Color InputBackgroundPointerOver { get; init; }
        public required Color InputBackgroundFocused { get; init; }
        public required Color InputBorder { get; init; }
        public required Color InputBorderPointerOver { get; init; }
        public required Color InputBorderFocused { get; init; }
        public required Color InputForeground { get; init; }
        public required Color InputForegroundPointerOver { get; init; }
        public required Color InputForegroundFocused { get; init; }
        public required Color InputPlaceholder { get; init; }
        public required Color InputPlaceholderPointerOver { get; init; }
        public required Color InputPlaceholderFocused { get; init; }

        /// <summary>Colore d'accento passato ai controlli standard di WinUI.</summary>
        public required Color SystemAccent { get; init; }

        // Grafici
        public required Color ChartGrid { get; init; }
        public required Color ChartLabel { get; init; }

        /// <summary>
        /// Colori dei depositi, assegnati in ordine e riusati ciclicamente.
        /// Devono essere esattamente <see cref="NumeroColoriDeposito"/>.
        /// </summary>
        public required IReadOnlyList<Color> Depositi { get; init; }

        /// <summary>Il tema verde con cui è nata l'app.</summary>
        public static Palette Verde { get; } = new()
        {
            Nome = "Verde",

            Canvas = Hex("#141613"),
            Surface = Hex("#1C1F1A"),
            RaisedSurface = Hex("#242820"),
            BorderSubtle = Hex("#3A4034"),

            TextPrimary = Hex("#F1EEDF"),
            SecondaryText = Hex("#B8B9AC"),
            MutedText = Hex("#888B7E"),
            AccentText = Hex("#A8B58A"),
            ErrorText = Hex("#FA8072"),
            OnAccent = Hex("#FFFFFF"),

            AccentActionBackground = Hex("#657747"),
            AccentActionPointerOver = Hex("#7B8D58"),
            AccentActionPressed = Hex("#53633D"),
            AccentActionDisabled = Hex("#242520"),
            AccentActionDisabledForeground = Hex("#8A8C83"),
            AccentActionBorder = Hex("#87996A"),
            AccentActionForeground = Hex("#FFFFFF"),

            DestructiveSurface = Hex("#29251F"),
            DestructiveBorder = Hex("#55453A"),
            DestructiveForeground = Hex("#D7A997"),

            InputBackground = Hex("#20231D"),
            InputBackgroundPointerOver = Hex("#282C24"),
            InputBackgroundFocused = Hex("#252A20"),
            InputBorder = Hex("#41483A"),
            InputBorderPointerOver = Hex("#59664A"),
            InputBorderFocused = Hex("#A8B58A"),
            InputForeground = Hex("#F1EEDF"),
            InputForegroundPointerOver = Hex("#FFFFFF"),
            InputForegroundFocused = Hex("#FFFFFF"),
            InputPlaceholder = Hex("#898D7F"),
            InputPlaceholderPointerOver = Hex("#A8AA9E"),
            InputPlaceholderFocused = Hex("#A8B58A"),

            SystemAccent = Hex("#78885A"),

            ChartGrid = Hex("#5A5A5A"),
            ChartLabel = Hex("#FFFFFF"),

            // Toni profondi e terrosi, con contrasto sufficiente per il testo chiaro.
            Depositi = new[]
            {
                Hex("#43523A"), Hex("#3B5146"), Hex("#4B4937"), Hex("#41464D"),
                Hex("#514359"), Hex("#37505A"), Hex("#59443A"), Hex("#4B5142")
            }
        };

        /// <summary>Converte "#RRGGBB" in un colore opaco.</summary>
        private static Color Hex(string hex)
        {
            if (hex.Length != 7 || hex[0] != '#')
                throw new FormatException($"Colore non valido: '{hex}'. Formato atteso: #RRGGBB.");

            byte r = byte.Parse(hex.AsSpan(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            byte g = byte.Parse(hex.AsSpan(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            byte b = byte.Parse(hex.AsSpan(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return Color.FromArgb(255, r, g, b);
        }
    }
}
