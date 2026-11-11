using Depositum.Core;
using System;
using System.Linq;
using Xunit;

namespace Depositum.Tests
{
    public class StoricoTests
    {
        private static Rilevazione Ril(int anno, int mese, int giorno, int totale) =>
            new() { Data = new DateOnly(anno, mese, giorno), Totale = totale };

        [Fact]
        public void PiuRecente_NessunaRilevazione()
        {
            Assert.Null(Storico.PiuRecente(Array.Empty<Rilevazione>()));
        }

        // Anomalia 1 corretta: una rilevazione registrata con data passata non diventa "l'ultima".
        [Fact]
        public void PiuRecente_ContaLaData_NonLOrdineDiInserimento()
        {
            var rilevazioni = new[] { Ril(2026, 11, 2, 100), Ril(2025, 1, 5, 200) };

            Assert.Equal(100, Storico.PiuRecente(rilevazioni)!.Totale);
        }

        [Fact]
        public void PiuRecente_StessaData_VinceQuellaInseritaPerUltima()
        {
            var rilevazioni = new[] { Ril(2026, 11, 2, 100), Ril(2026, 11, 2, 200) };

            Assert.Equal(200, Storico.PiuRecente(rilevazioni)!.Totale);
        }

        [Fact]
        public void OrdinatePerData_DallaPiuVecchia_ConOrdineDiInserimentoAParita()
        {
            var rilevazioni = new[] { Ril(2026, 11, 2, 1), Ril(2025, 1, 5, 2), Ril(2026, 11, 2, 3), Ril(2025, 6, 1, 4) };

            Assert.Equal(new[] { 2, 4, 1, 3 }, Storico.OrdinatePerData(rilevazioni).Select(r => r.Totale));
        }

        [Fact]
        public void IndiciColore_PosizioneDelNomeNelloStoricoOrdinatoPerData()
        {
            var vecchia = Ril(2025, 1, 5, 0);
            vecchia.Depositi["Conto"] = 1;
            vecchia.Depositi["Contanti"] = 2;
            var recente = Ril(2026, 11, 2, 0);
            recente.Depositi["Conto"] = 3;
            recente.Depositi["Fondo"] = 4;

            // inserite in ordine inverso rispetto alle date
            var indici = Storico.IndiciColore(new[] { recente, vecchia });

            Assert.Equal(0, indici["Conto"]);
            Assert.Equal(1, indici["Contanti"]);
            Assert.Equal(2, indici["Fondo"]);
            Assert.Equal(2, indici["FONDO"]); // senza distinzione tra maiuscole e minuscole
        }

        // Anomalia 3 corretta: un deposito tolto nel tempo non sposta i colori degli altri.
        [Fact]
        public void IndiciColore_UnDepositoToltoNonCambiaIColoriDegliAltri()
        {
            var prima = Ril(2025, 1, 5, 0);
            prima.Depositi["Conto"] = 1;
            prima.Depositi["Contanti"] = 2;
            prima.Depositi["Fondo"] = 3;
            var dopo = Ril(2026, 11, 2, 0);
            dopo.Depositi["Conto"] = 1;
            dopo.Depositi["Fondo"] = 3;

            Assert.Equal(Storico.IndiciColore(new[] { prima })["Fondo"], Storico.IndiciColore(new[] { prima, dopo })["Fondo"]);
        }

        [Fact]
        public void PiuRecente_ELUltimaDiOrdinatePerData()
        {
            var rilevazioni = new[] { Ril(2026, 11, 2, 1), Ril(2025, 1, 5, 2), Ril(2026, 11, 2, 3) };

            Assert.Same(Storico.OrdinatePerData(rilevazioni)[^1], Storico.PiuRecente(rilevazioni));
        }
    }
}
