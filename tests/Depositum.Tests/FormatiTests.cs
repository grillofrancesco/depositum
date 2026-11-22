using Depositum.Core;
using System;
using System.Globalization;
using Xunit;

namespace Depositum.Tests
{
    public class FormatiTests
    {
        public FormatiTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("it-IT");
            Formati.LinguaSchermo = Lingua.Italiano;
        }

        // ---------- Date a schermo, italiano ----------

        [Fact]
        public void Data_Italiano_GiornoMeseAnnoConZeri()
        {
            Assert.Equal("05/01/2025", Formati.Data(new DateOnly(2025, 1, 5)));
            Assert.Equal("31/12/2026", Formati.Data(new DateOnly(2026, 12, 31)));
        }

        [Theory]
        [InlineData("05/01/2025")]
        [InlineData("5/1/2025")]
        [InlineData("2025-01-05")]
        [InlineData("5 gennaio 2025")]
        public void DataInserita_Italiano_FormatiAccettati(string testo)
        {
            Assert.True(Formati.ProvaALeggereDataInserita(testo, out var data));
            Assert.Equal(new DateOnly(2025, 1, 5), data);
        }

        // ---------- Date a schermo, inglese ----------

        [Fact]
        public void Data_Inglese_MeseGiornoAnno()
        {
            Formati.LinguaSchermo = Lingua.Inglese;

            Assert.Equal("01/05/2025", Formati.Data(new DateOnly(2025, 1, 5)));
            Assert.Equal("12/31/2026", Formati.Data(new DateOnly(2026, 12, 31)));
        }

        [Theory]
        [InlineData("01/05/2025")]
        [InlineData("1/5/2025")]
        [InlineData("2025-01-05")]
        [InlineData("January 5, 2025")]
        public void DataInserita_Inglese_FormatiAccettati(string testo)
        {
            Formati.LinguaSchermo = Lingua.Inglese;

            Assert.True(Formati.ProvaALeggereDataInserita(testo, out var data));
            Assert.Equal(new DateOnly(2025, 1, 5), data);
        }

        [Fact]
        public void DataInserita_StessoTesto_SignificatoDiversoPerLingua()
        {
            Assert.True(Formati.ProvaALeggereDataInserita("05/01/2025", out var italiano));

            Formati.LinguaSchermo = Lingua.Inglese;
            Assert.True(Formati.ProvaALeggereDataInserita("05/01/2025", out var inglese));

            Assert.Equal(new DateOnly(2025, 1, 5), italiano);
            Assert.Equal(new DateOnly(2025, 5, 1), inglese);
        }

        [Theory]
        [InlineData(Lingua.Italiano, "")]
        [InlineData(Lingua.Italiano, "domani")]
        [InlineData(Lingua.Italiano, "32/01/2025")]
        [InlineData(Lingua.Italiano, "29/02/2025")]
        [InlineData(Lingua.Inglese, "13/05/2025")]
        [InlineData(Lingua.Inglese, "02/29/2025")]
        public void DataInserita_TestoNonValido(Lingua lingua, string testo)
        {
            Formati.LinguaSchermo = lingua;

            Assert.False(Formati.ProvaALeggereDataInserita(testo, out _));
        }

        [Theory]
        [InlineData(Lingua.Italiano)]
        [InlineData(Lingua.Inglese)]
        public void DataMostrata_SiRilegge(Lingua lingua)
        {
            Formati.LinguaSchermo = lingua;
            var data = new DateOnly(2026, 11, 2);

            Assert.True(Formati.ProvaALeggereDataInserita(Formati.Data(data), out var riletta));
            Assert.Equal(data, riletta);
        }

        // ---------- Date nel file ----------

        [Fact]
        public void DataPerFile_GiornoMeseAnnoConTrattino()
        {
            Assert.Equal("05-01-2025", Formati.DataPerFile(new DateOnly(2025, 1, 5)));
            Assert.Equal("31-12-2026", Formati.DataPerFile(new DateOnly(2026, 12, 31)));
        }

        [Theory]
        [InlineData(Lingua.Italiano)]
        [InlineData(Lingua.Inglese)]
        public void File_NonDipendeDallaLinguaDellInterfaccia(Lingua lingua)
        {
            Formati.LinguaSchermo = lingua;
            var data = new DateOnly(2025, 1, 5);

            Assert.Equal("05-01-2025", Formati.DataPerFile(data));
            Assert.True(Formati.ProvaALeggereDataDelFile("05/01/2025", out var vecchia));
            Assert.True(Formati.ProvaALeggereDataDelFile("05-01-2025", out var nuova));
            Assert.Equal(data, vecchia);
            Assert.Equal(data, nuova);
        }

