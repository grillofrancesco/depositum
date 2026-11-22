using System.Collections.Generic;

namespace Depositum.Core.Temi
{
    // Le palette disponibili. Tutte sono temi scuri: sfondi quasi neri con una sfumatura
    // della tinta del tema, accento nella tinta piena. I colori dei depositi alternano la
    // tinta del tema e tinte complementari, a luminosità simile, perché card vicine si
    // distinguano. I contrasti minimi sono verificati da Depositum.Tests/PaletteTests.
    public sealed partial record Palette
    {
        /// <summary>Giallo oro e ottone. Ottone e oro antico; depositi in oro, blu e viola per contrasto.</summary>
        public static Palette Oro { get; } = new()
        {
            Id = "oro",
            Nome = "Giallo oro e ottone",
            Descrizione = "Ottone e oro antico; depositi in oro, blu e viola per contrasto.",

            Canvas = Hex("#161513"),
            Surface = Hex("#1F1E1A"),
            RaisedSurface = Hex("#29261F"),
            BorderSubtle = Hex("#413D33"),

            TextPrimary = Hex("#F2ECDE"),
            SecondaryText = Hex("#BAB5AB"),
            MutedText = Hex("#8C877D"),
            AccentText = Hex("#C2AA73"),
            ErrorText = Hex("#FA8072"),
            OnAccent = Hex("#FFFFFF"),

            AccentActionBackground = Hex("#826A32"),
            AccentActionPointerOver = Hex("#9A7F41"),
            AccentActionPressed = Hex("#6A572C"),
            AccentActionDisabled = Hex("#252420"),
            AccentActionDisabledForeground = Hex("#8D8A82"),
            AccentActionBorder = Hex("#A58D54"),
            AccentActionForeground = Hex("#FFFFFF"),

            DestructiveSurface = Hex("#29251F"),
            DestructiveBorder = Hex("#55453A"),
            DestructiveForeground = Hex("#D7A997"),

            InputBackground = Hex("#23211D"),
            InputBackgroundPointerOver = Hex("#2D2A23"),
            InputBackgroundFocused = Hex("#2B271F"),
            InputBorder = Hex("#494439"),
            InputBorderPointerOver = Hex("#6A5C3C"),
            InputBorderFocused = Hex("#C2AA73"),
            InputForeground = Hex("#F2ECDE"),
            InputForegroundPointerOver = Hex("#FFFFFF"),
            InputForegroundFocused = Hex("#FFFFFF"),
            InputPlaceholder = Hex("#8E897E"),
            InputPlaceholderPointerOver = Hex("#ABA79D"),
            InputPlaceholderFocused = Hex("#C2AA73"),

            SystemAccent = Hex("#927B45"),

            ChartGrid = Hex("#5A5A5A"),
            ChartLabel = Hex("#FFFFFF"),

            Depositi = new[]
            {
                Hex("#594F39"), Hex("#3C3959"), Hex("#3D5939"), Hex("#394A59"),
                Hex("#593F39"), Hex("#4F3959"), Hex("#39594F"), Hex("#555939")
            }
        };

        /// <summary>Acqua. Verde acqua e petrolio; depositi in acqua, azzurro e terracotta per contrasto.</summary>
        public static Palette Acqua { get; } = new()
        {
            Id = "acqua",
            Nome = "Acqua",
            Descrizione = "Verde acqua e petrolio; depositi in acqua, azzurro e terracotta per contrasto.",

            Canvas = Hex("#131616"),
            Surface = Hex("#1A1F1F"),
            RaisedSurface = Hex("#202828"),
            BorderSubtle = Hex("#344040"),

            TextPrimary = Hex("#DFF1F1"),
            SecondaryText = Hex("#ACB9B9"),
            MutedText = Hex("#7E8B8B"),
            AccentText = Hex("#80BDBF"),
            ErrorText = Hex("#FA8472"),
            OnAccent = Hex("#FFFFFF"),

            AccentActionBackground = Hex("#3C7F82"),
            AccentActionPointerOver = Hex("#4C9699"),
            AccentActionPressed = Hex("#346A6C"),
            AccentActionDisabled = Hex("#202525"),
            AccentActionDisabledForeground = Hex("#838C8C"),
            AccentActionBorder = Hex("#5FA1A4"),
            AccentActionForeground = Hex("#FFFFFF"),

            DestructiveSurface = Hex("#29261F"),
            DestructiveBorder = Hex("#55473A"),
            DestructiveForeground = Hex("#D7AD97"),

            InputBackground = Hex("#1D2323"),
            InputBackgroundPointerOver = Hex("#242C2C"),
            InputBackgroundFocused = Hex("#202A2A"),
            InputBorder = Hex("#3A4848"),
            InputBorderPointerOver = Hex("#446B6C"),
            InputBorderFocused = Hex("#80BDBF"),
            InputForeground = Hex("#DFF1F1"),
            InputForegroundPointerOver = Hex("#FFFFFF"),
            InputForegroundFocused = Hex("#FFFFFF"),
            InputPlaceholder = Hex("#7F8D8D"),
            InputPlaceholderPointerOver = Hex("#9EAAAA"),
            InputPlaceholderFocused = Hex("#80BDBF"),

            SystemAccent = Hex("#509092"),

            ChartGrid = Hex("#5A5A5A"),
            ChartLabel = Hex("#FFFFFF"),

            Depositi = new[]
            {
                Hex("#395859"), Hex("#394A59"), Hex("#594439"), Hex("#39594C"),
                Hex("#593942"), Hex("#394059"), Hex("#594F39"), Hex("#39593F")
            }
        };

