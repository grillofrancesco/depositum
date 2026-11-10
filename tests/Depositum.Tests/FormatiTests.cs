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
        }

        [Fact]
        public void Data_GiornoMeseAnnoConZeri()
        {
            Assert.Equal("05/01/2025", Formati.Data(new DateOnly(2025, 1, 5)));
            Assert.Equal("31/12/2026", Formati.Data(new DateOnly(2026, 12, 31)));
        }

        [Theory]
        [InlineData("05/01/2025")]
        [InlineData("5/1/2025")]
        [InlineData("2025-01-05")]
        [InlineData("5 gennaio 2025")]
        public void ProvaALeggereData_FormatiAccettati(string testo)
        {
            Assert.True(Formati.ProvaALeggereData(testo, out var data));
            Assert.Equal(new DateOnly(2025, 1, 5), data);
        }

        [Theory]
        [InlineData("")]
        [InlineData("domani")]
        [InlineData("32/01/2025")]
        [InlineData("29/02/2025")]
        public void ProvaALeggereData_TestoNonValido(string testo)
        {
            Assert.False(Formati.ProvaALeggereData(testo, out _));
        }

        [Fact]
        public void LetturaEScrittura_SonoCoerenti()
        {
            var data = new DateOnly(2026, 11, 2);

            Assert.True(Formati.ProvaALeggereData(Formati.Data(data), out var riletta));
            Assert.Equal(data, riletta);
        }

        [Theory]
        [InlineData(0, "0€")]
        [InlineData(500, "500€")]
        [InlineData(1800, "1.800€")]
        [InlineData(999999999, "999.999.999€")]
        public void Importo_SeparatoreDelleMigliaiaSenzaSpazio(int valore, string atteso)
        {
            Assert.Equal(atteso, Formati.Importo(valore));
        }

        [Theory]
        [InlineData(0, "0 €")]
        [InlineData(1800, "1.800 €")]
        [InlineData(2500.4, "2.500 €")] // i valori degli assi possono non essere interi
        [InlineData(2500.6, "2.501 €")]
        public void ImportoGrafico_SeparatoreDelleMigliaiaConSpazio(double valore, string atteso)
        {
            Assert.Equal(atteso, Formati.ImportoGrafico(valore));
        }

        // Anomalia 6 (docs/piano-refactoring.md, sezione 4): dashboard e grafici scrivono
        // lo stesso importo in due modi diversi.
        [Fact]
        public void Anomalia6_DashboardEGraficiUsanoFormatiDiversi()
        {
            Assert.NotEqual(Formati.Importo(1000), Formati.ImportoGrafico(1000));
        }

        // Anomalia 7: il separatore della data segue la lingua di Windows.
        [Fact]
        public void Anomalia7_IlSeparatoreDellaDataDipendeDallaLingua()
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            Assert.Equal("05.01.2025", Formati.Data(new DateOnly(2025, 1, 5)));
        }
    }
}
