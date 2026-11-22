using Depositum.Core.Temi;

namespace Depositum.Services
{
    /// <summary>
    /// Ricorda il tema scelto tra un avvio e l'altro, nel file 'tema.txt' accanto a history.dpstm.
    /// Il file contiene solo l'identificativo della palette (per esempio "acqua").
    /// </summary>
    public static class PreferenzaTema
    {
        private const string File = "tema.txt";

        /// <summary>La palette salvata; quella predefinita se non c'è o non è valida.</summary>
        public static Palette Carica() => Palette.PerId(Preferenze.Leggi(File));

        /// <summary>Salva la scelta. Restituisce <c>false</c> se non è stato possibile scrivere il file.</summary>
        public static bool Salva(Palette palette) => Preferenze.Scrivi(File, palette.Id);
    }
}
