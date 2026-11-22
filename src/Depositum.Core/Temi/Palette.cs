using System;
using System.Collections.Generic;
using System.Linq;

namespace Depositum.Core.Temi
{
    /// <summary>
    /// Una palette: un nome e un colore per ogni ruolo dell'interfaccia.
    /// Le palette disponibili sono definite in Palette.Temi.cs; per aggiungerne una basta
    /// definirla lì e inserirla in <see cref="Tutte"/>. Pagine e stili non cambiano, perché
    /// usano i ruoli e non i colori. Nell'app la applica Services/Tema.
    /// </summary>
    public sealed partial record Palette
    {
        /// <summary>Numero di colori dei depositi che ogni palette deve fornire.</summary>
        public const int NumeroColoriDeposito = 8;

        /// <summary>Identificativo stabile, salvato tra le preferenze: non va mai cambiato.</summary>
        public required string Id { get; init; }

        /// <summary>Nome mostrato all'utente.</summary>
        public required string Nome { get; init; }

        /// <summary>Breve descrizione mostrata nella scelta del tema.</summary>
        public required string Descrizione { get; init; }

        // Sfondi e bordi
        public required ColoreRgb Canvas { get; init; }
        public required ColoreRgb Surface { get; init; }
        public required ColoreRgb RaisedSurface { get; init; }
        public required ColoreRgb BorderSubtle { get; init; }

        // Testi
        public required ColoreRgb TextPrimary { get; init; }
        public required ColoreRgb SecondaryText { get; init; }
        public required ColoreRgb MutedText { get; init; }
        public required ColoreRgb AccentText { get; init; }
        public required ColoreRgb ErrorText { get; init; }
        /// <summary>Testo e icone sopra superfici colorate: pulsanti grandi, card dei depositi.</summary>
        public required ColoreRgb OnAccent { get; init; }

        // Pulsante d'azione principale
        public required ColoreRgb AccentActionBackground { get; init; }
        public required ColoreRgb AccentActionPointerOver { get; init; }
        public required ColoreRgb AccentActionPressed { get; init; }
        public required ColoreRgb AccentActionDisabled { get; init; }
        public required ColoreRgb AccentActionDisabledForeground { get; init; }
        public required ColoreRgb AccentActionBorder { get; init; }
        public required ColoreRgb AccentActionForeground { get; init; }

        // Azione distruttiva (rimozione di un deposito)
        public required ColoreRgb DestructiveSurface { get; init; }
        public required ColoreRgb DestructiveBorder { get; init; }
        public required ColoreRgb DestructiveForeground { get; init; }

        // Campi di testo (TextBox, NumberBox)
        public required ColoreRgb InputBackground { get; init; }
        public required ColoreRgb InputBackgroundPointerOver { get; init; }
        public required ColoreRgb InputBackgroundFocused { get; init; }
        public required ColoreRgb InputBorder { get; init; }
        public required ColoreRgb InputBorderPointerOver { get; init; }
        public required ColoreRgb InputBorderFocused { get; init; }
        public required ColoreRgb InputForeground { get; init; }
        public required ColoreRgb InputForegroundPointerOver { get; init; }
        public required ColoreRgb InputForegroundFocused { get; init; }
        public required ColoreRgb InputPlaceholder { get; init; }
        public required ColoreRgb InputPlaceholderPointerOver { get; init; }
        public required ColoreRgb InputPlaceholderFocused { get; init; }

        // Controlli standard di WinUI
        /// <summary>Colore d'accento passato a WinUI come SystemAccentColor.</summary>
        public required ColoreRgb SystemAccent { get; init; }

        // Grafici
        public required ColoreRgb ChartGrid { get; init; }
        /// <summary>Etichette sulle fette del grafico radiale.</summary>
        public required ColoreRgb ChartLabel { get; init; }
        /// <summary>
        /// Colori dei depositi, assegnati in ordine e riusati ciclicamente.
        /// Devono essere esattamente <see cref="NumeroColoriDeposito"/>.
        /// </summary>
        public required IReadOnlyList<ColoreRgb> Depositi { get; init; }

        /// <summary>Palette usata al primo avvio e quando la preferenza salvata non è valida.</summary>
        public static Palette Predefinita => Verde;

        /// <summary>
        /// La palette con l'identificativo indicato (senza distinzione tra maiuscole e minuscole);
        /// <see cref="Predefinita"/> se l'identificativo è vuoto o sconosciuto.
        /// </summary>
        public static Palette PerId(string? id) =>
            Tutte.FirstOrDefault(p => string.Equals(p.Id, id?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? Predefinita;

        private static ColoreRgb Hex(string hex) => ColoreRgb.Da(hex);
    }
}