        /// <summary>Terra. Terracotta e terra bruciata; depositi in terra, ottanio e verde salvia.</summary>
        public static Palette Terra { get; } = new()
        {
            Id = "terra",
            Nome = "Terra",
            Descrizione = "Terracotta e terra bruciata; depositi in terra, ottanio e verde salvia.",

            Canvas = Hex("#161413"),
            Surface = Hex("#1F1C1A"),
            RaisedSurface = Hex("#282320"),
            BorderSubtle = Hex("#413933"),

            TextPrimary = Hex("#F2E6DE"),
            SecondaryText = Hex("#BAB1AB"),
            MutedText = Hex("#8C837D"),
            AccentText = Hex("#BB977F"),
            ErrorText = Hex("#FA8072"),
            OnAccent = Hex("#FFFFFF"),

            AccentActionBackground = Hex("#7C563D"),
            AccentActionPointerOver = Hex("#93694D"),
            AccentActionPressed = Hex("#664835"),
            AccentActionDisabled = Hex("#252220"),
            AccentActionDisabledForeground = Hex("#8C8783"),
            AccentActionBorder = Hex("#9F795F"),
            AccentActionForeground = Hex("#FFFFFF"),

            DestructiveSurface = Hex("#29241F"),
            DestructiveBorder = Hex("#55423A"),
            DestructiveForeground = Hex("#D7A397"),

            InputBackground = Hex("#231F1D"),
            InputBackgroundPointerOver = Hex("#2C2724"),
            InputBackgroundFocused = Hex("#2A241F"),
            InputBorder = Hex("#493F39"),
            InputBorderPointerOver = Hex("#685243"),
            InputBorderFocused = Hex("#BB977F"),
            InputForeground = Hex("#F2E6DE"),
            InputForegroundPointerOver = Hex("#FFFFFF"),
            InputForegroundFocused = Hex("#FFFFFF"),
            InputPlaceholder = Hex("#8E847E"),
            InputPlaceholderPointerOver = Hex("#ABA39D"),
            InputPlaceholderFocused = Hex("#BB977F"),

            SystemAccent = Hex("#8D6850"),

            ChartGrid = Hex("#5A5A5A"),
            ChartLabel = Hex("#FFFFFF"),

            Depositi = new[]
            {
                Hex("#594539"), Hex("#394759"), Hex("#595539"), Hex("#395759"),
                Hex("#593D39"), Hex("#393959"), Hex("#4C5939"), Hex("#395945")
            }
        };

        /// <summary>Verde natura. Il tema con cui è nata l'app: verde oliva e alloro.</summary>
        public static Palette Verde { get; } = new()
        {
            Id = "verde",
            Nome = "Verde natura",
            Descrizione = "Il tema con cui è nata l'app: verde oliva e alloro.",

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

            Depositi = new[]
            {
                Hex("#43523A"), Hex("#3B5146"), Hex("#4B4937"), Hex("#41464D"),
                Hex("#514359"), Hex("#37505A"), Hex("#59443A"), Hex("#4B5142")
            }
        };

        /// <summary>Rosso. Rosso mattone e cremisi; depositi in rosso, verde acqua e blu per contrasto.</summary>
        public static Palette Rosso { get; } = new()
        {
            Id = "rosso",
            Nome = "Rosso",
            Descrizione = "Rosso mattone e cremisi; depositi in rosso, verde acqua e blu per contrasto.",

            Canvas = Hex("#161313"),
            Surface = Hex("#1F1A1A"),
            RaisedSurface = Hex("#282020"),
            BorderSubtle = Hex("#403434"),

            TextPrimary = Hex("#F1DFDF"),
            SecondaryText = Hex("#B9ACAC"),
            MutedText = Hex("#8B7E7E"),
            AccentText = Hex("#BF7B7E"),
            ErrorText = Hex("#FAA472"),
            OnAccent = Hex("#FFFFFF"),

            AccentActionBackground = Hex("#81383C"),
            AccentActionPointerOver = Hex("#98484C"),
            AccentActionPressed = Hex("#6A3134"),
            AccentActionDisabled = Hex("#252020"),
            AccentActionDisabledForeground = Hex("#8C8383"),
            AccentActionBorder = Hex("#A45A5E"),
            AccentActionForeground = Hex("#FFFFFF"),

            DestructiveSurface = Hex("#29281F"),
            DestructiveBorder = Hex("#554E3A"),
            DestructiveForeground = Hex("#D7BE97"),

            InputBackground = Hex("#231D1D"),
            InputBackgroundPointerOver = Hex("#2C2424"),
            InputBackgroundFocused = Hex("#2A2020"),
            InputBorder = Hex("#483A3A"),
            InputBorderPointerOver = Hex("#6B4042"),
            InputBorderFocused = Hex("#BF7B7E"),
            InputForeground = Hex("#F1DFDF"),
            InputForegroundPointerOver = Hex("#FFFFFF"),
            InputForegroundFocused = Hex("#FFFFFF"),
            InputPlaceholder = Hex("#8D7F7F"),
            InputPlaceholderPointerOver = Hex("#AA9E9E"),
            InputPlaceholderFocused = Hex("#BF7B7E"),

            SystemAccent = Hex("#914C4F"),

            ChartGrid = Hex("#5A5A5A"),
            ChartLabel = Hex("#FFFFFF"),

            Depositi = new[]
            {
                Hex("#59393A"), Hex("#395957"), Hex("#594C39"), Hex("#394859"),
                Hex("#593950"), Hex("#395947"), Hex("#4A5939"), Hex("#3C3959")
            }
        };

        // Deve restare dopo le palette: i campi statici si inizializzano nell'ordine del file.
        /// <summary>Tutte le palette, nell'ordine in cui compaiono nella scelta del tema.</summary>
        public static IReadOnlyList<Palette> Tutte { get; } = new[] { Oro, Acqua, Terra, Verde, Rosso };
    }
}
