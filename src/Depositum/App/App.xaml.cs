using Depositum.Core;
using Depositum.Services;
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
            // La lingua va comunicata a Windows prima che si carichi qualunque testo.
            Testi.ApplicaLinguaScelta();

            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Assegna i colori del tema scelto ai pennelli prima che nasca la finestra.
            // Non si può fare nel costruttore: lì le risorse dell'app non sono ancora accessibili.
            Tema.Applica(PreferenzaTema.Carica());

            // Date e importi nella stessa lingua dei testi.
            Formati.LinguaSchermo = Testi.LinguaAttiva;

            // Carica le rilevazioni salvate. Se il file non si riesce a leggere l'app parte
            // comunque, senza dati, e lo dice: il file sul disco non viene toccato.
            string? erroreLettura = null;
            try
            {
                DataService.CaricaDati();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
            {
                erroreLettura = ex.Message;
            }

            _window = new MainWindow();
            _window.Activate();

            if (erroreLettura != null)
            {
                _window.MostraAvviso(
                    Testi.Get("Avvio_LetturaFallita_Titolo"),
                    Testi.Formato("Avvio_LetturaFallita_Testo", erroreLettura));
            }
            else if (DataService.RigheIgnorate > 0)
            {
                int n = DataService.RigheIgnorate;
                _window.MostraAvviso(
                    Testi.Get("Avvio_RigheIgnorate_Titolo"),
                    n == 1 ? Testi.Get("Avvio_RigheIgnorate_Una") : Testi.Formato("Avvio_RigheIgnorate_Molte", n));
            }
        }
    }
}
