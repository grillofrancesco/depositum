using Depositum.Core;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.Globalization;
using System;

namespace Depositum.Services
{
    /// <summary>
    /// Testi dell'interfaccia e lingua dell'app.
    /// <para>
    /// I testi stanno in Strings/&lt;lingua&gt;/Resources.resw. Lo XAML li prende da solo tramite
    /// x:Uid; il codice li chiede qui per chiave. Windows sceglie la lingua in base alle
    /// preferenze del sistema (italiano o inglese; per altre lingue l'inglese), salvo che
    /// l'utente ne abbia scelta una nelle impostazioni.
    /// </para>
    /// </summary>
    public static class Testi
    {
        private const string FilePreferenza = "lingua.txt";

        /// <summary>Lingue che si possono scegliere: il valore salvato tra le preferenze.</summary>
        public const string Automatica = "";
        public const string Italiano = "it-IT";
        public const string Inglese = "en-US";

        private static ResourceLoader? _loader;

        private static ResourceLoader Loader => _loader ??= new ResourceLoader();

        /// <summary>Il testo con la chiave indicata; la chiave tra parentesi quadre se manca, per vederla subito.</summary>
        public static string Get(string chiave)
        {
            string testo = Loader.GetString(chiave);
            return string.IsNullOrEmpty(testo) ? $"[{chiave}]" : testo;
        }

        /// <summary>Il testo con la chiave indicata, con i segnaposto {0}, {1}… sostituiti.</summary>
        public static string Formato(string chiave, params object[] valori) =>
            string.Format(Formati.CulturaSchermo, Get(chiave), valori);

        /// <summary>Messaggio da mostrare per un errore di validazione di un deposito.</summary>
        public static string Per(ErroreDeposito errore) => errore switch
        {
            ErroreDeposito.NomeTroppoLungo => Formato("ErroreDeposito_NomeTroppoLungo", RegoleDeposito.LunghezzaMassimaNome),
            _ => Get("ErroreDeposito_" + errore)
        };

        // ---------- Lingua ----------

        /// <summary>La lingua scelta dall'utente (<see cref="Automatica"/>, <see cref="Italiano"/> o <see cref="Inglese"/>).</summary>
        public static string LinguaScelta
        {
            get
            {
                string? salvata = Preferenze.Leggi(FilePreferenza);
                return salvata is Italiano or Inglese ? salvata : Automatica;
            }
        }

        /// <summary>
        /// Comunica a Windows la lingua scelta. Va chiamato all'avvio, prima che si carichi
        /// qualunque testo: il costruttore di App, prima di InitializeComponent.
        /// </summary>
        public static void ApplicaLinguaScelta()
        {
            string scelta = LinguaScelta;
            if (scelta != Automatica)
            {
                ApplicationLanguages.PrimaryLanguageOverride = scelta;
                return;
            }

            // Lingua di Windows: va tolta una scelta precedente. Da app installata (pacchetto MSIX)
            // Windows la ricorda tra un avvio e l'altro; senza pacchetto non la ricorda, e
            // impostare una stringa vuota è rifiutato, quindi si tocca solo se c'è qualcosa.
            if (!string.IsNullOrEmpty(ApplicationLanguages.PrimaryLanguageOverride))
                ApplicationLanguages.PrimaryLanguageOverride = string.Empty;
        }

        /// <summary>
        /// Salva la lingua scelta, che si applicherà al prossimo avvio.
        /// Restituisce <c>false</c> se non è stato possibile salvarla.
        /// </summary>
        public static bool SalvaLinguaScelta(string lingua)
        {
            if (lingua is not (Automatica or Italiano or Inglese))
                throw new ArgumentException($"Lingua non prevista: '{lingua}'.", nameof(lingua));

            return Preferenze.Scrivi(FilePreferenza, lingua);
        }

        /// <summary>
        /// La lingua in cui sono effettivamente mostrati i testi. Date e importi la seguono,
        /// così testi e formati sono sempre nella stessa lingua.
        /// </summary>
        public static Lingua LinguaAttiva => Get("CodiceLingua") == "en" ? Lingua.Inglese : Lingua.Italiano;
    }
}
