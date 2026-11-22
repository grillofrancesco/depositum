using Depositum.Core.Temi;
using System;
using System.IO;

namespace Depositum.Services
{
    /// <summary>
    /// Ricorda il tema scelto tra un avvio e l'altro, nel file 'tema.txt' accanto a history.csv.
    /// Il file contiene solo l'identificativo della palette (per esempio "acqua").
    /// </summary>
    public static class PreferenzaTema
    {
        private static string FilePath =>
            Path.Combine(Path.GetDirectoryName(DataService.CsvFilePath)!, "tema.txt");

        /// <summary>
        /// La palette salvata. Se il file non c'è, non si legge o contiene un tema sconosciuto,
        /// restituisce quella predefinita: il tema non è un dato da proteggere, l'app parte comunque.
        /// </summary>
        public static Palette Carica()
        {
            try
            {
                return File.Exists(FilePath) ? Palette.PerId(File.ReadAllText(FilePath)) : Palette.Predefinita;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return Palette.Predefinita;
            }
        }

        /// <summary>Salva la scelta. Restituisce <c>false</c> se non è stato possibile scrivere il file.</summary>
        public static bool Salva(Palette palette)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, palette.Id);
                return true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
