using Depositum.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Depositum.Tests
{
    public class DatiGraficiTests
    {
        private static Rilevazione Ril(int anno, int mese, int giorno, params (string Nome, int Valore)[] depositi)
        {
            var r = new Rilevazione { Data = new DateOnly(anno, mese, giorno) };
            foreach (var (nome, valore) in depositi)
                r.Depositi[nome] = valore;
            r.Totale = depositi.Sum(d => d.Valore);
            return r;
        }

        private static HashSet<string> Visibili(params string[] nomi) => new(nomi, StringComparer.OrdinalIgnoreCase);

        private static readonly List<Rilevazione> Storico = new()
        {
            Ril(2025, 1, 5, ("Conto", 1200), ("Risparmi", 5000), ("Contanti", 300)),
            Ril(2025, 3, 10, ("Conto", 1500), ("Risparmi", 5200), ("Contanti", 250), ("Fondo", 1000)),
            Ril(2025, 6, 15, ("Conto", 900), ("Risparmi", 6000), ("Fondo", 1500)),
            Ril(2025, 9, 20, ("Conto", 2100), ("Risparmi", 6400), ("Fondo", 1800), ("Azioni", 3000)),
            Ril(2026, 11, 2, ("Conto", 1800), ("Risparmi", 7000), ("Fondo", 2100), ("Azioni", 3500), ("Buoni", 500)),
        };

        // ---------- Finestra temporale ----------

        [Fact]
        public void InizioFinestra_Tutto_NessunLimite()
        {
            Assert.Null(DatiGrafici.InizioFinestra(FinestraTemporale.Tutto, new DateOnly(2026, 11, 2)));
        }

        [Theory]
        [InlineData(FinestraTemporale.UnMese, 2026, 10, 2)]
        [InlineData(FinestraTemporale.SeiMesi, 2026, 5, 2)]
        [InlineData(FinestraTemporale.UnAnno, 2025, 11, 2)]
        [InlineData(FinestraTemporale.CinqueAnni, 2021, 11, 2)]
        public void InizioFinestra_ContataAllIndietroDallUltimaData(FinestraTemporale finestra, int anno, int mese, int giorno)
        {
            Assert.Equal(new DateOnly(anno, mese, giorno), DatiGrafici.InizioFinestra(finestra, new DateOnly(2026, 11, 2)));
        }

        [Fact]
        public void InizioFinestra_FineMese_RestaNelMesePrecedente()
        {
            Assert.Equal(new DateOnly(2025, 2, 28), DatiGrafici.InizioFinestra(FinestraTemporale.UnMese, new DateOnly(2025, 3, 31)));
        }

        // ---------- Nomi ----------

        [Fact]
        public void NomiDepositi_InOrdineDiComparsa_SenzaDoppioni()
        {
            Assert.Equal(new[] { "Conto", "Risparmi", "Contanti", "Fondo", "Azioni", "Buoni" }, DatiGrafici.NomiDepositi(Storico));
        }

        [Fact]
        public void NomiDepositi_SenzaDistinzioneMaiuscole_TieneLaPrimaGrafia()
        {
            var rilevazioni = new[] { Ril(2025, 1, 1, ("Conto", 1)), Ril(2025, 2, 1, ("CONTO", 2)) };

            Assert.Equal(new[] { "Conto" }, DatiGrafici.NomiDepositi(rilevazioni));
        }

        // ---------- Istogramma ----------

        [Fact]
        public void LimitiIstogramma_DaZeroAlMassimoPiuMargine()
        {
            var (minimo, massimo) = DatiGrafici.LimitiIstogramma(new[] { 1800, 7000, 500 });

            Assert.Equal(0, minimo);
            Assert.Equal(9800, massimo, 2);
        }

        [Fact]
        public void LimitiIstogramma_ValoriNegativi_IlMinimoScendeSottoZero()
        {
            var (minimo, massimo) = DatiGrafici.LimitiIstogramma(new[] { -50, -10 });

            Assert.Equal(-50, minimo);
            Assert.Equal(1.4, massimo, 5); // il massimo non scende mai sotto 1, prima del margine
        }

        // ---------- Linee ----------

        [Fact]
        public void PreparaLinee_Tutto_UnaSeriePerDepositoConValoriAllineatiAlleDate()
        {
            var dati = DatiGrafici.PreparaLinee(Storico, FinestraTemporale.Tutto,
                Visibili("Conto", "Risparmi", "Contanti", "Fondo", "Azioni", "Buoni"));

            Assert.NotNull(dati);
            Assert.Equal(Storico.Select(r => r.Data), dati!.Date);
            Assert.Equal(new[] { "Conto", "Risparmi", "Contanti", "Fondo", "Azioni", "Buoni" }, dati.Serie.Select(s => s.Nome));
            Assert.Equal(new[] { 0, 1, 2, 3, 4, 5 }, dati.Serie.Select(s => s.IndiceColore));

            Assert.Equal(new int?[] { 1200, 1500, 900, 2100, 1800 }, dati.Serie[0].Valori);
            Assert.Equal(new int?[] { 300, 250, null, null, null }, dati.Serie[2].Valori);
            Assert.Equal(new int?[] { null, null, null, null, 500 }, dati.Serie[5].Valori);
        }

        [Fact]
        public void PreparaLinee_DepositoNascosto_NonHaSerie_MaGliAltriTengonoIlLoroColore()
        {
            var dati = DatiGrafici.PreparaLinee(Storico, FinestraTemporale.Tutto, Visibili("Conto", "Fondo"));

            Assert.Equal(new[] { "Conto", "Fondo" }, dati!.Serie.Select(s => s.Nome));
            Assert.Equal(new[] { 0, 3 }, dati.Serie.Select(s => s.IndiceColore));
        }

        [Fact]
        public void PreparaLinee_Finestra_TieneSoloLeRilevazioniRecenti()
        {
            // L'ultima è del 02/11/2026: un anno indietro arriva al 02/11/2025 e resta solo lei.
            var dati = DatiGrafici.PreparaLinee(Storico, FinestraTemporale.UnAnno,
                Visibili("Conto", "Risparmi", "Contanti", "Fondo", "Azioni", "Buoni"));

            Assert.Equal(new[] { new DateOnly(2026, 11, 2) }, dati!.Date);
            // "Contanti" non compare nella finestra: niente serie, ma gli indici di colore restano quelli dello storico.
            Assert.Equal(new[] { "Conto", "Risparmi", "Fondo", "Azioni", "Buoni" }, dati.Serie.Select(s => s.Nome));
            Assert.Equal(new[] { 0, 1, 3, 4, 5 }, dati.Serie.Select(s => s.IndiceColore));
        }

        [Fact]
        public void PreparaLinee_DataDiInizioFinestraInclusa()
        {
            var rilevazioni = new[] { Ril(2026, 10, 1, ("A", 1)), Ril(2026, 10, 2, ("A", 2)), Ril(2026, 11, 2, ("A", 3)) };

            var dati = DatiGrafici.PreparaLinee(rilevazioni, FinestraTemporale.UnMese, Visibili("A"));

            Assert.Equal(new[] { new DateOnly(2026, 10, 2), new DateOnly(2026, 11, 2) }, dati!.Date);
        }

        [Fact]
        public void PreparaLinee_NessunDepositoScelto_NessunDato()
        {
            Assert.Null(DatiGrafici.PreparaLinee(Storico, FinestraTemporale.Tutto, Visibili()));
        }

        [Fact]
        public void PreparaLinee_DepositiSceltiAssentiNellaFinestra_NessunDato()
        {
            Assert.Null(DatiGrafici.PreparaLinee(Storico, FinestraTemporale.UnAnno, Visibili("Contanti")));
        }

        [Fact]
        public void PreparaLinee_Asse_ValoriVicini_ArrivaAUnaVoltaESeiLaMedia()
        {
            var rilevazioni = new[] { Ril(2025, 1, 1, ("A", 1000), ("B", 1000)) };

            var dati = DatiGrafici.PreparaLinee(rilevazioni, FinestraTemporale.Tutto, Visibili("A", "B"));

            Assert.Equal(0, dati!.MinimoAsse);
            Assert.Equal(1600, dati.MassimoAsse, 2);
        }

        [Fact]
        public void PreparaLinee_Asse_UnValoreMoltoAlto_ArrivaAUnaVoltaEQuattroIlMassimo()
        {
            // media = 3700 → 1,6 × media = 5920, meno del massimo 10000 → 1,4 × 10000
            var rilevazioni = new[] { Ril(2025, 1, 1, ("A", 100), ("B", 1000), ("C", 10000)) };

            var dati = DatiGrafici.PreparaLinee(rilevazioni, FinestraTemporale.Tutto, Visibili("A", "B", "C"));

            Assert.Equal(14000, dati!.MassimoAsse, 2);
        }

        [Fact]
        public void PreparaLinee_Asse_ConsideraSoloIDepositiScelti()
        {
            var rilevazioni = new[] { Ril(2025, 1, 1, ("A", 100), ("B", 10000)) };

            var dati = DatiGrafici.PreparaLinee(rilevazioni, FinestraTemporale.Tutto, Visibili("A"));

            Assert.Equal(160, dati!.MassimoAsse, 2);
        }

        [Fact]
        public void PreparaLinee_Asse_ValoriNegativi_IlMinimoScendeSottoZero()
        {
            var rilevazioni = new[] { Ril(2025, 1, 1, ("A", -200), ("B", 1000)) };

            var dati = DatiGrafici.PreparaLinee(rilevazioni, FinestraTemporale.Tutto, Visibili("A", "B"));

            Assert.Equal(-200, dati!.MinimoAsse);
        }

        // Anomalia 3 corretta (docs/piano-refactoring.md, sezione 4): il grafico a linee e
        // Storico.IndiciColore, usato da dashboard, istogramma e radiale, danno a ogni
        // deposito lo stesso indice di colore.
        [Fact]
        public void IndiceColoreDelleLinee_UgualeAQuelloDelleAltreSchermate()
        {
            var indici = Core.Storico.IndiciColore(Storico);
            var dati = DatiGrafici.PreparaLinee(Storico, FinestraTemporale.Tutto,
                Visibili("Conto", "Risparmi", "Contanti", "Fondo", "Azioni", "Buoni"));

            foreach (var serie in dati!.Serie)
                Assert.Equal(indici[serie.Nome], serie.IndiceColore);
        }
    }
}
