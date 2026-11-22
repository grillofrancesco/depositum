using Depositum.Core.Temi;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Depositum.Tests
{
    /// <summary>
    /// Regole che ogni palette deve rispettare. Valgono anche per le palette aggiunte in futuro,
    /// perché i test le prendono tutte da <see cref="Palette.Tutte"/>.
    /// </summary>
    public class PaletteTests
    {
        public static IEnumerable<object[]> OgniPalette() => Palette.Tutte.Select(p => new object[] { p.Id });

        private static Palette P(string id) => Palette.Tutte.Single(p => p.Id == id);

        private static void AlmenoContrasto(ColoreRgb a, ColoreRgb b, double minimo, string cosa)
        {
            double c = ColoreRgb.Contrasto(a, b);
            Assert.True(c >= minimo, $"{cosa}: contrasto {c:0.00} tra {a} e {b}, minimo {minimo}");
        }

        // ---------- Elenco ----------

        [Fact]
        public void CinqueTemi_NellOrdineScelto()
        {
            Assert.Equal(new[] { "oro", "acqua", "terra", "verde", "rosso" }, Palette.Tutte.Select(p => p.Id));
            Assert.Equal(new[] { "Giallo oro e ottone", "Acqua", "Terra", "Verde natura", "Rosso" }, Palette.Tutte.Select(p => p.Nome));
        }

        [Fact]
        public void IlPredefinitoEIlVerde()
        {
            Assert.Same(Palette.Verde, Palette.Predefinita);
        }

        [Theory]
        [InlineData("acqua", "acqua")]
        [InlineData("ROSSO", "rosso")]
        [InlineData("  terra ", "terra")]
        [InlineData(null, "verde")]
        [InlineData("", "verde")]
        [InlineData("viola", "verde")]
        public void PerId_SconosciutoOVuoto_DiventaIlPredefinito(string? id, string atteso)
        {
            Assert.Equal(atteso, Palette.PerId(id).Id);
        }

        // ---------- Completezza ----------

        [Theory]
        [MemberData(nameof(OgniPalette))]
        public void OttoColoriDiDepositoTuttiDiversi(string id)
        {
            var depositi = P(id).Depositi;

            Assert.Equal(Palette.NumeroColoriDeposito, depositi.Count);
            Assert.Equal(depositi.Count, depositi.Distinct().Count());
        }

        // ---------- Leggibilità ----------

        [Theory]
        [MemberData(nameof(OgniPalette))]
        public void TestiLeggibili(string id)
        {
            var p = P(id);

            AlmenoContrasto(p.TextPrimary, p.Canvas, 7, "testo principale sullo sfondo");
            AlmenoContrasto(p.TextPrimary, p.Surface, 7, "testo principale sulle superfici");
            AlmenoContrasto(p.TextPrimary, p.RaisedSurface, 7, "testo principale sui pulsanti");
            AlmenoContrasto(p.SecondaryText, p.Surface, 4.5, "testo secondario");
            AlmenoContrasto(p.AccentText, p.Canvas, 4.5, "testo d'accento (data, totale)");
            AlmenoContrasto(p.MutedText, p.Canvas, 3, "testo attenuato");
            AlmenoContrasto(p.InputForeground, p.InputBackground, 7, "campi di testo");
        }

        [Theory]
        [MemberData(nameof(OgniPalette))]
        public void PulsantiEControlliLeggibili(string id)
        {
            var p = P(id);

            AlmenoContrasto(p.AccentActionForeground, p.AccentActionBackground, 4.5, "pulsante principale");
            AlmenoContrasto(p.AccentActionForeground, p.AccentActionPressed, 4.5, "pulsante principale premuto");
            AlmenoContrasto(p.DestructiveForeground, p.DestructiveSurface, 4.5, "pulsante di rimozione");
        }

        [Theory]
        [MemberData(nameof(OgniPalette))]
        public void MessaggioDErroreLeggibileNeiDialoghi(string id)
        {
            // Sfondo dei dialoghi di WinUI nel tema scuro.
            var sfondoDialogo = ColoreRgb.Da("#2B2B2B");

            AlmenoContrasto(P(id).ErrorText, sfondoDialogo, 4.5, "messaggio d'errore");
        }

        [Theory]
        [MemberData(nameof(OgniPalette))]
        public void TestoLeggibileSuOgniCardDiDeposito(string id)
        {
            var p = P(id);

            foreach (var colore in p.Depositi)
                AlmenoContrasto(p.OnAccent, colore, 4.5, "testo della card " + colore);
        }

        [Theory]
        [MemberData(nameof(OgniPalette))]
        public void CardVicineDistinguibili(string id)
        {
            // Le card sono su due colonne: 1-2, 3-4… stanno accanto, 1-3, 2-4… una sopra l'altra.
            // Due colori vicini non devono essere quasi uguali.
            var d = P(id).Depositi;
            for (int i = 0; i < d.Count; i++)
            {
                foreach (int j in new[] { i + 1, i + 2 }.Where(j => j < d.Count))
                {
                    double distanza = Math.Sqrt(
                        Math.Pow(d[i].R - d[j].R, 2) + Math.Pow(d[i].G - d[j].G, 2) + Math.Pow(d[i].B - d[j].B, 2));
                    Assert.True(distanza >= 12, $"Depositi {i + 1} e {j + 1} troppo simili: {d[i]} e {d[j]}");
                }
            }
        }

        // ---------- Il verde non cambia ----------

        [Fact]
        public void Verde_StessiColoriDiPrima()
        {
            var v = Palette.Verde;

            Assert.Equal(ColoreRgb.Da("#141613"), v.Canvas);
            Assert.Equal(ColoreRgb.Da("#1C1F1A"), v.Surface);
            Assert.Equal(ColoreRgb.Da("#F1EEDF"), v.TextPrimary);
            Assert.Equal(ColoreRgb.Da("#A8B58A"), v.AccentText);
            Assert.Equal(ColoreRgb.Da("#657747"), v.AccentActionBackground);
            Assert.Equal(
                new[] { "#43523A", "#3B5146", "#4B4937", "#41464D", "#514359", "#37505A", "#59443A", "#4B5142" },
                v.Depositi.Select(c => c.ToString()));
        }

        // ---------- Colore ----------

        [Theory]
        [InlineData("#000000", "#FFFFFF", 21)]
        [InlineData("#FFFFFF", "#FFFFFF", 1)]
        [InlineData("#767676", "#FFFFFF", 4.54)]
        public void Contrasto_ValoriNoti(string a, string b, double atteso)
        {
            Assert.Equal(atteso, ColoreRgb.Contrasto(ColoreRgb.Da(a), ColoreRgb.Da(b)), 2);
        }

        [Theory]
        [InlineData("")]
        [InlineData("141613")]
        [InlineData("#14161")]
        [InlineData("#GG1613")]
        public void Da_FormatoNonValido(string hex)
        {
            Assert.ThrowsAny<FormatException>(() => ColoreRgb.Da(hex));
        }
    }
}
