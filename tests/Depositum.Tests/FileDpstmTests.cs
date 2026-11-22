using Depositum.Core;
using System.IO;
using System.Text;
using Xunit;

namespace Depositum.Tests
{
    /// <summary>
    /// Struttura del file 'history.dpstm' (firma, versione, righe). La cifratura DPAPI
    /// dipende da Windows e dall'utente, quindi qui il contenuto "cifrato" è un array qualsiasi.
    /// </summary>
    public class FileDpstmTests
    {
        [Fact]
        public void Componi_Antepone_Firma_E_Versione()
        {
            var file = FileDpstm.Componi(new byte[] { 9, 8, 7 });

            Assert.Equal(new byte[] { (byte)'D', (byte)'P', (byte)'S', (byte)'T', (byte)'M', 1, 9, 8, 7 }, file);
        }

        [Fact]
        public void Estrai_Restituisce_Il_Contenuto_Composto()
        {
            var contenuto = new byte[] { 1, 2, 3, 4 };

            Assert.Equal(contenuto, FileDpstm.Estrai(FileDpstm.Componi(contenuto)));
        }

        [Fact]
        public void Estrai_Rifiuta_Un_Csv_In_Chiaro()
        {
            var csv = Encoding.UTF8.GetBytes("Data:05-01-2025,Conto:1200,Totale:1200\n");

            Assert.Throws<InvalidDataException>(() => FileDpstm.Estrai(csv));
        }

        [Fact]
        public void Estrai_Rifiuta_Un_File_Troppo_Corto()
        {
            Assert.Throws<InvalidDataException>(() => FileDpstm.Estrai("DPST"u8.ToArray()));
        }

        [Fact]
        public void Estrai_Rifiuta_Una_Versione_Sconosciuta()
        {
            var file = FileDpstm.Componi(new byte[] { 1 });
            file[5] = 2;

            Assert.Throws<InvalidDataException>(() => FileDpstm.Estrai(file));
        }

        [Fact]
        public void Righe_Andata_E_Ritorno()
        {
            var righe = new[] { "Data:05-01-2025,Conto:1200,Totale:1200", "", "riga senza data", "Data:06-01-2025,Totale:0" };

            Assert.Equal(righe, FileDpstm.RigheDaTesto(FileDpstm.TestoDaRighe(righe)));
        }

        [Fact]
        public void Righe_Accetta_Crlf_E_Bom()
        {
            var testo = Encoding.UTF8.GetBytes("﻿uno\r\ndue\r\n");

            Assert.Equal(new[] { "uno", "due" }, FileDpstm.RigheDaTesto(testo));
        }

        [Fact]
        public void Righe_Di_Un_File_Vuoto()
        {
            Assert.Empty(FileDpstm.RigheDaTesto(FileDpstm.TestoDaRighe(new string[0])));
        }
    }
}
