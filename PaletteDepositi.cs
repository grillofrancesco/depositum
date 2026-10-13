using System.Collections.Generic;

namespace codebase
{
    /// <summary>Palette condivisa per depositi in dashboard e grafici.</summary>
    internal static class PaletteDepositi
    {
        public static IReadOnlyList<string> ColoriHex { get; } = new[]
        {
            "#1A6B6B", "#206535", "#2B4A6F", "#733F35",
            "#58395B", "#1C4E5E", "#644F2E", "#3A444C"
        };
    }
}
