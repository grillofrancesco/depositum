using Depositum.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace Depositum.Services
{
    /// <summary>
    /// Servizio centralizzato per il caricamento e il salvataggio delle rilevazioni.
    /// Il file di salvataggio è unicamente 'Depositum/history.dpstm' nella cartella AppData locale
    /// dell'utente, cifrato con DPAPI di Windows: si legge solo con lo stesso utente sullo stesso PC.
    /// Il formato del file è in <see cref="FileDpstm"/>, l'interpretazione delle righe in <see cref="RilevazioneCsv"/>.
    /// </summary>
    public static class DataService
    {
        public static string CartellaDati { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Depositum"
        );

        public static string DataFilePath { get; } = Path.Combine(CartellaDati, FileDpstm.NomeFile);

        /// <summary>Il vecchio file in chiaro: se c'è e manca il .dpstm, viene convertito ed eliminato.</summary>
        private static string VecchioCsvPath { get; } = Path.Combine(CartellaDati, "history.csv");

        /// <summary>Rilevazioni nell'ordine in cui sono nel file, che può non essere quello delle date.</summary>
        public static List<Rilevazione> Rilevazioni { get; } = new();

        /// <summary>
        /// Numero di righe non vuote del file scartate dall'ultimo <see cref="CaricaDati"/>
        /// perché senza una data valida. Restano nel file, ma l'app non le usa.
        /// </summary>
        public static int RigheIgnorate { get; private set; }

        /// <summary>La rilevazione con la data più recente, la stessa per dashboard, registrazione e grafici.</summary>
        public static Rilevazione? UltimaRilevazione => Storico.PiuRecente(Rilevazioni);

        /// <summary>
        /// Tutte le righe del file, comprese quelle ignorate: a ogni salvataggio il file viene
        /// riscritto per intero, e le righe che l'app non usa non devono andare perse.
        /// </summary>
        private static readonly List<string> _righeFile = new();

        /// <summary>
        /// Falso finché il file non è stato letto con successo. Se la lettura fallisce l'app
        /// parte senza dati, e salvare riscriverebbe il file con la sola nuova rilevazione:
        /// in quel caso il salvataggio è bloccato.
        /// </summary>
        private static bool _caricato;

        /// <summary>
        /// Legge e decifra il file in AppData. Se non esiste, lascia la lista vuota.
        /// Se manca il .dpstm ma c'è il vecchio 'history.csv', lo converte ed elimina il CSV.
        /// </summary>
        /// <exception cref="IOException">Il file esiste ma non si riesce a leggere (per esempio è in uso).</exception>
        /// <exception cref="UnauthorizedAccessException">Manca il permesso di leggere il file.</exception>
        /// <exception cref="InvalidDataException">Il file non è un .dpstm valido o non si riesce a decifrare.</exception>
        public static void CaricaDati()
        {
            Rilevazioni.Clear();
            _righeFile.Clear();
            RigheIgnorate = 0;
            _caricato = false;

            Debug.WriteLine(DataFilePath);

            if (File.Exists(DataFilePath))
            {
                _righeFile.AddRange(Decifra(File.ReadAllBytes(DataFilePath)));
            }
            else if (File.Exists(VecchioCsvPath))
            {
                var righe = File.ReadAllLines(VecchioCsvPath);
                ScriviFile(righe);
                File.Delete(VecchioCsvPath);
                _righeFile.AddRange(righe);
            }

            foreach (var rigaGrezza in _righeFile)
            {
                if (string.IsNullOrWhiteSpace(rigaGrezza))
                    continue;

                var rilevazione = RilevazioneCsv.DaRiga(rigaGrezza);
                if (rilevazione != null)
                    Rilevazioni.Add(rilevazione);
                else
                    RigheIgnorate++;
            }

            _caricato = true;
        }

        /// <summary>
        /// Aggiunge una nuova rilevazione a 'history.dpstm' in AppData, riscrivendolo cifrato,
        /// e poi la aggiunge a quelle in memoria.
        /// Se la scrittura fallisce la rilevazione non viene aggiunta nemmeno in memoria.
        /// </summary>
        /// <exception cref="IOException">Il file non si riesce a scrivere, oppure all'avvio non era stato letto.</exception>
        /// <exception cref="UnauthorizedAccessException">Manca il permesso di scrivere il file.</exception>
        public static void AggiungiRilevazione(Rilevazione nuova)
        {
            if (!_caricato)
                throw new IOException(Testi.Get("Salvataggio_FileNonLetto"));

            var righe = new List<string>(_righeFile) { RilevazioneCsv.InRiga(nuova) };
            ScriviFile(righe);

            _righeFile.Add(righe[^1]);
            Rilevazioni.Add(nuova);
        }

        /// <summary>
        /// Cifra le righe e le scrive prima in un file temporaneo, che poi sostituisce quello vero:
        /// se l'app si interrompe a metà, il file precedente resta intatto.
        /// </summary>
        private static void ScriviFile(IEnumerable<string> righe)
        {
            Directory.CreateDirectory(CartellaDati);

            byte[] cifrato = ProtectedData.Protect(
                FileDpstm.TestoDaRighe(righe), FileDpstm.Entropia, DataProtectionScope.CurrentUser);

            string temporaneo = DataFilePath + ".tmp";
            File.WriteAllBytes(temporaneo, FileDpstm.Componi(cifrato));
            File.Move(temporaneo, DataFilePath, overwrite: true);
        }

        private static List<string> Decifra(byte[] file)
        {
            byte[] cifrato = FileDpstm.Estrai(file);
            try
            {
                byte[] testo = ProtectedData.Unprotect(cifrato, FileDpstm.Entropia, DataProtectionScope.CurrentUser);
                return FileDpstm.RigheDaTesto(testo);
            }
            catch (CryptographicException ex)
            {
                // Tipicamente: file creato da un altro utente Windows o su un altro PC, oppure danneggiato.
                throw new InvalidDataException(Testi.Get("Avvio_DecifraturaFallita"), ex);
            }
        }
    }
}
