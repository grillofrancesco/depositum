using Microsoft.UI.Xaml.Controls;

namespace Depositum
{
    internal static class Navigazione
    {
        /// <summary>
        /// Torna alla pagina precedente; se non ce n'è una, apre la dashboard.
        /// </summary>
        public static void TornaIndietro(this Frame frame)
        {
            if (frame.CanGoBack)
                frame.GoBack();
            else
                frame.Navigate(typeof(DashboardPage));
        }
    }
}
