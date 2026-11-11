using Depositum.Core;
using System;
using System.Globalization;
using System.Linq;
using Xunit;

namespace Depositum.Tests
{
    /// <summary>
    /// Test di caratterizzazione: fissano ciò che la lettura e la scrittura di 'history.csv'
    /// fanno oggi, anomalie comprese. I test delle anomalie citano il numero che hanno in
    /// docs/piano-refactoring.md, sezione 4: correggerne una richiede di aggiornare il suo test.
    /// </summary>
    public class RilevazioneCsvTests
    {
        public RilevazioneCsvTests()
        {
            // Lettura e scrittura della data dipendono in parte dalla cultura corrente:
            // i test la fissano su quella con cui l'app viene usata.
            CultureInfo.CurrentCulture = new CultureInfo("it-IT");
        }

        // ---------- Lettura ----------

        [Fact]
        public void Lettura_RigaNormale()
        {
            var r = RilevazioneCsv.DaRiga("Data:05/01/2025,Conto:1200,Risparmi:5000,Totale:6200");

            Assert.NotNull(r);
            Assert.Equal(new DateOnly(2025, 1, 5), r!.Data);
            Assert.Equal(new[] { "Conto", "Risparmi" }, r.Depositi.Keys.ToArray());
            Assert.Equal(1200, r.Depositi["Conto"]);
            Assert.Equal(5000, r.Depositi["Risparmi"]);
            Assert.Equal(6200, r.Totale);
        }

        [Fact]
        public void Lettura_SpaziAttornoAiCampiIgnorati()
        {
            var r = RilevazioneCsv.DaRiga("   Data : 05/01/2025 ,  Conto : 1200 , Totale: 1200   ");

            Assert.Equal(new DateOnly(2025, 1, 5), r!.Data);
            Assert.Equal(1200, r.Depositi["Conto"]);
            Assert.Equal(1200, r.Totale);
        }

