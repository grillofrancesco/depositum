using Depositum.Pages;
using Depositum.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Graphics;

namespace Depositum
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Dimensione della finestra all'avvio: 500x700
            AppWindow.Resize(new SizeInt32(500, 700));

            // Icona nella barra del titolo e nella barra delle applicazioni
            AppWindow.SetIcon(System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "Depositum.ico"));

            // Naviga verso la pagina principale (Dashboard)
            RootFrame.Navigate(typeof(DashboardPage));
        }

        /// <summary>
        /// Mostra un avviso sopra la pagina corrente. Si può chiamare anche prima che la
        /// finestra sia a schermo: in quel caso l'avviso compare appena il contenuto è pronto.
        /// </summary>
        public void MostraAvviso(string titolo, string messaggio)
        {
            if (RootFrame.IsLoaded)
                ApriAvviso(titolo, messaggio);
            else
                RootFrame.Loaded += (_, _) => ApriAvviso(titolo, messaggio);
        }

        private async void ApriAvviso(string titolo, string messaggio)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = RootFrame.XamlRoot,
                Title = titolo,
                Content = new TextBlock { Text = messaggio, TextWrapping = TextWrapping.Wrap },
                CloseButtonText = Testi.Get("Comune_Chiudi")
            };
            await dialog.ShowAsync();
        }
    }
}
