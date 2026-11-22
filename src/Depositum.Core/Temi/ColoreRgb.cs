using System;
using System.Globalization;

namespace Depositum.Core.Temi
{
    /// <summary>
    /// Un colore opaco. Indipendente dall'interfaccia: l'app lo converte nel tipo di colore
    /// che le serve (WinUI o SkiaSharp).
    /// </summary>
    public readonly record struct ColoreRgb(byte R, byte G, byte B)
    {
        /// <summary>Converte "#RRGGBB" in un colore.</summary>
        public static ColoreRgb Da(string hex)
        {
            if (hex.Length != 7 || hex[0] != '#')
                throw new FormatException($"Colore non valido: '{hex}'. Formato atteso: #RRGGBB.");

            return new ColoreRgb(
                byte.Parse(hex.AsSpan(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                byte.Parse(hex.AsSpan(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                byte.Parse(hex.AsSpan(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
        }

        /// <summary>Luminanza relativa secondo le WCAG (0 = nero, 1 = bianco).</summary>
        public double Luminanza => 0.2126 * Canale(R) + 0.7152 * Canale(G) + 0.0722 * Canale(B);

        /// <summary>
        /// Rapporto di contrasto WCAG tra due colori, da 1 a 21. Per un testo normale
        /// serve almeno 4,5.
        /// </summary>
        public static double Contrasto(ColoreRgb a, ColoreRgb b)
        {
            double chiaro = Math.Max(a.Luminanza, b.Luminanza);
            double scuro = Math.Min(a.Luminanza, b.Luminanza);
            return (chiaro + 0.05) / (scuro + 0.05);
        }

        public override string ToString() => $"#{R:X2}{G:X2}{B:X2}";

        private static double Canale(byte valore)
        {
            double c = valore / 255.0;
            return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }
    }
}
