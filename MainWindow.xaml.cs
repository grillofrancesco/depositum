using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace codebase
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void Stampa(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Boom");
            Debug.WriteLine(sender);
            Debug.WriteLine(e);
        }

        // Questo metodo viene scatenato dal click sul bottone
        void Salva_Click(object sender, RoutedEventArgs e)
        {
            // 1. Legge il testo dalla TextBox tramite il suo x:Name
            string valore = InputDati.Text;

            if (!string.IsNullOrWhiteSpace(valore))
            {
                // 2. Aggiunge il valore alla ListView
                ListaDati.Items.Add(valore);

                // 3. Svuota la casella di testo
                InputDati.Text = string.Empty;
            }
        }
    }
}