        [Theory]
        [InlineData("Data:05/01/2025,Conto:1200,Totale:1200;")]
        [InlineData("Data:05/01/2025,Conto:1200,Totale:1200;;;")]
        [InlineData("Data:05/01/2025;,Conto:1200;,Totale:1200;")]
        public void Lettura_PuntoEVirgolaFinaleIgnorato(string riga)
        {
            var r = RilevazioneCsv.DaRiga(riga);

            Assert.Equal(new DateOnly(2025, 1, 5), r!.Data);
            Assert.Equal(1200, r.Depositi["Conto"]);
            Assert.Equal(1200, r.Totale);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void Lettura_RigaVuotaNonProduceRilevazione(string riga)
        {
            Assert.Null(RilevazioneCsv.DaRiga(riga));
        }

        [Theory]
        [InlineData("05-01-2025")]     // formato con cui l'app scrive oggi
        [InlineData("5-1-2025")]
        [InlineData("05/01/2025")]     // formato delle righe salvate prima dell'anomalia 7
        [InlineData("5/1/2025")]
        [InlineData("2025-01-05")]
        [InlineData("5 gennaio 2025")] // formato libero, interpretato secondo la cultura corrente
        public void Lettura_FormatiDataAccettati(string data)
        {
            var r = RilevazioneCsv.DaRiga($"Data:{data},Conto:1,Totale:1");

            Assert.Equal(new DateOnly(2025, 1, 5), r!.Data);
        }

        [Fact]
        public void Lettura_ChiaviDataETotaleSenzaDistinzioneMaiuscole()
        {
            var r = RilevazioneCsv.DaRiga("data:05/01/2025,Conto:10,TOTALE:10");

            Assert.Equal(new DateOnly(2025, 1, 5), r!.Data);
            Assert.Equal(10, r.Totale);
            Assert.Equal(new[] { "Conto" }, r.Depositi.Keys.ToArray());
        }

        [Fact]
        public void Lettura_NomiDepositoSenzaDistinzioneMaiuscole_VinceLUltimoValore()
        {
            var r = RilevazioneCsv.DaRiga("Data:05/01/2025,Conto:1,conto:2,Totale:2");

            Assert.Equal(new[] { "Conto" }, r!.Depositi.Keys.ToArray());
            Assert.Equal(2, r.Depositi["CONTO"]);
        }

        [Fact]
        public void Lettura_OrdineDeiDepositiConservato()
        {
            var r = RilevazioneCsv.DaRiga("Data:05/01/2025,Zeta:1,Alfa:2,Media:3,Totale:6");

            Assert.Equal(new[] { "Zeta", "Alfa", "Media" }, r!.Depositi.Keys.ToArray());
        }

        [Theory]
        [InlineData("Conto:abc")]   // non numerico
        [InlineData("Conto:12.5")]  // decimale
        [InlineData("Conto:")]      // senza valore
        [InlineData("Conto")]       // senza i due punti
        [InlineData(":5")]          // senza nome
        public void Lettura_CampiNonRiconoscibiliIgnorati(string campo)
        {
            var r = RilevazioneCsv.DaRiga($"Data:05/01/2025,{campo},Totale:7");

            Assert.Empty(r!.Depositi);
            Assert.Equal(new DateOnly(2025, 1, 5), r.Data);
            Assert.Equal(7, r.Totale);
        }

        [Fact]
        public void Lettura_ImportoConVirgolaDecimale_LettaSoloLaParteIntera()
        {
            // La virgola separa i campi: "Conto:12" è un deposito valido, "5" non ha i due punti.
            var r = RilevazioneCsv.DaRiga("Data:05/01/2025,Conto:12,5,Totale:12");

            Assert.Equal(12, r!.Depositi["Conto"]);
            Assert.Single(r.Depositi);
        }

        [Fact]
        public void Lettura_TotaleNonNumericoRestaZero()
        {
            var r = RilevazioneCsv.DaRiga("Data:05/01/2025,Conto:5,Totale:xyz");

            Assert.Equal(0, r!.Totale);
            Assert.Equal(5, r.Depositi["Conto"]);
        }

        [Fact]
        public void Lettura_ImportoNegativoAccettato()
        {
            var r = RilevazioneCsv.DaRiga("Data:05/01/2025,Conto:-5,Totale:-5");

            Assert.Equal(-5, r!.Depositi["Conto"]);
            Assert.Equal(-5, r.Totale);
        }

        // ---------- Scrittura ----------

        [Fact]
        public void Scrittura_FormatoEsattoDellaRiga()
        {
            var r = new Rilevazione { Data = new DateOnly(2025, 1, 5), Totale = 6200 };
            r.Depositi["Conto"] = 1200;
            r.Depositi["Risparmi"] = 5000;

            Assert.Equal("Data:05-01-2025,Conto:1200,Risparmi:5000,Totale:6200", RilevazioneCsv.InRiga(r));
        }

        [Fact]
        public void Scrittura_SenzaDepositi()
        {
            var r = new Rilevazione { Data = new DateOnly(2025, 12, 31), Totale = 0 };

            Assert.Equal("Data:31-12-2025,Totale:0", RilevazioneCsv.InRiga(r));
        }

        // ---------- Andata e ritorno ----------

        [Fact]
        public void FileConRigheVecchieENuove_LetteTutte()
        {
            // Un file scritto in parte prima e in parte dopo il passaggio al trattino.
            var vecchia = RilevazioneCsv.DaRiga("Data:05/01/2025,Conto:100,Totale:100");
            var nuova = RilevazioneCsv.DaRiga("Data:02-11-2026,Conto:200,Totale:200");

            Assert.Equal(new DateOnly(2025, 1, 5), vecchia!.Data);
            Assert.Equal(new DateOnly(2026, 11, 2), nuova!.Data);
        }

        [Fact]
        public void Scrittura_NonDipendeDallaLinguaDiWindows()
        {
            var r = new Rilevazione { Data = new DateOnly(2025, 1, 5), Totale = 1 };
            r.Depositi["Conto"] = 1;

            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            Assert.Equal("Data:05-01-2025,Conto:1,Totale:1", RilevazioneCsv.InRiga(r));
        }

        [Fact]
        public void AndataERitorno_StessiDati()
        {
            var originale = new Rilevazione { Data = new DateOnly(2026, 11, 2), Totale = 14900 };
            originale.Depositi["Conto"] = 1800;
            originale.Depositi["Risparmi"] = 7000;
            originale.Depositi["Fondo pensione"] = 6100;

            var riletta = RilevazioneCsv.DaRiga(RilevazioneCsv.InRiga(originale));

            Assert.Equal(originale.Data, riletta!.Data);
            Assert.Equal(originale.Totale, riletta.Totale);
            Assert.Equal(originale.Depositi.ToArray(), riletta.Depositi.ToArray());
        }

        // Anomalia 5 corretta: tre depositi al massimo consentito superano il limite di un int;
        // prima la somma sollevava un'eccezione e l'app si chiudeva alla conferma.
        [Fact]
        public void TotaleOltreIlLimiteDiUnInt_CalcolatoSalvatoERiletto()
        {
            var originale = new Rilevazione { Data = new DateOnly(2025, 1, 5) };
            originale.Depositi["A"] = RegoleDeposito.ImportoMassimo;
            originale.Depositi["B"] = RegoleDeposito.ImportoMassimo;
            originale.Depositi["C"] = RegoleDeposito.ImportoMassimo;
            originale.Totale = originale.SommaDepositi();

            Assert.Equal(2_999_999_997L, originale.Totale);
            Assert.True(originale.Totale > int.MaxValue);

            string riga = RilevazioneCsv.InRiga(originale);
            var riletta = RilevazioneCsv.DaRiga(riga);

            Assert.Equal("Data:05-01-2025,A:999999999,B:999999999,C:999999999,Totale:2999999997", riga);
            Assert.Equal(2_999_999_997L, riletta!.Totale);
            Assert.Equal(2_999_999_997L, riletta.SommaDepositi());
        }

        // ---------- Limiti del formato ----------
        // Il file non protegge i nomi con virgola o due punti: per questo RegoleDeposito li
        // rifiuta prima che arrivino qui (anomalia 2, corretta lì).

        [Fact]
        public void Formato_NomeConDuePunti_SiPerdeAllaRilettura()
        {
            var originale = new Rilevazione { Data = new DateOnly(2025, 1, 5), Totale = 100 };
            originale.Depositi["Conto: BNL"] = 100;

            string riga = RilevazioneCsv.InRiga(originale);
            var riletta = RilevazioneCsv.DaRiga(riga);

            Assert.Equal("Data:05-01-2025,Conto: BNL:100,Totale:100", riga);
            Assert.Empty(riletta!.Depositi);
        }

        [Fact]
        public void Formato_NomeConVirgola_VieneLettoConUnAltroNome()
        {
            var originale = new Rilevazione { Data = new DateOnly(2025, 1, 5), Totale = 5 };
            originale.Depositi["Casa, mare"] = 5;

            var riletta = RilevazioneCsv.DaRiga(RilevazioneCsv.InRiga(originale));

            Assert.Equal(new[] { "mare" }, riletta!.Depositi.Keys.ToArray());
            Assert.Equal(5, riletta.Depositi["mare"]);
        }

        // ---------- Righe senza data ----------

        [Theory]
        [InlineData("Conto:5,Totale:5")]              // data assente
        [InlineData("Data:non-una-data,Conto:5,Totale:5")] // data illeggibile
        [InlineData(";")]                             // solo il terminatore
        public void RigaSenzaDataValida_NonProduceRilevazione(string riga)
        {
            // Anomalia 4 corretta: prima diventava una rilevazione datata 01/01/0001.
            Assert.Null(RilevazioneCsv.DaRiga(riga));
        }
    }
}
