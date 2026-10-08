using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace codebase
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Dimensione della finestra all'avvio: 500x700
            AppWindow.Resize(new SizeInt32(500, 700));

            // Caricamento dei dati dal file CSV all'avvio dell'applicazione
            DataService.CaricaDati();

            // Naviga verso la pagina principale (Dashboard)
            RootFrame.Navigate(typeof(DashboardPage));
        }
    }
}
