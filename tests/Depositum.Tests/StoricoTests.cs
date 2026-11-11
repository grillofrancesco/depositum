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
        public void PiuRecente_ELUltimaDiOrdinatePerData()
        {
            var rilevazioni = new[] { Ril(2026, 11, 2, 1), Ril(2025, 1, 5, 2), Ril(2026, 11, 2, 3) };

            Assert.Same(Storico.OrdinatePerData(rilevazioni)[^1], Storico.PiuRecente(rilevazioni));
        }
    }
}