        [Theory]
        [InlineData("05/01/2025")]
        [InlineData("5/1/2025")]
        [InlineData("05-01-2025")]
        [InlineData("5-1-2025")]
        [InlineData("2025-01-05")]
        [InlineData("5 gennaio 2025")]
        public void DataDelFile_FormatiAccettati(string testo)
        {
            Assert.True(Formati.ProvaALeggereDataDelFile(testo, out var data));
            Assert.Equal(new DateOnly(2025, 1, 5), data);
        }

        [Theory]
        [InlineData("")]
        [InlineData("domani")]
        [InlineData("32/01/2025")]
        [InlineData("29/02/2025")]
        [InlineData("32-01-2025")]
        [InlineData("29-02-2025")]
        public void DataDelFile_TestoNonValido(string testo)
        {
            Assert.False(Formati.ProvaALeggereDataDelFile(testo, out _));
        }

        // ---------- Importi ----------

        [Theory]
        [InlineData(0, "0€")]
        [InlineData(500, "500€")]
        [InlineData(1800, "1.800€")]
        [InlineData(999999999, "999.999.999€")]
        [InlineData(2999999997, "2.999.999.997€")]
        [InlineData(-1800, "-1.800€")]
        public void Importo_Italiano_PuntoPerLeMigliaiaESimboloDopo(long valore, string atteso)
        {
            Assert.Equal(atteso, Formati.Importo(valore));
        }

        [Theory]
        [InlineData(0, "€0")]
        [InlineData(500, "€500")]
        [InlineData(1800, "€1,800")]
        [InlineData(2999999997, "€2,999,999,997")]
        [InlineData(-1800, "-€1,800")]
        public void Importo_Inglese_VirgolaPerLeMigliaiaESimboloPrima(long valore, string atteso)
        {
            Formati.LinguaSchermo = Lingua.Inglese;

            Assert.Equal(atteso, Formati.Importo(valore));
        }

        [Theory]
        [InlineData(0.0, "0€")]
        [InlineData(1800.0, "1.800€")]
        [InlineData(2500.4, "2.500€")] // i valori degli assi possono non essere interi
        [InlineData(2500.6, "2.501€")]
        public void Importo_ValoreNonIntero_Arrotondato(double valore, string atteso)
        {
            Assert.Equal(atteso, Formati.Importo(valore));
        }

        // Anomalia 6 corretta (docs/piano-refactoring.md, sezione 4): dashboard e grafici
        // scrivono lo stesso importo nello stesso modo, in ogni lingua.
        [Theory]
        [InlineData(Lingua.Italiano, "1.000€")]
        [InlineData(Lingua.Inglese, "€1,000")]
        public void DashboardEGrafici_StessoFormato(Lingua lingua, string atteso)
        {
            Formati.LinguaSchermo = lingua;
            long daDashboard = 1000;
            double daGrafico = 1000;

            Assert.Equal(Formati.Importo(daDashboard), Formati.Importo(daGrafico));
            Assert.Equal(atteso, Formati.Importo(daGrafico));
        }

        [Theory]
        [InlineData(Lingua.Italiano, 0.1234, "12,3%")]
        [InlineData(Lingua.Inglese, 0.1234, "12.3%")]
        public void Percentuale_UnDecimale(Lingua lingua, double frazione, string atteso)
        {
            Formati.LinguaSchermo = lingua;

            Assert.Equal(atteso, Formati.Percentuale(frazione).Replace(" ", " ").Replace(" ", ""));
        }

        // Anomalia 7 corretta: il formato non segue più la lingua di Windows, ma solo quella
        // dell'interfaccia (a schermo) o nessuna (nel file).
        [Theory]
        [InlineData("it-IT")]
        [InlineData("de-DE")]
        [InlineData("en-US")]
        [InlineData("nl-NL")]
        public void Formati_NonDipendonoDallaLinguaDiWindows(string linguaWindows)
        {
            CultureInfo.CurrentCulture = new CultureInfo(linguaWindows);
            var data = new DateOnly(2025, 1, 5);

            Assert.Equal("05/01/2025", Formati.Data(data));
            Assert.Equal("05-01-2025", Formati.DataPerFile(data));
            Assert.Equal("1.000€", Formati.Importo(1000));
        }
    }
}
