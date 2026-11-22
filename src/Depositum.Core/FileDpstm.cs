using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Depositum.Core
{
    /// <summary>
    /// Struttura del file delle rilevazioni 'history.dpstm', senza la cifratura vera e propria
    /// (che dipende da Windows ed è nell'app).
    /// <para>
    /// Il file è composto da: i 5 byte ASCII <c>DPSTM</c>, un byte con la versione del formato,
    /// e poi il contenuto cifrato. Il contenuto in chiaro sono le stesse righe di 'history.csv'
    /// (vedi <see cref="RilevazioneCsv"/>) in UTF-8, separate da <c>\n</c>.
    /// </para>
    /// <para>
    /// Versione 1: contenuto cifrato con DPAPI di Windows per l'utente corrente,
    /// con <see cref="Entropia"/> come entropia aggiuntiva.
    /// Lo script 'csv_a_dpstm.py' produce lo stesso formato: se cambia qui, va cambiato anche lì.
    /// </para>
    /// </summary>
    public static class FileDpstm
    {
        public const string NomeFile = "history.dpstm";

        public const byte Versione = 1;

        private static readonly byte[] Firma = "DPSTM"u8.ToArray();

        /// <summary>
        /// Entropia aggiuntiva passata a DPAPI: un altro programma dello stesso utente che
        /// chiamasse DPAPI senza conoscerla non riuscirebbe a decifrare il file.
        /// </summary>
        public static byte[] Entropia => "Depositum.history.v1"u8.ToArray();

        /// <summary>Antepone firma e versione al contenuto già cifrato.</summary>
        public static byte[] Componi(byte[] contenutoCifrato)
        {
            var file = new byte[Firma.Length + 1 + contenutoCifrato.Length];
            Firma.CopyTo(file, 0);
            file[Firma.Length] = Versione;
            contenutoCifrato.CopyTo(file, Firma.Length + 1);
            return file;
        }

        /// <summary>Controlla firma e versione e restituisce il contenuto cifrato.</summary>
        /// <exception cref="InvalidDataException">Il file non è un .dpstm o ha una versione sconosciuta.</exception>
        public static byte[] Estrai(byte[] file)
        {
            if (file.Length < Firma.Length + 1 || !file.AsSpan(0, Firma.Length).SequenceEqual(Firma))
                throw new InvalidDataException("Il file non è nel formato .dpstm.");

            byte versione = file[Firma.Length];
            if (versione != Versione)
                throw new InvalidDataException($"Versione del formato .dpstm non supportata: {versione}.");

            return file[(Firma.Length + 1)..];
        }

        /// <summary>Le righe del file come testo in chiaro da cifrare.</summary>
        public static byte[] TestoDaRighe(IEnumerable<string> righe)
        {
            var sb = new StringBuilder();
            foreach (var riga in righe)
                sb.Append(riga).Append('\n');
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        /// <summary>
        /// Le righe del testo decifrato. Accetta anche <c>\r\n</c> e un eventuale BOM UTF-8;
        /// le righe vuote restano, come in 'history.csv', e le scarta chi le interpreta.
        /// </summary>
        public static List<string> RigheDaTesto(byte[] testo)
        {
            var s = Encoding.UTF8.GetString(testo).TrimStart('﻿');
            if (s.Length == 0)
                return new List<string>();

            var righe = s.Split('\n').Select(r => r.TrimEnd('\r')).ToList();
            if (righe[^1].Length == 0)
                righe.RemoveAt(righe.Count - 1); // il \n finale non apre una nuova riga
            return righe;
        }
    }
}
