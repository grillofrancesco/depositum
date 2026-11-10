using System.Collections.Generic;

namespace Depositum
{
    /// <summary>Palette condivisa per depositi in dashboard e grafici.</summary>
    internal static class PaletteDepositi
    {
        public static IReadOnlyList<string> ColoriHex { get; } = new[]
        {
            // Toni profondi e terrosi, con contrasto sufficiente per il testo chiaro.
            "#43523A", "#3B5146", "#4B4937", "#41464D",
            "#514359", "#37505A", "#59443A", "#4B5142"
        };
    }
}
