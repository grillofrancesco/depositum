using Microsoft.UI.Xaml;
using System;
using System.IO;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Depositum
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private MainWindow? _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Assegna i colori ai pennelli prima che nasca la finestra.
            // Non si può fare nel costruttore: lì le risorse dell'app non sono ancora accessibili.
            Tema.Applica(Palette.Verde);

            // Carica le rilevazioni salvate. Se il file non si riesce a leggere l'app parte
            // comunque, senza dati, e lo dice: il file sul disco non viene toccato.
            string? erroreLettura = null;
            try
            {
                DataService.CaricaDati();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                erroreLettura = ex.Message;
            }

            _window = new MainWindow();
            _window.Activate();

            if (erroreLettura != null)
            {
                _window.MostraAvviso(
                    "Impossibile leggere i dati",
                    "Il file delle rilevazioni non è accessibile, quindi l'app è partita senza dati. " +
                    "Il file non è stato modificato: chiudi l'app, verifica che nessun altro programma lo stia usando e riprova." +
                    Environment.NewLine + Environment.NewLine + erroreLettura);
            }
        }
    }
}
