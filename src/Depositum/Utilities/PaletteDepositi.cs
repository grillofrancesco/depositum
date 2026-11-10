using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Depositum
{
    /// <summary>
    /// Colori dei depositi, condivisi tra dashboard e grafici.
    /// I valori sono in Theme/Colors.xaml (Deposito1Brush … Deposito8Brush).
    /// </summary>
    internal static class PaletteDepositi
    {
        private const int NumeroColori = 8;

        /// <summary>Pennello del deposito in posizione <paramref name="indice"/>; oltre l'ottavo i colori si ripetono.</summary>
        public static SolidColorBrush Pennello(int indice) =>
            (SolidColorBrush)Application.Current.Resources[$"Deposito{indice % NumeroColori + 1}Brush"];

        public static Color Colore(int indice) => Pennello(indice).Color;
    }
}
